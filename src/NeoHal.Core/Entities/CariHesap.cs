using NeoHal.Core.Common;
using NeoHal.Core.Enums;

namespace NeoHal.Core.Entities;

/// <summary>
/// Cari Hesap - Müstahsil, Komisyoncu, Sevkiyatçı, Alıcı, Nakliyeci, Şube
/// 
/// Şube desteği: CariTipi = Sube olan kayıtlar AnaCariId ile 
/// üst sevkiyatçıya/firmaya bağlanır.
/// </summary>
public class CariHesap : SoftDeleteEntity
{
    public string Kod { get; set; } = string.Empty;
    public string Unvan { get; set; } = string.Empty;
    public CariTipi CariTipi { get; set; }
    public CariTipiDetay? CariTipiDetay { get; set; }
    
    // Şube İlişkisi - Sadece CariTipi = Sube için
    /// <summary>
    /// Şube ise bağlı olduğu ana cari (Sevkiyatçı)
    /// </summary>
    public Guid? AnaCariId { get; set; }
    
    // Kimlik Bilgileri
    public string? TcKimlikNo { get; set; }
    public string? VergiNo { get; set; }
    public string? VergiDairesi { get; set; }
    
    // İletişim Bilgileri
    public string? Telefon { get; set; }
    public string? Telefon2 { get; set; }
    public string? Email { get; set; }
    public string? Adres { get; set; }
    
    // Lokasyon
    public int? IlId { get; set; }
    public int? IlceId { get; set; }
    
    // HKS Bilgileri
    public string? HksSicilNo { get; set; }
    
    // Mali Bilgiler
    public decimal Bakiye { get; set; } = 0;
    public decimal KasaBakiye { get; set; } = 0; // Kasa rehin bakiyesi
    
    // Risk ve Limit Bilgileri
    public decimal RiskLimiti { get; set; } = 0; // Maksimum borç limiti (TL)
    public int RehinLimiti { get; set; } = 0; // Maksimum verilebilecek boş kasa
    public int MevcutRehinKasa { get; set; } = 0; // Şu an zimmetinde olan kasa
    public int VarsayilanVadeGun { get; set; } = 30;
    public bool CekKabulEder { get; set; } = true;
    public bool SenetKabulEder { get; set; } = false;
    
    // Blokaj
    public bool SatisBlokajli { get; set; } = false;
    public string? BlokajNedeni { get; set; }
    
    // Navigation Properties
    public virtual CariHesap? AnaCari { get; set; }
    public virtual ICollection<CariHesap> Subeler { get; set; } = new List<CariHesap>();
    public virtual Il? Il { get; set; }
    public virtual Ilce? Ilce { get; set; }
    public virtual ICollection<GirisIrsaliyesi> GirisIrsaliyeleri { get; set; } = new List<GirisIrsaliyesi>();
    public virtual ICollection<SatisFaturasi> SatisFaturalari { get; set; } = new List<SatisFaturasi>();
    public virtual ICollection<CariHareket> CariHareketler { get; set; } = new List<CariHareket>();
    public virtual ICollection<KasaStokDurumu> KasaStokDurumlari { get; set; } = new List<KasaStokDurumu>();
}

/// <summary>
/// İl
/// </summary>
public class Il
{
    public int Id { get; set; }
    public int PlakaKodu { get; set; }
    public string Ad { get; set; } = string.Empty;
    
    public virtual ICollection<Ilce> Ilceler { get; set; } = new List<Ilce>();
}

/// <summary>
/// İlçe
/// </summary>
public class Ilce
{
    public int Id { get; set; }
    public int IlId { get; set; }
    public string Ad { get; set; } = string.Empty;
    
    public virtual Il Il { get; set; } = null!;
}
