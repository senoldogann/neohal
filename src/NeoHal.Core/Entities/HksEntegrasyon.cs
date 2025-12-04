using NeoHal.Core.Common;
using NeoHal.Core.Enums;

namespace NeoHal.Core.Entities;

/// <summary>
/// HKS Bildirim Kuyruğu - Offline/Online Senkronizasyon
/// </summary>
public class HksBildirim : BaseEntity
{
    public string BildirimTipi { get; set; } = string.Empty;
    
    // Referans Belge
    public Guid ReferansBelgeId { get; set; }
    public string ReferansBelgeTipi { get; set; } = string.Empty;
    
    // Bildirim Verisi (JSON)
    public string BildirimJson { get; set; } = string.Empty;
    
    // Durum Takibi
    public HksBildirimDurumu Durum { get; set; } = HksBildirimDurumu.Beklemede;
    public DateTime? GonderimTarihi { get; set; }
    public DateTime? OnayTarihi { get; set; }
    
    // HKS Yanıtı
    public string? HksKunyeNo { get; set; }
    public string? HksYanitKodu { get; set; }
    public string? HksYanitMesaji { get; set; }
    
    // Hata Durumu
    public int DenemeSayisi { get; set; } = 0;
    public string? SonHataMesaji { get; set; }
}

/// <summary>
/// Müstahsil Makbuzu - e-MM Entegrasyonu
/// </summary>
public class MustahsilMakbuzu : BaseEntity
{
    public string MakbuzNo { get; set; } = string.Empty;
    public DateTime Tarih { get; set; } = DateTime.Today;
    
    public Guid MustahsilId { get; set; }
    public Guid GirisIrsaliyesiId { get; set; }
    
    // Ürün Bilgileri
    public Guid UrunId { get; set; }
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal BrutTutar { get; set; }
    
    // Kesintiler
    public decimal StopajTutari { get; set; } = 0;
    public decimal RusumTutari { get; set; } = 0;
    public decimal KomisyonTutari { get; set; } = 0;
    public decimal NavlunTutari { get; set; } = 0;
    public decimal HamaliyeTutari { get; set; } = 0;
    public decimal BagkurTutari { get; set; } = 0;
    public decimal DigerKesintiler { get; set; } = 0;
    
    public decimal ToplamKesinti => StopajTutari + RusumTutari + KomisyonTutari + 
                                    NavlunTutari + HamaliyeTutari + BagkurTutari + DigerKesintiler;
    public decimal NetOdeme => BrutTutar - ToplamKesinti;
    
    // e-MM Bilgileri
    public EBelgeDurumu EmmDurumu { get; set; } = EBelgeDurumu.Taslak;
    public string? EmmUuid { get; set; }
    public DateTime? EmmTarihi { get; set; }
    
    // Ödeme
    public bool Odendi { get; set; } = false;
    public DateTime? OdemeTarihi { get; set; }
    public OdemeTuru? OdemeTuru { get; set; }
    
    // Navigation
    public virtual CariHesap Mustahsil { get; set; } = null!;
    public virtual GirisIrsaliyesi GirisIrsaliyesi { get; set; } = null!;
    public virtual Urun Urun { get; set; } = null!;
}
