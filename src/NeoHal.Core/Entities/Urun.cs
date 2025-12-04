using NeoHal.Core.Common;
using NeoHal.Core.Enums;

namespace NeoHal.Core.Entities;

/// <summary>
/// Ürün Grubu (Sebze, Meyve vb.)
/// </summary>
public class UrunGrubu : BaseEntity
{
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    
    public virtual ICollection<Urun> Urunler { get; set; } = new List<Urun>();
}

/// <summary>
/// Ürün (Domates, Biber, Elma vb.)
/// </summary>
public class Urun : SoftDeleteEntity
{
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    
    public Guid GrupId { get; set; }
    
    // HKS Bilgileri
    public string? HksUrunKodu { get; set; }
    
    // Birim ve Oranlar
    public BirimTuru Birim { get; set; } = BirimTuru.Kilogram;
    public decimal KdvOrani { get; set; } = 1; // %1
    public decimal RusumOrani { get; set; } = 8; // %8
    public decimal StopajOrani { get; set; } = 4; // %4
    
    // Navigation
    public virtual UrunGrubu Grup { get; set; } = null!;
    public virtual ICollection<UrunKapEslestirme> KapEslestirmeleri { get; set; } = new List<UrunKapEslestirme>();
}

/// <summary>
/// Kap Tipi (Plastik Kasa, Tahta Kasa, Karton Koli vb.)
/// </summary>
public class KapTipi : BaseEntity
{
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public decimal DaraAgirlik { get; set; } // KG cinsinden
    public decimal RehinBedeli { get; set; } // TL cinsinden
    
    public virtual ICollection<UrunKapEslestirme> UrunEslestirmeleri { get; set; } = new List<UrunKapEslestirme>();
}

/// <summary>
/// Ürün-Kap Eşleştirmesi (Hangi ürün hangi kapta gelebilir)
/// </summary>
public class UrunKapEslestirme : BaseEntity
{
    public Guid UrunId { get; set; }
    public Guid KapTipiId { get; set; }
    public bool Varsayilan { get; set; } = false;
    public decimal OrtalamaAgirlik { get; set; } // Bu kaptaki ortalama net ağırlık
    
    public virtual Urun Urun { get; set; } = null!;
    public virtual KapTipi KapTipi { get; set; } = null!;
}
