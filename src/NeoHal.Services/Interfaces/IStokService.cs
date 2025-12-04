using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeoHal.Services.Interfaces;

/// <summary>
/// Stok takip servisi - FIFO bazlı stok yönetimi
/// </summary>
public interface IStokService
{
    /// <summary>
    /// Ürün bazlı stok durumunu getirir
    /// </summary>
    Task<List<StokDurumuItem>> GetStokDurumuAsync(Guid? urunId = null, bool sadeceMevcut = true);
    
    /// <summary>
    /// Belirli bir ürünün FIFO lot detaylarını getirir
    /// </summary>
    Task<List<StokDetayItem>> GetStokDetayAsync(Guid urunId);
    
    /// <summary>
    /// Satış için FIFO sırasına göre stok rezerve eder
    /// </summary>
    Task<List<StokRezervasyon>> RezerveStokAsync(Guid urunId, decimal miktar);
    
    /// <summary>
    /// Stok düşümü yapar (satış sonrası)
    /// </summary>
    Task DusStokAsync(List<StokRezervasyon> rezervasyonlar);
}

/// <summary>
/// Stok rezervasyon bilgisi
/// </summary>
public class StokRezervasyon
{
    public Guid IrsaliyeKalemId { get; set; }
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
}

/// <summary>
/// Stok durumu özet satırı
/// </summary>
public class StokDurumuItem
{
    public Guid UrunId { get; set; }
    public string UrunKodu { get; set; } = string.Empty;
    public string UrunAdi { get; set; } = string.Empty;
    public string UrunGrubu { get; set; } = string.Empty;
    
    public decimal ToplamKalanKg { get; set; }
    public int ToplamKalanKap { get; set; }
    public int LotSayisi { get; set; }
    
    public DateTime? SonGirisTarihi { get; set; }
    public string SonGirisKaynagi { get; set; } = string.Empty;
    
    public decimal OrtalamaFiyat { get; set; }
    
    public string DurumRenk => ToplamKalanKg <= 0 ? "#F44336" : ToplamKalanKg < 100 ? "#FF9800" : "#4CAF50";
    public string DurumIkon => ToplamKalanKg <= 0 ? "❌" : ToplamKalanKg < 100 ? "⚠️" : "✅";
}

/// <summary>
/// Stok detayı - FIFO lot bilgisi
/// </summary>
public class StokDetayItem
{
    public Guid IrsaliyeKalemId { get; set; }
    public string IrsaliyeNo { get; set; } = string.Empty;
    public DateTime GirisTarihi { get; set; }
    public string MustahsilAdi { get; set; } = string.Empty;
    public string KapTipiAdi { get; set; } = string.Empty;
    
    public int BaslangicKap { get; set; }
    public decimal BaslangicKg { get; set; }
    public int KalanKap { get; set; }
    public decimal KalanKg { get; set; }
    public decimal BirimFiyat { get; set; }
    
    public int FifoSira { get; set; }
    
    public decimal SatilanKg => BaslangicKg - KalanKg;
    public int SatilanKap => BaslangicKap - KalanKap;
    public string DurumText => KalanKg > 0 ? "Mevcut" : "Tükenmiş";
    public string DurumRenk => KalanKg > 0 ? "#4CAF50" : "#888";
}
