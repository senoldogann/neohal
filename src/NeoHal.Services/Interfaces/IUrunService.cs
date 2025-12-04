using NeoHal.Core.Entities;

namespace NeoHal.Services.Interfaces;

public interface IUrunService
{
    Task<IEnumerable<Urun>> GetAllAsync();
    Task<IEnumerable<Urun>> GetActiveAsync();
    Task<IEnumerable<Urun>> SearchAsync(string searchTerm);
    Task<IEnumerable<Urun>> GetByGrupIdAsync(Guid grupId);
    Task<Urun?> GetByIdAsync(Guid id);
    Task<Urun?> GetByKodAsync(string kod);
    Task<Urun> CreateAsync(Urun urun);
    Task<Urun> UpdateAsync(Urun urun);
    Task DeleteAsync(Guid id);
    Task<string> GenerateNewKodAsync();
}
