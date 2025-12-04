using NeoHal.Core.Common;

namespace NeoHal.Core.Entities;

/// <summary>
/// Lot/Parti Takibi - Traceability için
/// </summary>
public class LotBilgisi : BaseEntity
{
    public string LotNo { get; set; } = string.Empty;
    
    // Kaynak Bilgisi
    public Guid GirisIrsaliyesiId { get; set; }
    public Guid GirisKalemId { get; set; }
    public Guid MustahsilId { get; set; }
    
    // Ürün Bilgisi
    public Guid UrunId { get; set; }
    public Guid KapTipiId { get; set; }
    public int KapAdet { get; set; }
    public decimal BrutKg { get; set; }
    public decimal NetKg { get; set; }
    
    // Menşei
    public string? UretimBolgesi { get; set; }
    public DateTime? HasatTarihi { get; set; }
    
    // HKS Bilgileri
    public string? HksKunyeNo { get; set; }
    public DateTime? HksBildirimTarihi { get; set; }
    
    // Satış Takibi
    public decimal SatilanKg { get; set; } = 0;
    public decimal KalanKg => NetKg - SatilanKg;
    public bool Tukendi => KalanKg <= 0;
    
    // Navigation
    public virtual GirisIrsaliyesi GirisIrsaliyesi { get; set; } = null!;
    public virtual GirisIrsaliyesiKalem GirisKalem { get; set; } = null!;
    public virtual CariHesap Mustahsil { get; set; } = null!;
    public virtual Urun Urun { get; set; } = null!;
    public virtual KapTipi KapTipi { get; set; } = null!;
    
    public virtual ICollection<LotSatisHareket> SatisHareketleri { get; set; } = new List<LotSatisHareket>();
}

/// <summary>
/// Lot Satış Hareketi - Hangi lottan ne kadar satıldı
/// </summary>
public class LotSatisHareket : BaseEntity
{
    public Guid LotId { get; set; }
    public Guid SatisFaturasiId { get; set; }
    public Guid SatisKalemId { get; set; }
    
    public decimal SatilanKg { get; set; }
    public int SatilanKap { get; set; }
    public DateTime SatisTarihi { get; set; }
    
    // Navigation
    public virtual LotBilgisi Lot { get; set; } = null!;
    public virtual SatisFaturasi SatisFaturasi { get; set; } = null!;
    public virtual SatisFaturasiKalem SatisKalem { get; set; } = null!;
}
