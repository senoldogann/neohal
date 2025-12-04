using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeoHal.Services;
using NeoHal.Services.Interfaces;

namespace NeoHal.Desktop.ViewModels;

public partial class BackupViewModel : ViewModelBase
{
    private readonly IBackupService _backupService;
    
    [ObservableProperty]
    private ObservableCollection<YedekKaydi> _yedekler = new();
    
    [ObservableProperty]
    private YedekKaydi? _selectedYedek;
    
    [ObservableProperty]
    private YedekAyarlari _ayarlar = new();
    
    [ObservableProperty]
    private YedekIstatistikleri _istatistikler = new();
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private bool _isBackupInProgress;
    
    [ObservableProperty]
    private string _yedekAciklama = string.Empty;
    
    [ObservableProperty]
    private string _statusMessage = string.Empty;
    
    [ObservableProperty]
    private bool _isSchedulerRunning;
    
    // Zamanlama seçenekleri
    public string[] ZamanlamaSecenekleri { get; } = { "Günlük", "Haftalık", "Aylık" };
    
    [ObservableProperty]
    private int _selectedZamanlamaIndex;

    public BackupViewModel(IBackupService backupService)
    {
        _backupService = backupService;
        _ = InitializeAsync();
    }
    
    // Parameterless constructor for design-time
    public BackupViewModel() : this(new BackupService())
    {
    }

    private async Task InitializeAsync()
    {
        await LoadDataInternalAsync();
    }

    private async Task LoadDataInternalAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Yükleniyor...";
            
            // Yedekleri yükle
            var yedekler = await _backupService.GetAllBackupsAsync();
            Yedekler = new ObservableCollection<YedekKaydi>(yedekler);
            
            // Ayarları yükle
            Ayarlar = await _backupService.GetBackupSettingsAsync();
            SelectedZamanlamaIndex = (int)Ayarlar.Zamanlama;
            
            // İstatistikleri yükle
            Istatistikler = await _backupService.GetBackupStatisticsAsync();
            
            StatusMessage = $"{Yedekler.Count} yedek listelendi";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadData()
    {
        await LoadDataInternalAsync();
    }

    [RelayCommand]
    private async Task CreateBackup()
    {
        try
        {
            IsBackupInProgress = true;
            StatusMessage = "Yedek oluşturuluyor...";
            
            var yedek = await _backupService.CreateBackupAsync(YedekAciklama);
            
            Yedekler.Insert(0, yedek);
            YedekAciklama = string.Empty;
            
            // İstatistikleri güncelle
            Istatistikler = await _backupService.GetBackupStatisticsAsync();
            
            StatusMessage = $"Yedek oluşturuldu: {yedek.DosyaAdi}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Yedekleme hatası: {ex.Message}";
        }
        finally
        {
            IsBackupInProgress = false;
        }
    }

    [RelayCommand]
    private async Task RestoreBackup()
    {
        if (SelectedYedek == null) return;
        
        try
        {
            IsBackupInProgress = true;
            StatusMessage = "Geri yükleme yapılıyor...";
            
            var success = await _backupService.RestoreBackupAsync(SelectedYedek.Id);
            
            if (success)
            {
                StatusMessage = "Veritabanı başarıyla geri yüklendi!";
            }
            else
            {
                StatusMessage = "Geri yükleme başarısız!";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Geri yükleme hatası: {ex.Message}";
        }
        finally
        {
            IsBackupInProgress = false;
        }
    }

    [RelayCommand]
    private async Task DeleteBackup()
    {
        if (SelectedYedek == null) return;
        
        try
        {
            var yedekId = SelectedYedek.Id;
            var success = await _backupService.DeleteBackupAsync(yedekId);
            
            if (success)
            {
                var yedek = Yedekler.FirstOrDefault(y => y.Id == yedekId);
                if (yedek != null)
                {
                    Yedekler.Remove(yedek);
                }
                
                SelectedYedek = null;
                Istatistikler = await _backupService.GetBackupStatisticsAsync();
                
                StatusMessage = "Yedek silindi";
            }
            else
            {
                StatusMessage = "Yedek silinemedi!";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Silme hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task VerifyBackup()
    {
        if (SelectedYedek == null) return;
        
        try
        {
            StatusMessage = "Yedek doğrulanıyor...";
            
            var isValid = await _backupService.VerifyBackupAsync(SelectedYedek.Id);
            
            StatusMessage = isValid ? "✓ Yedek geçerli" : "✗ Yedek bozuk!";
            
            // Yedeği güncelle
            await LoadDataInternalAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Doğrulama hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ExportBackup()
    {
        if (SelectedYedek == null) return;
        
        try
        {
            // Masaüstüne dışa aktar
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var targetPath = Path.Combine(desktopPath, SelectedYedek.DosyaAdi);
            
            var success = await _backupService.ExportBackupAsync(SelectedYedek.Id, targetPath);
            
            if (success)
            {
                StatusMessage = $"Yedek dışa aktarıldı: {targetPath}";
            }
            else
            {
                StatusMessage = "Dışa aktarma başarısız!";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Dışa aktarma hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveSettings()
    {
        try
        {
            Ayarlar.Zamanlama = (YedekZamanlama)SelectedZamanlamaIndex;
            await _backupService.SaveBackupSettingsAsync(Ayarlar);
            StatusMessage = "Ayarlar kaydedildi";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ayar kaydetme hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CleanupOldBackups()
    {
        try
        {
            StatusMessage = "Eski yedekler temizleniyor...";
            
            var count = await _backupService.CleanupOldBackupsAsync(Ayarlar.SaklanacakYedekSayisi);
            
            await LoadDataInternalAsync();
            
            StatusMessage = $"{count} eski yedek temizlendi";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Temizleme hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ToggleScheduler()
    {
        try
        {
            if (IsSchedulerRunning)
            {
                await _backupService.StopAutoBackupSchedulerAsync();
                IsSchedulerRunning = false;
                StatusMessage = "Otomatik yedekleme durduruldu";
            }
            else
            {
                await _backupService.StartAutoBackupSchedulerAsync();
                IsSchedulerRunning = true;
                StatusMessage = "Otomatik yedekleme başlatıldı";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Zamanlayıcı hatası: {ex.Message}";
        }
    }

    // Helper methods for view formatting
    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;
        
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        
        return $"{size:0.##} {sizes[order]}";
    }

    public static string GetDurumRenk(YedekDurumu durum)
    {
        return durum switch
        {
            YedekDurumu.Basarili => "#22C55E",
            YedekDurumu.Hatali => "#EF4444",
            YedekDurumu.Devam => "#F59E0B",
            YedekDurumu.Beklemede => "#6B7280",
            YedekDurumu.Silindi => "#9CA3AF",
            _ => "#6B7280"
        };
    }

    public static string GetTurText(YedekTuru tur)
    {
        return tur switch
        {
            YedekTuru.Manuel => "Manuel",
            YedekTuru.Otomatik => "Otomatik",
            YedekTuru.GunlukPlanlanan => "Günlük",
            YedekTuru.HaftalikPlanlanan => "Haftalık",
            YedekTuru.AylikPlanlanan => "Aylık",
            _ => "Bilinmiyor"
        };
    }
}
