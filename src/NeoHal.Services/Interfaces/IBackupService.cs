using NeoHal.Core.Common;
using NeoHal.Core.Entities;

namespace NeoHal.Services.Interfaces;

/// <summary>
/// Veritabanı yedekleme ve geri yükleme servisi
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Manuel yedek oluşturur
    /// </summary>
    Task<YedekKaydi> CreateBackupAsync(string aciklama = "", CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Otomatik yedek oluşturur
    /// </summary>
    Task<YedekKaydi> CreateAutoBackupAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Yedekten geri yükler
    /// </summary>
    Task<bool> RestoreBackupAsync(Guid yedekId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Yedek dosyasından geri yükler
    /// </summary>
    Task<bool> RestoreFromFileAsync(string filePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Tüm yedekleri listeler
    /// </summary>
    Task<IEnumerable<YedekKaydi>> GetAllBackupsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Belirli bir yedeği getirir
    /// </summary>
    Task<YedekKaydi?> GetBackupByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Yedeği siler
    /// </summary>
    Task<bool> DeleteBackupAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Eski yedekleri temizler
    /// </summary>
    Task<int> CleanupOldBackupsAsync(int keepCount = 10, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Yedeği başka bir konuma dışa aktarır
    /// </summary>
    Task<bool> ExportBackupAsync(Guid yedekId, string targetPath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Yedekleme ayarlarını getirir
    /// </summary>
    Task<YedekAyarlari> GetBackupSettingsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Yedekleme ayarlarını kaydeder
    /// </summary>
    Task SaveBackupSettingsAsync(YedekAyarlari ayarlar, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Yedekleme istatistiklerini getirir
    /// </summary>
    Task<YedekIstatistikleri> GetBackupStatisticsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Yedek dosyasını doğrular
    /// </summary>
    Task<bool> VerifyBackupAsync(Guid yedekId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Otomatik yedekleme zamanlamasını başlatır
    /// </summary>
    Task StartAutoBackupSchedulerAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Otomatik yedekleme zamanlamasını durdurur
    /// </summary>
    Task StopAutoBackupSchedulerAsync();
}

/// <summary>
/// Yedek kaydı entity'si
/// </summary>
public class YedekKaydi : BaseEntity
{
    public string DosyaAdi { get; set; } = string.Empty;
    public string DosyaYolu { get; set; } = string.Empty;
    public long DosyaBoyutu { get; set; }
    public string Aciklama { get; set; } = string.Empty;
    public YedekTuru Tur { get; set; } = YedekTuru.Manuel;
    public YedekDurumu Durum { get; set; } = YedekDurumu.Basarili;
    public string? HataMesaji { get; set; }
    public bool Dogrulandi { get; set; }
    public DateTime? DogrulamaTarihi { get; set; }
    public string Checksum { get; set; } = string.Empty;
}

/// <summary>
/// Yedek türü
/// </summary>
public enum YedekTuru
{
    Manuel,
    Otomatik,
    GunlukPlanlanan,
    HaftalikPlanlanan,
    AylikPlanlanan
}

/// <summary>
/// Yedek durumu
/// </summary>
public enum YedekDurumu
{
    Beklemede,
    Devam,
    Basarili,
    Hatali,
    Silindi
}

/// <summary>
/// Yedekleme ayarları
/// </summary>
public class YedekAyarlari
{
    public string YedekKlasoru { get; set; } = "Yedekler";
    public bool OtomatikYedeklemeAktif { get; set; } = true;
    public YedekZamanlama Zamanlama { get; set; } = YedekZamanlama.Gunluk;
    public TimeOnly YedeklemeSaati { get; set; } = new TimeOnly(2, 0); // Gece 02:00
    public int SaklanacakYedekSayisi { get; set; } = 30;
    public bool SikistirmaAktif { get; set; } = true;
    public bool FtpYedeklemeAktif { get; set; } = false;
    public string FtpSunucu { get; set; } = string.Empty;
    public string FtpKullanici { get; set; } = string.Empty;
    public string FtpSifre { get; set; } = string.Empty;
    public string FtpKlasor { get; set; } = "/yedekler";
    public bool EmailBildirimAktif { get; set; } = false;
    public string BildirimEmail { get; set; } = string.Empty;
}

/// <summary>
/// Yedek zamanlama türü
/// </summary>
public enum YedekZamanlama
{
    Gunluk,
    Haftalik,
    Aylik
}

/// <summary>
/// Yedekleme istatistikleri
/// </summary>
public class YedekIstatistikleri
{
    public int ToplamYedekSayisi { get; set; }
    public int BasariliYedekSayisi { get; set; }
    public int HataliYedekSayisi { get; set; }
    public long ToplamBoyut { get; set; }
    public DateTime? SonYedekTarihi { get; set; }
    public DateTime? SonBasariliYedekTarihi { get; set; }
    public YedekKaydi? SonYedek { get; set; }
    public double OrtalamaBoyut { get; set; }
    public int BugunYedekSayisi { get; set; }
    public int BuHaftaYedekSayisi { get; set; }
    public int BuAyYedekSayisi { get; set; }
}
