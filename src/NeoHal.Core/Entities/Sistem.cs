using NeoHal.Core.Common;
using NeoHal.Core.Enums;

namespace NeoHal.Core.Entities;

/// <summary>
/// Sync Log - Offline senkronizasyon takibi
/// </summary>
public class SyncLog
{
    public long Id { get; set; }
    public string TabloAdi { get; set; } = string.Empty;
    public Guid KayitId { get; set; }
    public string IslemTipi { get; set; } = string.Empty; // INSERT, UPDATE, DELETE
    public DateTime YerelTarih { get; set; } = DateTime.UtcNow;
    public DateTime? SyncTarih { get; set; }
    public SyncDurumu Durum { get; set; } = SyncDurumu.Beklemede;
    public string? HataMesaji { get; set; }
    public string? JsonData { get; set; } // Değişiklik verisi
}

/// <summary>
/// Uygulama Ayarları
/// </summary>
public class Ayar
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Tip { get; set; } = "STRING"; // STRING, INT, BOOL, DECIMAL, JSON
    public string? Aciklama { get; set; }
}

/// <summary>
/// Kullanıcı
/// </summary>
public class Kullanici : BaseEntity
{
    public string KullaniciAdi { get; set; } = string.Empty;
    public string SifreHash { get; set; } = string.Empty;
    public string AdSoyad { get; set; } = string.Empty;
    public string Rol { get; set; } = "OPERATOR"; // ADMIN, OPERATOR, MUHASEBE, IZLEYICI
    public string? Yetkiler { get; set; } // JSON formatında yetkiler
    public DateTime? SonGirisTarihi { get; set; }
}

/// <summary>
/// Firma Bilgileri (Tek kayıt)
/// </summary>
public class Firma
{
    public int Id { get; set; } = 1;
    public string Unvan { get; set; } = string.Empty;
    public string? VergiNo { get; set; }
    public string? VergiDairesi { get; set; }
    public string? Adres { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? WebSite { get; set; }
    
    // Hal Bilgileri
    public string? HalAdi { get; set; }
    public string? HksKullaniciAdi { get; set; }
    public string? HksSifre { get; set; } // Şifrelenmiş
    
    // Varsayılan Oranlar
    public decimal VarsayilanRusumOrani { get; set; } = 8;
    public decimal VarsayilanKomisyonOrani { get; set; } = 8;
    public decimal VarsayilanStopajOrani { get; set; } = 4;
}
