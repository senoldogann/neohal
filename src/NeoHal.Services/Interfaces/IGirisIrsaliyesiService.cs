using NeoHal.Core.Entities;

namespace NeoHal.Services.Interfaces;

public interface IGirisIrsaliyesiService
{
    Task<IEnumerable<GirisIrsaliyesi>> GetAllAsync();
    Task<IEnumerable<GirisIrsaliyesi>> GetByDateRangeAsync(DateTime baslangic, DateTime bitis);
    Task<IEnumerable<GirisIrsaliyesi>> GetByMustahsilIdAsync(Guid mustahsilId);
    Task<GirisIrsaliyesi?> GetByIdAsync(Guid id);
    Task<GirisIrsaliyesi?> GetByIdWithKalemlerAsync(Guid id);
    Task<GirisIrsaliyesi> CreateAsync(GirisIrsaliyesi irsaliye);
    Task UpdateAsync(GirisIrsaliyesi irsaliye);
    Task DeleteAsync(Guid id);
    Task<string> GenerateNewIrsaliyeNoAsync();
    Task OnaylaAsync(Guid id);
    Task IptalEtAsync(Guid id);
    
    /// <summary>
    /// Belirtilen tarihe ait aktif (silinmemiş) irsaliyeyi getirir.
    /// Hal kayıtta aynı güne ait tüm alımlar tek irsaliyede toplanır.
    /// </summary>
    Task<GirisIrsaliyesi?> GetByTarihAsync(DateTime tarih);
    
    /// <summary>
    /// Mevcut irsaliyeye yeni kalemler ekler
    /// </summary>
    Task AddKalemlerAsync(Guid irsaliyeId, IEnumerable<GirisIrsaliyesiKalem> kalemler);
}
