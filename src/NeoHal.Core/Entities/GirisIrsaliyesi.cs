using NeoHal.Core.Common;
using NeoHal.Core.Enums;

namespace NeoHal.Core.Entities;

/// <summary>
/// Giriş İrsaliyesi - Müstahsilden gelen mal
/// </summary>
public class GirisIrsaliyesi : SoftDeleteEntity
{
    public string IrsaliyeNo { get; set; } = string.Empty;
    public DateTime Tarih { get; set; } = DateTime.Today;
    
    // İlişkili Cariler
    public Guid MustahsilId { get; set; }  // Malı getiren üretici
    public Guid? SevkiyatciId { get; set; } // Malı gönderen (varsa)
    public Guid? NakliyeciId { get; set; }  // Taşıyan nakliyeci (varsa)
    
    // Araç Bilgileri
    public string? Plaka { get; set; }
    
    // HKS Bilgileri
    public string? KunyeNo { get; set; }
    public string? HksBildirimNo { get; set; }
    
    // Toplam Bilgiler
    public decimal ToplamBrut { get; set; }
    public decimal ToplamDara { get; set; }
    public decimal ToplamNet { get; set; }
    public int ToplamKapAdet { get; set; }
    
    // Durum
    public BelgeDurumu Durum { get; set; } = BelgeDurumu.Taslak;
    
    // Notlar
    public string? Aciklama { get; set; }
    
    // Navigation
    public virtual CariHesap Mustahsil { get; set; } = null!;
    public virtual CariHesap? Sevkiyatci { get; set; }
    public virtual CariHesap? Nakliyeci { get; set; }
    public virtual ICollection<GirisIrsaliyesiKalem> Kalemler { get; set; } = new List<GirisIrsaliyesiKalem>();
}

/// <summary>
/// Giriş İrsaliyesi Kalemi
/// </summary>
public class GirisIrsaliyesiKalem : BaseEntity
{
    public Guid IrsaliyeId { get; set; }
    public Guid UrunId { get; set; }
    public Guid KapTipiId { get; set; }
    
    /// <summary>
    /// Hal alışında: Bu satırdaki malı aldığımız komisyoncu
    /// Müstahsil irsaliyesinde: null
    /// </summary>
    public Guid? KomisyoncuId { get; set; }
    
    // Miktar Bilgileri
    public int KapAdet { get; set; }
    public decimal BrutKg { get; set; }
    public decimal DaraKg { get; set; }
    public decimal NetKg { get; set; }
    
    // Fiyat (opsiyonel - satış anında da belirlenebilir)
    public decimal? BirimFiyat { get; set; }
    
    // Satış Takibi - FIFO için önemli
    public int KalanKapAdet { get; set; }  // Henüz satılmamış kap adedi
    public decimal KalanKg { get; set; }    // Henüz satılmamış kg
    
    // Navigation
    public virtual GirisIrsaliyesi Irsaliye { get; set; } = null!;
    public virtual Urun Urun { get; set; } = null!;
    public virtual KapTipi KapTipi { get; set; } = null!;
    public virtual CariHesap? Komisyoncu { get; set; }
    public virtual ICollection<SatisFaturasiKalem> SatisKalemleri { get; set; } = new List<SatisFaturasiKalem>();
}
