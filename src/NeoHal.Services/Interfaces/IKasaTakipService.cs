using NeoHal.Core.Entities;
using NeoHal.Core.Enums;

namespace NeoHal.Services.Interfaces;

/// <summary>
/// Kasa takip servisi - Dolu/Boş kasa döngüsünü yönetir
/// </summary>
public interface IKasaTakipService
{
    /// <summary>
    /// Carinin belirli bir kap tipindeki kasa durumunu getirir
    /// </summary>
    Task<KasaStokDurumu?> GetKasaDurumuAsync(Guid cariId, Guid kapTipiId);
    
    /// <summary>
    /// Carinin tüm kasa durumlarını getirir
    /// </summary>
    Task<IEnumerable<KasaStokDurumu>> GetCariKasaDurumlariAsync(Guid cariId);
    
    /// <summary>
    /// Dolu kasa girişi yapar (Müstahsilden mal geldiğinde)
    /// </summary>
    Task DoluKasaGirisiAsync(Guid cariId, Guid kapTipiId, int adet, Guid? referansBelgeId = null, string? referansBelgeTipi = null);
    
    /// <summary>
    /// Dolu kasa çıkışı yapar (Alıcıya mal gittiğinde)
    /// </summary>
    Task DoluKasaCikisiAsync(Guid cariId, Guid kapTipiId, int adet, Guid? referansBelgeId = null, string? referansBelgeTipi = null);
    
    /// <summary>
    /// Boş kasa iadesi alır (Alıcıdan boş kasa geldiğinde)
    /// </summary>
    Task BosKasaIadesiAlAsync(Guid cariId, Guid kapTipiId, int adet, Guid? referansBelgeId = null, string? referansBelgeTipi = null);
    
    /// <summary>
    /// Boş kasa iadesi verir (Müstahsile boş kasa verildiğinde)
    /// </summary>
    Task BosKasaIadesiVerAsync(Guid cariId, Guid kapTipiId, int adet, Guid? referansBelgeId = null, string? referansBelgeTipi = null);
    
    /// <summary>
    /// Kasa rehin bedeli alır
    /// </summary>
    Task<RehinFisi> RehinAlAsync(Guid cariId, Guid kapTipiId, int adet);
    
    /// <summary>
    /// Kasa rehin bedeli iade eder
    /// </summary>
    Task<RehinFisi> RehinIadeEtAsync(Guid cariId, Guid kapTipiId, int adet);
    
    /// <summary>
    /// Kasa hareketlerini getirir
    /// </summary>
    Task<IEnumerable<KasaHareket>> GetKasaHareketleriAsync(Guid cariId, Guid? kapTipiId = null, DateTime? baslangic = null, DateTime? bitis = null);
    
    /// <summary>
    /// Carinin toplam kasa borcunu hesaplar
    /// </summary>
    Task<decimal> GetToplamKasaBorcuAsync(Guid cariId);
    
    /// <summary>
    /// Rehin fişlerini tarih aralığına göre getirir
    /// </summary>
    Task<IEnumerable<RehinFisi>> GetRehinFisleriAsync(DateTime baslangic, DateTime bitis);
}
