using NeoHal.Core.Common;
using NeoHal.Core.Enums;

namespace NeoHal.Core.Entities;

/// <summary>
/// Satış Faturası - Alıcıya/Dükkana/Şubeye kesilen fatura
/// 
/// FaturaTipi'ne göre kesinti davranışı değişir:
/// - Toptan/Perakende: Komisyon, Rüsum, Stopaj uygulanır
/// - Sevkiyat/IcTransfer: HİÇBİR KESİNTİ UYGULANMAZ
/// </summary>
public class SatisFaturasi : SoftDeleteEntity
{
    public string FaturaNo { get; set; } = string.Empty;
    public DateTime FaturaTarihi { get; set; } = DateTime.Today;
    public FaturaTipi FaturaTipi { get; set; } = FaturaTipi.Sevkiyat; // Sevkiyatçı için varsayılan
    
    // İlişkili Cariler
    public Guid AliciId { get; set; }       // Malı alan (şube, müşteri vs.)
    public Guid? MustahsilId { get; set; }  // Komisyon kesilen müstahsil (sadece Toptan/Perakende için)
    
    // Tutarlar
    public decimal AraToplam { get; set; }
    
    // Kesintiler - Sadece FaturaTipi = Toptan/Perakende için
    public decimal RusumTutari { get; set; }
    public decimal KomisyonTutari { get; set; }
    public decimal StopajTutari { get; set; }
    
    // Ek Masraflar - Sevkiyatçı için
    /// <summary>
    /// Nakliye, yükleme, boşaltma gibi ek masraflar (JSON formatında veya toplam tutar)
    /// </summary>
    public decimal EkMasrafTutari { get; set; } = 0;
    public string? EkMasrafAciklama { get; set; }
    
    public decimal KdvTutari { get; set; }
    public decimal GenelToplam { get; set; }
    
    // Ödeme
    public OdemeDurumu OdemeDurumu { get; set; } = OdemeDurumu.Odenmedi;
    public decimal OdenenTutar { get; set; } = 0;
    
    // E-Fatura
    public string? EFaturaUuid { get; set; }
    public string? EFaturaDurum { get; set; }
    
    // HKS - Sadece komisyoncu faturalarında kullanılır
    public string? HksBildirimNo { get; set; }
    
    // Durum
    public BelgeDurumu Durum { get; set; } = BelgeDurumu.Taslak;
    
    // Notlar
    public string? Aciklama { get; set; }
    
    // Navigation
    public virtual CariHesap Alici { get; set; } = null!;
    public virtual CariHesap? Mustahsil { get; set; }
    public virtual ICollection<SatisFaturasiKalem> Kalemler { get; set; } = new List<SatisFaturasiKalem>();
    
    /// <summary>
    /// Fatura tipine göre toplamları hesaplar
    /// Sevkiyat/IcTransfer: Kesinti uygulanmaz
    /// Toptan/Perakende: Kesintiler uygulanır
    /// </summary>
    /// <summary>
    /// Fatura tipine göre toplamları hesaplar
    /// Sevkiyat/IcTransfer: Kesinti uygulanmaz
    /// Toptan/Perakende: Kesintiler uygulanır
    /// </summary>
    public void HesaplaToplamlar()
    {
        // Ara toplam = Tüm kalemlerin tutarı
        AraToplam = Kalemler.Sum(k => k.Tutar);
        
        if (FaturaTipi == FaturaTipi.Sevkiyat || FaturaTipi == FaturaTipi.IcTransfer)
        {
            // Sevkiyatçı veya iç transfer - KESİNTİ YOK
            RusumTutari = 0;
            KomisyonTutari = 0;
            StopajTutari = 0;
            
            // Genel toplam = Ara toplam + Ek masraflar
            GenelToplam = AraToplam + EkMasrafTutari;
        }
        else
        {
            // Toptan/Perakende - Kesintiler kalemlerden toplanır
            
            // Hal Kanunu Gereği Rüsum Hesabı:
            // Rüsum Oranı genelde %1 veya %2'dir.
            // Eğer müstahsilin "Müstahsil Belgesi" varsa %1, yoksa %2 olabilir.
            // Burada varsayılan olarak kalemlerdeki oranı kullanıyoruz.
            
            RusumTutari = Kalemler.Sum(k => k.RusumTutari);
            KomisyonTutari = Kalemler.Sum(k => k.KomisyonTutari);
            StopajTutari = Kalemler.Sum(k => k.StopajTutari);
            
            // Genel toplam = Ara toplam - Kesintiler + Ek masraflar
            // Not: Hal sisteminde genellikle alıcı brüt tutarı öder, kesintiler müstahsilden düşülür.
            // Ancak fatura toplamı alıcının ödeyeceği tutardır.
            // Alıcı sadece Mal Bedeli + KDV + Varsa Rüsum (Alıcı öderse) öder.
            // Komisyoncu faturasında ise:
            // Alıcıdan tahsil edilen: Mal Bedeli + KDV + Rüsum (bazı durumlarda)
            // Müstahsile ödenen: Mal Bedeli - (Komisyon + Rüsum + Stopaj + Masraflar)
            
            // Basitleştirilmiş model: Fatura alıcıya kesildiği için alıcının ödeyeceği tutar esastır.
            // Alıcı mal bedelini öder. Kesintiler arka planda müstahsil cari hesabına işlenir.
            
            GenelToplam = AraToplam + EkMasrafTutari;
        }
        
        // KDV ayrıca hesaplanabilir (ayar bazlı)
        // KdvTutari = GenelToplam * KdvOrani;
    }
}

/// <summary>
/// Satış Faturası Kalemi
/// 
/// Sevkiyatçı için önemli: KomisyoncuId ile hangi komisyoncudan alındığı,
/// AlisFiyati ile maliyet takip edilir
/// </summary>
public class SatisFaturasiKalem : BaseEntity
{
    public Guid FaturaId { get; set; }
    public Guid? GirisKalemId { get; set; } // FIFO takibi için - hangi girişten satıldı
    public Guid UrunId { get; set; }
    public Guid KapTipiId { get; set; }
    
    // Sevkiyatçı için - Hangi komisyoncudan alındı
    /// <summary>
    /// Malın alındığı komisyoncu (sevkiyatçı için maliyet takibi)
    /// </summary>
    public Guid? KomisyoncuId { get; set; }
    
    /// <summary>
    /// Komisyoncudan alış fiyatı (maliyet)
    /// Kar = SatisFiyati - AlisFiyati
    /// </summary>
    public decimal AlisFiyati { get; set; } = 0;
    
    // Miktar Bilgileri
    public int KapAdet { get; set; }
    public decimal BrutKg { get; set; }
    public decimal DaraKg { get; set; }
    public decimal NetKg { get; set; }
    
    // Fiyat ve Tutar
    public decimal BirimFiyat { get; set; } // Satış fiyatı
    public decimal Tutar { get; set; }
    
    // Kesintiler - Sadece komisyoncu faturaları için (Toptan/Perakende)
    // Sevkiyat/IcTransfer için hepsi 0 kalır
    public decimal RusumOrani { get; set; }
    public decimal RusumTutari { get; set; }
    public decimal KomisyonOrani { get; set; }
    public decimal KomisyonTutari { get; set; }
    public decimal StopajOrani { get; set; }
    public decimal StopajTutari { get; set; }
    
    // Navigation
    public virtual SatisFaturasi Fatura { get; set; } = null!;
    public virtual GirisIrsaliyesiKalem? GirisKalem { get; set; }
    public virtual Urun Urun { get; set; } = null!;
    public virtual KapTipi KapTipi { get; set; } = null!;
    public virtual CariHesap? Komisyoncu { get; set; }
}
