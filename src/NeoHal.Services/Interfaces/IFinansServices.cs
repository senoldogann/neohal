using NeoHal.Core.Entities;

namespace NeoHal.Services.Interfaces;

public interface IKasaHesabiService
{
    Task<IEnumerable<KasaHesabi>> GetAllAsync();
    Task<IEnumerable<KasaHesabi>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<KasaHesabi>> GetByCariIdAsync(Guid cariId);
    Task<KasaHesabi?> GetByIdAsync(Guid id);
    Task<KasaHesabi> CreateAsync(KasaHesabi kasaHesabi);
    Task UpdateAsync(KasaHesabi kasaHesabi);
    Task DeleteAsync(Guid id);
    Task<decimal> GetBakiyeAsync();
    Task<decimal> GetBakiyeByDateAsync(DateTime date);
}

public interface ICariHareketService
{
    Task<IEnumerable<CariHareket>> GetByCariIdAsync(Guid cariId);
    Task<IEnumerable<CariHareket>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<CariHareket> CreateAsync(CariHareket hareket);
    Task DeleteAsync(Guid id);
    Task<decimal> GetCariBakiyeAsync(Guid cariId);
}

public interface IKasaStokService
{
    Task<IEnumerable<KasaStokDurumu>> GetAllAsync();
    Task<IEnumerable<KasaStokDurumu>> GetByCariIdAsync(Guid cariId);
    Task<KasaStokDurumu?> GetByIdAsync(Guid id);
    Task<KasaStokDurumu?> GetByCariAndKapTipiAsync(Guid cariId, Guid kapTipiId);
    Task<KasaStokDurumu> CreateOrUpdateAsync(Guid cariId, Guid kapTipiId, int doluAdet, int bosAdet);
}

public interface ICekSenetService
{
    Task<IEnumerable<CekSenet>> GetAllAsync();
    Task<IEnumerable<CekSenet>> GetByCariIdAsync(Guid cariId);
    Task<IEnumerable<CekSenet>> GetByVadeRangeAsync(DateTime startDate, DateTime endDate);
    Task<CekSenet?> GetByIdAsync(Guid id);
    Task<CekSenet> CreateAsync(CekSenet cekSenet);
    Task UpdateAsync(CekSenet cekSenet);
    Task DeleteAsync(Guid id);
    Task TahsilEtAsync(Guid id);
}
