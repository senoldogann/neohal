using NeoHal.Core.Common;
using NeoHal.Core.Enums;

namespace NeoHal.Core.Entities;

/// <summary>
/// Kasa Stok Durumu - Her cari için anlık kasa durumu
/// </summary>
public class KasaStokDurumu : BaseEntity
{
    public Guid CariId { get; set; }
    public Guid KapTipiId { get; set; }
    
    // Anlık Durum
    public int DoluKasaAdet { get; set; } = 0;  // Bizde olan carinin dolu kasası
    public int BosKasaAdet { get; set; } = 0;   // Bizde olan carinin boş kasası
    
    // Rehin Bilgisi
    public decimal RehinToplam { get; set; } = 0; // Toplam rehin bedeli
    
    // Navigation
    public virtual CariHesap Cari { get; set; } = null!;
    public virtual KapTipi KapTipi { get; set; } = null!;
    public virtual ICollection<KasaHareket> Hareketler { get; set; } = new List<KasaHareket>();
}

/// <summary>
/// Kasa Hareket - Tüm kasa giriş/çıkışları
/// </summary>
public class KasaHareket : BaseEntity
{
    public Guid KasaStokId { get; set; }
    public KasaHareketTipi HareketTipi { get; set; }
    public int Adet { get; set; }
    public DateTime Tarih { get; set; } = DateTime.Now;
    
    // Referans Belge
    public string? ReferansBelgeTipi { get; set; } // "Irsaliye", "Fatura", "RehinFisi"
    public Guid? ReferansBelgeId { get; set; }
    
    public string? Aciklama { get; set; }
    
    // Navigation
    public virtual KasaStokDurumu KasaStok { get; set; } = null!;
}

/// <summary>
/// Rehin Fişi - Kasa rehin alma/iade işlemleri
/// </summary>
public class RehinFisi : BaseEntity
{
    public string FisNo { get; set; } = string.Empty;
    public DateTime Tarih { get; set; } = DateTime.Today;
    
    public Guid CariId { get; set; }
    public Guid KapTipiId { get; set; }
    
    // İşlem Bilgileri
    public bool IslemTipiAl { get; set; } // true: Al, false: İade
    public int KasaAdet { get; set; }
    public decimal BirimBedel { get; set; }
    public decimal ToplamTutar { get; set; }
    
    // Ödeme
    public bool Odendi { get; set; } = false;
    public DateTime? OdemeTarihi { get; set; }
    
    public string? Aciklama { get; set; }
    
    // Navigation
    public virtual CariHesap Cari { get; set; } = null!;
    public virtual KapTipi KapTipi { get; set; } = null!;
}
