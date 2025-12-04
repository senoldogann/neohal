using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NeoHal.Services.Interfaces;

namespace NeoHal.Services;

/// <summary>
/// Veritabanı yedekleme ve geri yükleme servisi implementasyonu
/// </summary>
public class BackupService : IBackupService
{
    private readonly ILogger<BackupService>? _logger;
    private readonly string _veritabaniYolu;
    private readonly string _yedekKlasoru;
    private readonly List<YedekKaydi> _yedekler = new();
    private YedekAyarlari _ayarlar = new();
    private CancellationTokenSource? _schedulerCts;
    private Task? _schedulerTask;
    
    private const string AYARLAR_DOSYASI = "backup_settings.json";
    private const string YEDEKLER_INDEX = "backups_index.json";

    public BackupService(ILogger<BackupService>? logger = null)
    {
        _logger = logger;
        
        // Veritabanı yolunu belirle
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        _veritabaniYolu = Path.Combine(baseDir, "Data", "neohal.db");
        _yedekKlasoru = Path.Combine(baseDir, "Yedekler");
        
        // Yedek klasörünü oluştur
        if (!Directory.Exists(_yedekKlasoru))
        {
            Directory.CreateDirectory(_yedekKlasoru);
        }
        
        // Ayarları ve yedek listesini yükle
        LoadSettings();
        LoadBackupIndex();
    }

    public async Task<YedekKaydi> CreateBackupAsync(string aciklama = "", CancellationToken cancellationToken = default)
    {
        return await CreateBackupInternalAsync(YedekTuru.Manuel, aciklama, cancellationToken);
    }

    public async Task<YedekKaydi> CreateAutoBackupAsync(CancellationToken cancellationToken = default)
    {
        return await CreateBackupInternalAsync(YedekTuru.Otomatik, "Otomatik yedekleme", cancellationToken);
    }

    private async Task<YedekKaydi> CreateBackupInternalAsync(YedekTuru tur, string aciklama, CancellationToken cancellationToken)
    {
        var yedek = new YedekKaydi
        {
            Id = Guid.NewGuid(),
            OlusturmaTarihi = DateTime.Now,
            GuncellemeTarihi = DateTime.Now,
            Tur = tur,
            Aciklama = aciklama,
            Durum = YedekDurumu.Devam,
            Aktif = true
        };

        try
        {
            _logger?.LogInformation("Yedekleme başlatılıyor: {Tur}", tur);

            // Dosya adı oluştur
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var dosyaAdi = $"neohal_backup_{timestamp}";
            
            if (_ayarlar.SikistirmaAktif)
            {
                dosyaAdi += ".zip";
            }
            else
            {
                dosyaAdi += ".db";
            }

            yedek.DosyaAdi = dosyaAdi;
            yedek.DosyaYolu = Path.Combine(_yedekKlasoru, dosyaAdi);

            // Veritabanı dosyasının varlığını kontrol et
            if (!File.Exists(_veritabaniYolu))
            {
                // Demo mod - simüle et
                _logger?.LogWarning("Veritabanı dosyası bulunamadı, demo yedek oluşturuluyor");
                
                // Demo yedek dosyası oluştur
                await CreateDemoBackupAsync(yedek.DosyaYolu, cancellationToken);
            }
            else
            {
                // Gerçek yedekleme
                if (_ayarlar.SikistirmaAktif)
                {
                    await CreateCompressedBackupAsync(_veritabaniYolu, yedek.DosyaYolu, cancellationToken);
                }
                else
                {
                    await CopyFileAsync(_veritabaniYolu, yedek.DosyaYolu, cancellationToken);
                }
            }

            // Dosya boyutunu al
            if (File.Exists(yedek.DosyaYolu))
            {
                var fileInfo = new FileInfo(yedek.DosyaYolu);
                yedek.DosyaBoyutu = fileInfo.Length;
            }

            // Checksum hesapla
            yedek.Checksum = await CalculateChecksumAsync(yedek.DosyaYolu, cancellationToken);

            yedek.Durum = YedekDurumu.Basarili;
            
            _yedekler.Add(yedek);
            await SaveBackupIndexAsync();

            _logger?.LogInformation("Yedekleme tamamlandı: {DosyaAdi}, Boyut: {Boyut} bytes", yedek.DosyaAdi, yedek.DosyaBoyutu);

            // FTP yedekleme
            if (_ayarlar.FtpYedeklemeAktif)
            {
                await UploadToFtpAsync(yedek, cancellationToken);
            }

            // Email bildirimi
            if (_ayarlar.EmailBildirimAktif)
            {
                await SendEmailNotificationAsync(yedek, true, cancellationToken);
            }

            // Eski yedekleri temizle
            await CleanupOldBackupsAsync(_ayarlar.SaklanacakYedekSayisi, cancellationToken);

            return yedek;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Yedekleme hatası");
            
            yedek.Durum = YedekDurumu.Hatali;
            yedek.HataMesaji = ex.Message;
            
            if (_ayarlar.EmailBildirimAktif)
            {
                await SendEmailNotificationAsync(yedek, false, cancellationToken);
            }

            throw;
        }
    }

    public async Task<bool> RestoreBackupAsync(Guid yedekId, CancellationToken cancellationToken = default)
    {
        var yedek = _yedekler.FirstOrDefault(y => y.Id == yedekId);
        if (yedek == null)
        {
            _logger?.LogError("Yedek bulunamadı: {YedekId}", yedekId);
            return false;
        }

        return await RestoreFromFileAsync(yedek.DosyaYolu, cancellationToken);
    }

    public async Task<bool> RestoreFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger?.LogError("Yedek dosyası bulunamadı: {FilePath}", filePath);
                return false;
            }

            _logger?.LogInformation("Geri yükleme başlatılıyor: {FilePath}", filePath);

            // Mevcut veritabanının yedeğini al
            var backupBeforeRestore = _veritabaniYolu + ".before_restore";
            if (File.Exists(_veritabaniYolu))
            {
                File.Copy(_veritabaniYolu, backupBeforeRestore, true);
            }

            try
            {
                if (filePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    await ExtractBackupAsync(filePath, _veritabaniYolu, cancellationToken);
                }
                else
                {
                    await CopyFileAsync(filePath, _veritabaniYolu, cancellationToken);
                }

                _logger?.LogInformation("Geri yükleme tamamlandı");
                
                // Geçici yedek dosyasını sil
                if (File.Exists(backupBeforeRestore))
                {
                    File.Delete(backupBeforeRestore);
                }

                return true;
            }
            catch
            {
                // Hata durumunda eski veritabanını geri yükle
                if (File.Exists(backupBeforeRestore))
                {
                    File.Copy(backupBeforeRestore, _veritabaniYolu, true);
                    File.Delete(backupBeforeRestore);
                }
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Geri yükleme hatası");
            return false;
        }
    }

    public Task<IEnumerable<YedekKaydi>> GetAllBackupsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<YedekKaydi>>(
            _yedekler.Where(y => y.Aktif && y.Durum != YedekDurumu.Silindi)
                     .OrderByDescending(y => y.OlusturmaTarihi)
                     .ToList());
    }

    public Task<YedekKaydi?> GetBackupByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_yedekler.FirstOrDefault(y => y.Id == id));
    }

    public async Task<bool> DeleteBackupAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var yedek = _yedekler.FirstOrDefault(y => y.Id == id);
        if (yedek == null) return false;

        try
        {
            // Fiziksel dosyayı sil
            if (File.Exists(yedek.DosyaYolu))
            {
                File.Delete(yedek.DosyaYolu);
            }

            yedek.Durum = YedekDurumu.Silindi;
            yedek.Aktif = false;
            yedek.GuncellemeTarihi = DateTime.Now;
            
            await SaveBackupIndexAsync();

            _logger?.LogInformation("Yedek silindi: {YedekId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Yedek silme hatası: {YedekId}", id);
            return false;
        }
    }

    public async Task<int> CleanupOldBackupsAsync(int keepCount = 10, CancellationToken cancellationToken = default)
    {
        var aktifYedekler = _yedekler
            .Where(y => y.Aktif && y.Durum == YedekDurumu.Basarili)
            .OrderByDescending(y => y.OlusturmaTarihi)
            .ToList();

        if (aktifYedekler.Count <= keepCount)
            return 0;

        var silinecekler = aktifYedekler.Skip(keepCount).ToList();
        var silinenSayisi = 0;

        foreach (var yedek in silinecekler)
        {
            if (await DeleteBackupAsync(yedek.Id, cancellationToken))
            {
                silinenSayisi++;
            }
        }

        _logger?.LogInformation("{Count} eski yedek temizlendi", silinenSayisi);
        return silinenSayisi;
    }

    public async Task<bool> ExportBackupAsync(Guid yedekId, string targetPath, CancellationToken cancellationToken = default)
    {
        var yedek = _yedekler.FirstOrDefault(y => y.Id == yedekId);
        if (yedek == null || !File.Exists(yedek.DosyaYolu))
        {
            return false;
        }

        try
        {
            var targetDir = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            await CopyFileAsync(yedek.DosyaYolu, targetPath, cancellationToken);
            
            _logger?.LogInformation("Yedek dışa aktarıldı: {YedekId} -> {TargetPath}", yedekId, targetPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Yedek dışa aktarma hatası");
            return false;
        }
    }

    public Task<YedekAyarlari> GetBackupSettingsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_ayarlar);
    }

    public async Task SaveBackupSettingsAsync(YedekAyarlari ayarlar, CancellationToken cancellationToken = default)
    {
        _ayarlar = ayarlar;
        
        var ayarlarYolu = Path.Combine(_yedekKlasoru, AYARLAR_DOSYASI);
        var json = JsonSerializer.Serialize(ayarlar, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(ayarlarYolu, json, cancellationToken);
        
        _logger?.LogInformation("Yedekleme ayarları kaydedildi");
    }

    public Task<YedekIstatistikleri> GetBackupStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var aktifYedekler = _yedekler.Where(y => y.Aktif).ToList();
        var bugun = DateTime.Today;
        var haftaBasi = bugun.AddDays(-(int)bugun.DayOfWeek);
        var ayBasi = new DateTime(bugun.Year, bugun.Month, 1);

        var istatistikler = new YedekIstatistikleri
        {
            ToplamYedekSayisi = aktifYedekler.Count,
            BasariliYedekSayisi = aktifYedekler.Count(y => y.Durum == YedekDurumu.Basarili),
            HataliYedekSayisi = aktifYedekler.Count(y => y.Durum == YedekDurumu.Hatali),
            ToplamBoyut = aktifYedekler.Sum(y => y.DosyaBoyutu),
            SonYedekTarihi = aktifYedekler.MaxBy(y => y.OlusturmaTarihi)?.OlusturmaTarihi,
            SonBasariliYedekTarihi = aktifYedekler.Where(y => y.Durum == YedekDurumu.Basarili)
                                                  .MaxBy(y => y.OlusturmaTarihi)?.OlusturmaTarihi,
            SonYedek = aktifYedekler.MaxBy(y => y.OlusturmaTarihi),
            OrtalamaBoyut = aktifYedekler.Count > 0 ? aktifYedekler.Average(y => y.DosyaBoyutu) : 0,
            BugunYedekSayisi = aktifYedekler.Count(y => y.OlusturmaTarihi.Date == bugun),
            BuHaftaYedekSayisi = aktifYedekler.Count(y => y.OlusturmaTarihi >= haftaBasi),
            BuAyYedekSayisi = aktifYedekler.Count(y => y.OlusturmaTarihi >= ayBasi)
        };

        return Task.FromResult(istatistikler);
    }

    public async Task<bool> VerifyBackupAsync(Guid yedekId, CancellationToken cancellationToken = default)
    {
        var yedek = _yedekler.FirstOrDefault(y => y.Id == yedekId);
        if (yedek == null || !File.Exists(yedek.DosyaYolu))
        {
            return false;
        }

        try
        {
            // Checksum doğrula
            var currentChecksum = await CalculateChecksumAsync(yedek.DosyaYolu, cancellationToken);
            var isValid = currentChecksum == yedek.Checksum;

            yedek.Dogrulandi = isValid;
            yedek.DogrulamaTarihi = DateTime.Now;
            yedek.GuncellemeTarihi = DateTime.Now;
            
            await SaveBackupIndexAsync();

            _logger?.LogInformation("Yedek doğrulaması: {YedekId} - {Sonuc}", yedekId, isValid ? "Geçerli" : "Geçersiz");
            return isValid;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Yedek doğrulama hatası: {YedekId}", yedekId);
            return false;
        }
    }

    public Task StartAutoBackupSchedulerAsync(CancellationToken cancellationToken = default)
    {
        if (!_ayarlar.OtomatikYedeklemeAktif)
        {
            _logger?.LogInformation("Otomatik yedekleme devre dışı");
            return Task.CompletedTask;
        }

        _schedulerCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _schedulerTask = RunSchedulerAsync(_schedulerCts.Token);

        _logger?.LogInformation("Otomatik yedekleme zamanlayıcısı başlatıldı");
        return Task.CompletedTask;
    }

    public Task StopAutoBackupSchedulerAsync()
    {
        _schedulerCts?.Cancel();
        _logger?.LogInformation("Otomatik yedekleme zamanlayıcısı durduruldu");
        return Task.CompletedTask;
    }

    private async Task RunSchedulerAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var simdikiSaat = TimeOnly.FromDateTime(DateTime.Now);
                var hedefSaat = _ayarlar.YedeklemeSaati;
                
                // Bugün yedekleme yapılmış mı kontrol et
                var bugunYedekVar = _yedekler.Any(y => 
                    y.OlusturmaTarihi.Date == DateTime.Today && 
                    y.Tur == YedekTuru.Otomatik &&
                    y.Durum == YedekDurumu.Basarili);

                if (!bugunYedekVar && simdikiSaat >= hedefSaat)
                {
                    // Zamanlama kontrolü
                    var yapilacakMi = _ayarlar.Zamanlama switch
                    {
                        YedekZamanlama.Gunluk => true,
                        YedekZamanlama.Haftalik => DateTime.Now.DayOfWeek == DayOfWeek.Sunday,
                        YedekZamanlama.Aylik => DateTime.Now.Day == 1,
                        _ => false
                    };

                    if (yapilacakMi)
                    {
                        _logger?.LogInformation("Zamanlanmış yedekleme başlatılıyor");
                        await CreateAutoBackupAsync(cancellationToken);
                    }
                }

                // Her 5 dakikada bir kontrol et
                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Zamanlayıcı hatası");
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }
    }

    #region Helper Methods

    private void LoadSettings()
    {
        var ayarlarYolu = Path.Combine(_yedekKlasoru, AYARLAR_DOSYASI);
        if (File.Exists(ayarlarYolu))
        {
            try
            {
                var json = File.ReadAllText(ayarlarYolu);
                _ayarlar = JsonSerializer.Deserialize<YedekAyarlari>(json) ?? new YedekAyarlari();
            }
            catch
            {
                _ayarlar = new YedekAyarlari();
            }
        }
    }

    private void LoadBackupIndex()
    {
        var indexYolu = Path.Combine(_yedekKlasoru, YEDEKLER_INDEX);
        if (File.Exists(indexYolu))
        {
            try
            {
                var json = File.ReadAllText(indexYolu);
                var yedekler = JsonSerializer.Deserialize<List<YedekKaydi>>(json);
                if (yedekler != null)
                {
                    _yedekler.AddRange(yedekler);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Yedek index yüklenemedi");
            }
        }
    }

    private async Task SaveBackupIndexAsync()
    {
        var indexYolu = Path.Combine(_yedekKlasoru, YEDEKLER_INDEX);
        var json = JsonSerializer.Serialize(_yedekler, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(indexYolu, json);
    }

    private async Task CreateDemoBackupAsync(string targetPath, CancellationToken cancellationToken)
    {
        // Demo için basit bir dosya oluştur
        var demoContent = $"NeoHal Demo Backup - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        
        if (targetPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            using var archive = ZipFile.Open(targetPath, ZipArchiveMode.Create);
            var entry = archive.CreateEntry("demo_backup.txt");
            using var writer = new StreamWriter(entry.Open());
            await writer.WriteAsync(demoContent);
        }
        else
        {
            await File.WriteAllTextAsync(targetPath, demoContent, cancellationToken);
        }
    }

    private static async Task CreateCompressedBackupAsync(string sourcePath, string targetPath, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            using var archive = ZipFile.Open(targetPath, ZipArchiveMode.Create);
            archive.CreateEntryFromFile(sourcePath, Path.GetFileName(sourcePath), CompressionLevel.Optimal);
        }, cancellationToken);
    }

    private static async Task ExtractBackupAsync(string zipPath, string targetPath, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            using var archive = ZipFile.OpenRead(zipPath);
            var entry = archive.Entries.FirstOrDefault();
            if (entry != null)
            {
                entry.ExtractToFile(targetPath, true);
            }
        }, cancellationToken);
    }

    private static async Task CopyFileAsync(string sourcePath, string targetPath, CancellationToken cancellationToken)
    {
        const int bufferSize = 81920; // 80 KB buffer
        
        using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var destinationStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        
        await sourceStream.CopyToAsync(destinationStream, bufferSize, cancellationToken);
    }

    private static async Task<string> CalculateChecksumAsync(string filePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
            return string.Empty;

        using var sha256 = SHA256.Create();
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
        
        var hash = await sha256.ComputeHashAsync(stream, cancellationToken);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private Task UploadToFtpAsync(YedekKaydi yedek, CancellationToken cancellationToken)
    {
        // FTP yükleme - gerçek implementasyon için FTP client gerekir
        _logger?.LogInformation("FTP yükleme simüle ediliyor: {DosyaAdi}", yedek.DosyaAdi);
        return Task.CompletedTask;
    }

    private Task SendEmailNotificationAsync(YedekKaydi yedek, bool success, CancellationToken cancellationToken)
    {
        // Email bildirimi - gerçek implementasyon için SMTP client gerekir
        _logger?.LogInformation("Email bildirimi simüle ediliyor: {DosyaAdi}, Başarılı: {Success}", yedek.DosyaAdi, success);
        return Task.CompletedTask;
    }

    #endregion
}
