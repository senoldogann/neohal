using NeoHal.Core.Entities;

namespace NeoHal.Services.Interfaces;

public interface ICariHesapService
{
    Task<IEnumerable<CariHesap>> GetAllAsync();
    Task<CariHesap?> GetByIdAsync(Guid id);
    Task<CariHesap?> GetByKodAsync(string kod);
    Task<IEnumerable<CariHesap>> GetByCariTipiAsync(Core.Enums.CariTipi cariTipi);
    Task<IEnumerable<CariHesap>> SearchAsync(string searchTerm);
    Task<CariHesap> CreateAsync(CariHesap cariHesap);
    Task UpdateAsync(CariHesap cariHesap);
    Task DeleteAsync(Guid id);
    Task<string> GenerateNewKodAsync(Core.Enums.CariTipi cariTipi);
    
    /// <summary>
    /// Cari hesabın güncel bakiyesini hesaplar (Borç - Alacak)
    /// </summary>
    Task<decimal> GetBakiyeAsync(Guid cariId);
    
    /// <summary>
    /// Cari hesabın risk limiti durumunu kontrol eder
    /// </summary>
    /// <returns>Tuple: (limitAsildiMi, mevcutBakiye, riskLimiti)</returns>
    Task<(bool LimitAsildi, decimal MevcutBakiye, decimal RiskLimiti)> CheckRiskLimitiAsync(Guid cariId, decimal yeniIslemTutari);
}
