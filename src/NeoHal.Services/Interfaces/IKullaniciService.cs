using NeoHal.Core.Entities;

namespace NeoHal.Services.Interfaces;

/// <summary>
/// Kullanıcı Yönetimi Servisi Interface
/// </summary>
public interface IKullaniciService
{
    // Authentication
    Task<KullaniciGirisSonucu> GirisYapAsync(string kullaniciAdi, string sifre);
    Task CikisYapAsync();
    Task<bool> SifreDegistirAsync(Guid kullaniciId, string eskiSifre, string yeniSifre);
    
    // CRUD
    Task<Kullanici?> GetByIdAsync(Guid id);
    Task<Kullanici?> GetByKullaniciAdiAsync(string kullaniciAdi);
    Task<IEnumerable<Kullanici>> GetAllAsync();
    Task<Kullanici> CreateAsync(KullaniciOlusturDto dto);
    Task<Kullanici> UpdateAsync(KullaniciGuncelleDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> AktiflikDegistirAsync(Guid id, bool aktif);
    
    // Yetkilendirme
    Task<bool> YetkiKontrolAsync(Guid kullaniciId, string yetki);
    Task<List<string>> GetYetkilerAsync(Guid kullaniciId);
    Task<bool> YetkiAtaAsync(Guid kullaniciId, List<string> yetkiler);
    
    // Aktif Kullanıcı
    Kullanici? AktifKullanici { get; }
    bool GirisYapildi { get; }
    
    // Olay
    event EventHandler<Kullanici>? KullaniciGirisYapti;
    event EventHandler? KullaniciCikisYapti;
}

/// <summary>
/// Kullanıcı Giriş Sonucu
/// </summary>
public class KullaniciGirisSonucu
{
    public bool Basarili { get; set; }
    public string Mesaj { get; set; } = string.Empty;
    public Kullanici? Kullanici { get; set; }
    public List<string> Yetkiler { get; set; } = new();
}

/// <summary>
/// Kullanıcı Oluşturma DTO
/// </summary>
public class KullaniciOlusturDto
{
    public string KullaniciAdi { get; set; } = string.Empty;
    public string Sifre { get; set; } = string.Empty;
    public string AdSoyad { get; set; } = string.Empty;
    public string Rol { get; set; } = "OPERATOR";
    public List<string> Yetkiler { get; set; } = new();
}

/// <summary>
/// Kullanıcı Güncelleme DTO
/// </summary>
public class KullaniciGuncelleDto
{
    public Guid Id { get; set; }
    public string? AdSoyad { get; set; }
    public string? Rol { get; set; }
    public List<string>? Yetkiler { get; set; }
    public string? YeniSifre { get; set; }
}

/// <summary>
/// Kullanıcı Rolleri
/// </summary>
public static class KullaniciRolleri
{
    public const string Admin = "ADMIN";
    public const string Operator = "OPERATOR";
    public const string Muhasebe = "MUHASEBE";
    public const string Izleyici = "IZLEYICI";
    
    public static readonly List<string> Tumu = new()
    {
        Admin, Operator, Muhasebe, Izleyici
    };
    
    public static string Aciklama(string rol) => rol switch
    {
        Admin => "Yönetici - Tüm yetkiler",
        Operator => "Operatör - Giriş/Satış işlemleri",
        Muhasebe => "Muhasebe - Finansal işlemler",
        Izleyici => "İzleyici - Sadece görüntüleme",
        _ => "Bilinmeyen"
    };
}

/// <summary>
/// Sistem Yetkileri
/// </summary>
public static class Yetkiler
{
    // Modüller
    public const string CariHesap_Gorme = "CARI_GORME";
    public const string CariHesap_Ekleme = "CARI_EKLEME";
    public const string CariHesap_Duzenleme = "CARI_DUZENLEME";
    public const string CariHesap_Silme = "CARI_SILME";
    
    public const string Urun_Gorme = "URUN_GORME";
    public const string Urun_Ekleme = "URUN_EKLEME";
    public const string Urun_Duzenleme = "URUN_DUZENLEME";
    public const string Urun_Silme = "URUN_SILME";
    
    public const string GirisIrsaliye_Gorme = "GIRIS_GORME";
    public const string GirisIrsaliye_Ekleme = "GIRIS_EKLEME";
    public const string GirisIrsaliye_Duzenleme = "GIRIS_DUZENLEME";
    public const string GirisIrsaliye_Silme = "GIRIS_SILME";
    public const string GirisIrsaliye_Onaylama = "GIRIS_ONAYLAMA";
    
    public const string SatisFatura_Gorme = "SATIS_GORME";
    public const string SatisFatura_Ekleme = "SATIS_EKLEME";
    public const string SatisFatura_Duzenleme = "SATIS_DUZENLEME";
    public const string SatisFatura_Silme = "SATIS_SILME";
    public const string SatisFatura_Onaylama = "SATIS_ONAYLAMA";
    
    public const string Kasa_Gorme = "KASA_GORME";
    public const string Kasa_Tahsilat = "KASA_TAHSILAT";
    public const string Kasa_Odeme = "KASA_ODEME";
    
    public const string Rapor_Gorme = "RAPOR_GORME";
    public const string Rapor_Detay = "RAPOR_DETAY";
    public const string Rapor_Export = "RAPOR_EXPORT";
    
    public const string EFatura_Gorme = "EFATURA_GORME";
    public const string EFatura_Gonderme = "EFATURA_GONDERME";
    
    public const string HKS_Gorme = "HKS_GORME";
    public const string HKS_Gonderme = "HKS_GONDERME";
    
    public const string Ayarlar_Gorme = "AYARLAR_GORME";
    public const string Ayarlar_Duzenleme = "AYARLAR_DUZENLEME";
    
    public const string Kullanici_Gorme = "KULLANICI_GORME";
    public const string Kullanici_Yonetme = "KULLANICI_YONETME";
    
    public const string Yedekleme_Gorme = "YEDEKLEME_GORME";
    public const string Yedekleme_Alma = "YEDEKLEME_ALMA";
    public const string Yedekleme_Yukleme = "YEDEKLEME_YUKLEME";
    
    /// <summary>
    /// Role göre varsayılan yetkiler
    /// </summary>
    public static List<string> RolYetkileri(string rol) => rol switch
    {
        KullaniciRolleri.Admin => TumYetkiler(),
        KullaniciRolleri.Operator => new List<string>
        {
            CariHesap_Gorme, CariHesap_Ekleme, CariHesap_Duzenleme,
            Urun_Gorme,
            GirisIrsaliye_Gorme, GirisIrsaliye_Ekleme, GirisIrsaliye_Duzenleme,
            SatisFatura_Gorme, SatisFatura_Ekleme, SatisFatura_Duzenleme,
            Kasa_Gorme,
            Rapor_Gorme
        },
        KullaniciRolleri.Muhasebe => new List<string>
        {
            CariHesap_Gorme, CariHesap_Ekleme, CariHesap_Duzenleme,
            Urun_Gorme,
            GirisIrsaliye_Gorme,
            SatisFatura_Gorme,
            Kasa_Gorme, Kasa_Tahsilat, Kasa_Odeme,
            Rapor_Gorme, Rapor_Detay, Rapor_Export,
            EFatura_Gorme, EFatura_Gonderme,
            HKS_Gorme
        },
        KullaniciRolleri.Izleyici => new List<string>
        {
            CariHesap_Gorme,
            Urun_Gorme,
            GirisIrsaliye_Gorme,
            SatisFatura_Gorme,
            Kasa_Gorme,
            Rapor_Gorme
        },
        _ => new List<string>()
    };
    
    public static List<string> TumYetkiler() => new()
    {
        CariHesap_Gorme, CariHesap_Ekleme, CariHesap_Duzenleme, CariHesap_Silme,
        Urun_Gorme, Urun_Ekleme, Urun_Duzenleme, Urun_Silme,
        GirisIrsaliye_Gorme, GirisIrsaliye_Ekleme, GirisIrsaliye_Duzenleme, GirisIrsaliye_Silme, GirisIrsaliye_Onaylama,
        SatisFatura_Gorme, SatisFatura_Ekleme, SatisFatura_Duzenleme, SatisFatura_Silme, SatisFatura_Onaylama,
        Kasa_Gorme, Kasa_Tahsilat, Kasa_Odeme,
        Rapor_Gorme, Rapor_Detay, Rapor_Export,
        EFatura_Gorme, EFatura_Gonderme,
        HKS_Gorme, HKS_Gonderme,
        Ayarlar_Gorme, Ayarlar_Duzenleme,
        Kullanici_Gorme, Kullanici_Yonetme,
        Yedekleme_Gorme, Yedekleme_Alma, Yedekleme_Yukleme
    };
}
