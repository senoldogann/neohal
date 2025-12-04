using NeoHal.Core.Entities;

namespace NeoHal.Services.Interfaces;

/// <summary>
/// Rapor oluşturma ve dışa aktarma servisi
/// </summary>
public interface IRaporService
{
    /// <summary>
    /// Günlük satış raporu oluşturur
    /// </summary>
    Task<RaporSonucu> GunlukSatisRaporuAsync(DateTime tarih, RaporFiltresi? filtre = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Tarih aralığı satış raporu oluşturur
    /// </summary>
    Task<RaporSonucu> SatisRaporuAsync(DateTime baslangic, DateTime bitis, RaporFiltresi? filtre = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cari hesap ekstre raporu oluşturur
    /// </summary>
    Task<RaporSonucu> CariExtreRaporuAsync(Guid cariId, DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ürün satış raporu oluşturur
    /// </summary>
    Task<RaporSonucu> UrunSatisRaporuAsync(DateTime baslangic, DateTime bitis, Guid? urunId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stok durumu raporu oluşturur
    /// </summary>
    Task<RaporSonucu> StokDurumuRaporuAsync(DateTime? tarih = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Kasa raporu oluşturur
    /// </summary>
    Task<RaporSonucu> KasaRaporuAsync(DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Müşteri analiz raporu oluşturur
    /// </summary>
    Task<RaporSonucu> MusteriAnalizRaporuAsync(DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ürün analiz raporu oluşturur
    /// </summary>
    Task<RaporSonucu> UrunAnalizRaporuAsync(DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// HKS bildirim raporu oluşturur
    /// </summary>
    Task<RaporSonucu> HksBildirimRaporuAsync(DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Raporu PDF olarak dışa aktarır
    /// </summary>
    Task<byte[]> ExportToPdfAsync(RaporSonucu rapor, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Raporu Excel olarak dışa aktarır
    /// </summary>
    Task<byte[]> ExportToExcelAsync(RaporSonucu rapor, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Raporu CSV olarak dışa aktarır
    /// </summary>
    Task<byte[]> ExportToCsvAsync(RaporSonucu rapor, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Rapor şablonlarını listeler
    /// </summary>
    Task<IEnumerable<RaporSablonu>> GetRaporSablonlariAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Rapor şablonu kaydeder
    /// </summary>
    Task SaveRaporSablonuAsync(RaporSablonu sablon, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Zamanlanmış rapor oluşturur
    /// </summary>
    Task<ZamanlanmisRapor> CreateZamanlanmisRaporAsync(ZamanlanmisRapor rapor, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Zamanlanmış raporları listeler
    /// </summary>
    Task<IEnumerable<ZamanlanmisRapor>> GetZamanlanmisRaporlarAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Rapor sonucu
/// </summary>
public class RaporSonucu
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RaporAdi { get; set; } = string.Empty;
    public RaporTuru Tur { get; set; }
    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public RaporFiltresi? Filtre { get; set; }
    
    // Özet Bilgiler
    public RaporOzet Ozet { get; set; } = new();
    
    // Detay Satırları
    public List<RaporSatiri> Satirlar { get; set; } = new();
    
    // Sütun Tanımları
    public List<RaporSutunu> Sutunlar { get; set; } = new();
    
    // Grafikler için veri
    public List<GrafikVerisi> GrafikVerileri { get; set; } = new();
}

/// <summary>
/// Rapor özet bilgileri
/// </summary>
public class RaporOzet
{
    public int ToplamKayit { get; set; }
    public decimal ToplamTutar { get; set; }
    public decimal ToplamKdv { get; set; }
    public decimal ToplamNet { get; set; }
    public decimal ToplamMiktar { get; set; }
    public Dictionary<string, object> EkBilgiler { get; set; } = new();
}

/// <summary>
/// Rapor satırı
/// </summary>
public class RaporSatiri
{
    public Dictionary<string, object?> Degerler { get; set; } = new();
}

/// <summary>
/// Rapor sütunu
/// </summary>
public class RaporSutunu
{
    public string Anahtar { get; set; } = string.Empty;
    public string Baslik { get; set; } = string.Empty;
    public SutunTipi Tip { get; set; } = SutunTipi.Metin;
    public int Genislik { get; set; } = 100;
    public bool Toplanabilir { get; set; }
    public string? Format { get; set; }
}

/// <summary>
/// Sütun veri tipi
/// </summary>
public enum SutunTipi
{
    Metin,
    Sayi,
    ParaBirimi,
    Tarih,
    Boole
}

/// <summary>
/// Grafik verisi
/// </summary>
public class GrafikVerisi
{
    public string Etiket { get; set; } = string.Empty;
    public decimal Deger { get; set; }
    public string? Renk { get; set; }
}

/// <summary>
/// Rapor filtresi
/// </summary>
public class RaporFiltresi
{
    public Guid? CariHesapId { get; set; }
    public Guid? UrunId { get; set; }
    public Guid? UrunGrubuId { get; set; }
    public string? CariTipi { get; set; }
    public string? Arama { get; set; }
    public string? SiralamaSutunu { get; set; }
    public bool SiralamaAzalan { get; set; }
    public int? SayfaNo { get; set; }
    public int? SayfaBoyutu { get; set; }
}

/// <summary>
/// Rapor türü
/// </summary>
public enum RaporTuru
{
    GunlukSatis,
    DonemselSatis,
    CariExtre,
    UrunSatis,
    StokDurumu,
    KasaHareket,
    MusteriAnaliz,
    UrunAnaliz,
    HksBildirim,
    Ozet
}

/// <summary>
/// Rapor şablonu
/// </summary>
public class RaporSablonu
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Ad { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public RaporTuru Tur { get; set; }
    public RaporFiltresi? VarsayilanFiltre { get; set; }
    public bool Aktif { get; set; } = true;
    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
}

/// <summary>
/// Zamanlanmış rapor
/// </summary>
public class ZamanlanmisRapor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Ad { get; set; } = string.Empty;
    public RaporTuru Tur { get; set; }
    public RaporFiltresi? Filtre { get; set; }
    public RaporZamanlama Zamanlama { get; set; } = RaporZamanlama.Gunluk;
    public TimeOnly CalismaSaati { get; set; } = new TimeOnly(8, 0);
    public ExportFormati ExportFormat { get; set; } = ExportFormati.Pdf;
    public string? EmailAdresi { get; set; }
    public bool Aktif { get; set; } = true;
    public DateTime? SonCalisma { get; set; }
    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
}

/// <summary>
/// Rapor zamanlama
/// </summary>
public enum RaporZamanlama
{
    Gunluk,
    Haftalik,
    Aylik
}

/// <summary>
/// Export formatı
/// </summary>
public enum ExportFormati
{
    Pdf,
    Excel,
    Csv
}
