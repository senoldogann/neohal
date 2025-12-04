using NeoHal.Core.Common;
using NeoHal.Core.Enums;

namespace NeoHal.Core.Entities;

/// <summary>
/// Cari Hareket - Borç/Alacak hareketleri
/// </summary>
public class CariHareket : BaseEntity
{
    public Guid CariId { get; set; }
    public CariHareketTipi HareketTipi { get; set; }
    public decimal Tutar { get; set; }
    public DateTime Tarih { get; set; } = DateTime.Now;
    
    // Referans Belge
    public string? ReferansBelgeTipi { get; set; } // "Fatura", "Tahsilat", "Odeme"
    public Guid? ReferansBelgeId { get; set; }
    
    public string? Aciklama { get; set; }
    
    // Navigation
    public virtual CariHesap Cari { get; set; } = null!;
}

/// <summary>
/// Kasa Hesabı - Nakit akışı
/// </summary>
public class KasaHesabi : BaseEntity
{
    public DateTime Tarih { get; set; } = DateTime.Now;
    public bool GirisHareketi { get; set; } // true: Giriş, false: Çıkış
    public decimal Tutar { get; set; }
    public OdemeTuru OdemeTuru { get; set; } = OdemeTuru.Nakit;
    
    public Guid? CariId { get; set; }
    
    // Referans
    public string? ReferansBelgeTipi { get; set; }
    public Guid? ReferansBelgeId { get; set; }
    
    public string? Aciklama { get; set; }
    
    // Navigation
    public virtual CariHesap? Cari { get; set; }
}

/// <summary>
/// Çek/Senet
/// </summary>
public class CekSenet : BaseEntity
{
    public bool CekMi { get; set; } = true; // true: Çek, false: Senet
    public string BelgeNo { get; set; } = string.Empty;
    public DateTime VadeTarihi { get; set; }
    public decimal Tutar { get; set; }
    
    public Guid CariId { get; set; }
    
    // Banka Bilgileri (Çek için)
    public string? BankaAdi { get; set; }
    public string? SubeAdi { get; set; }
    public string? HesapNo { get; set; }
    
    // Durum
    public CekSenetDurumu Durum { get; set; } = CekSenetDurumu.Beklemede;
    public DateTime? TahsilTarihi { get; set; }
    
    public string? Aciklama { get; set; }
    
    // Navigation
    public virtual CariHesap Cari { get; set; } = null!;
}
