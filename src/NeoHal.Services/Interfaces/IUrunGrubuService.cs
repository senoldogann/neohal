using NeoHal.Core.Entities;

namespace NeoHal.Services.Interfaces;

public interface IUrunGrubuService
{
    Task<IEnumerable<UrunGrubu>> GetAllAsync();
    Task<IEnumerable<UrunGrubu>> GetActiveAsync();
    Task<UrunGrubu?> GetByIdAsync(Guid id);
    Task<UrunGrubu> CreateAsync(UrunGrubu grup);
    Task<UrunGrubu> UpdateAsync(UrunGrubu grup);
    Task DeleteAsync(Guid id);
}
