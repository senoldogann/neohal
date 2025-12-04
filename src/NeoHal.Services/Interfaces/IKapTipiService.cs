using NeoHal.Core.Entities;

namespace NeoHal.Services.Interfaces;

public interface IKapTipiService
{
    Task<IEnumerable<KapTipi>> GetAllAsync();
    Task<IEnumerable<KapTipi>> GetActiveAsync();
    Task<KapTipi?> GetByIdAsync(Guid id);
    Task<KapTipi> CreateAsync(KapTipi kapTipi);
    Task<KapTipi> UpdateAsync(KapTipi kapTipi);
    Task DeleteAsync(Guid id);
}
