using NeoHal.Core.Entities;
using NeoHal.Core.Enums;

namespace NeoHal.Services.Interfaces;

public interface ISatisFaturasiService
{
    Task<IEnumerable<SatisFaturasi>> GetAllAsync();
    Task<IEnumerable<SatisFaturasi>> GetByDateRangeAsync(DateTime baslangic, DateTime bitis);
    Task<IEnumerable<SatisFaturasi>> GetByAliciIdAsync(Guid aliciId);
    Task<IEnumerable<SatisFaturasi>> GetByMustahsilIdAsync(Guid mustahsilId);
    Task<IEnumerable<SatisFaturasi>> GetByFaturaTipiAsync(FaturaTipi tip);
    Task<SatisFaturasi?> GetByIdAsync(Guid id);
    Task<SatisFaturasi?> GetByIdWithKalemlerAsync(Guid id);
    Task<SatisFaturasi> CreateAsync(SatisFaturasi fatura);
    Task UpdateAsync(SatisFaturasi fatura);
    Task DeleteAsync(Guid id);
    Task<string> GenerateNewFaturaNoAsync();
    Task OnaylaAsync(Guid id);
    Task IptalEtAsync(Guid id);
    
    // İrsaliyeden fatura oluşturma
    Task<SatisFaturasi> CreateFromIrsaliyeKalemAsync(Guid irsaliyeKalemId, Guid aliciId, decimal fiyat, int kapAdet);
    
    /// <summary>
    /// Sevkiyatçı satışı için fatura oluşturur (kesinti YOK)
    /// </summary>
    Task<SatisFaturasi> CreateSevkiyatFaturasiAsync(SatisFaturasi fatura);
    
    /// <summary>
    /// Şubeler arası transfer faturası (iç transfer, maliyet fiyatı)
    /// </summary>
    Task<SatisFaturasi> CreateIcTransferFaturasiAsync(SatisFaturasi fatura);
    
    /// <summary>
    /// Fatura kalemlerine göre otomatik rehin fişi oluşturur
    /// </summary>
    Task CreateRehinFisiForFaturaAsync(Guid faturaId);
}
