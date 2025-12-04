using Microsoft.EntityFrameworkCore;
using NeoHal.Core.Entities;
using NeoHal.Core.Enums;
using NeoHal.Data.Context;
using NeoHal.Services.Interfaces;

namespace NeoHal.Services.Implementations;

public class CariHesapService : ICariHesapService
{
    private readonly NeoHalDbContext _context;
    private readonly IKasaTakipService _kasaTakipService;

    public CariHesapService(NeoHalDbContext context, IKasaTakipService kasaTakipService)
    {
        _context = context;
        _kasaTakipService = kasaTakipService;
    }

    public async Task<IEnumerable<CariHesap>> GetAllAsync()
    {
        return await _context.CariHesaplar
            .Include(c => c.Il)
            .Include(c => c.Ilce)
            .OrderBy(c => c.Unvan)
            .ToListAsync();
    }

    public async Task<IEnumerable<CariHesap>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();
            
        var term = searchTerm.ToLower();
        return await _context.CariHesaplar
            .Include(c => c.Il)
            .Include(c => c.Ilce)
            .Where(c => c.Unvan.ToLower().Contains(term) || 
                        c.Kod.ToLower().Contains(term) ||
                        (c.Telefon != null && c.Telefon.Contains(term)))
            .OrderBy(c => c.Unvan)
            .ToListAsync();
    }

    public async Task<CariHesap?> GetByIdAsync(Guid id)
    {
        return await _context.CariHesaplar
            .Include(c => c.Il)
            .Include(c => c.Ilce)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CariHesap?> GetByKodAsync(string kod)
    {
        return await _context.CariHesaplar
            .FirstOrDefaultAsync(c => c.Kod == kod);
    }

    public async Task<IEnumerable<CariHesap>> GetByCariTipiAsync(CariTipi cariTipi)
    {
        return await _context.CariHesaplar
            .Where(c => c.CariTipi == cariTipi)
            .OrderBy(c => c.Unvan)
            .ToListAsync();
    }

    public async Task<CariHesap> CreateAsync(CariHesap cariHesap)
    {
        cariHesap.OlusturmaTarihi = DateTime.UtcNow;
        _context.CariHesaplar.Add(cariHesap);
        await _context.SaveChangesAsync();
        return cariHesap;
    }

    public async Task UpdateAsync(CariHesap cariHesap)
    {
        cariHesap.GuncellemeTarihi = DateTime.UtcNow;
        _context.CariHesaplar.Update(cariHesap);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var cari = await _context.CariHesaplar.FindAsync(id);
        if (cari != null)
        {
            cari.IsDeleted = true;
            cari.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateNewKodAsync(CariTipi cariTipi)
    {
        var prefix = cariTipi switch
        {
            CariTipi.Mustahsil => "MUS",
            CariTipi.Komisyoncu => "KOM",
            CariTipi.Sevkiyatci => "SVK",
            CariTipi.Alici => "ALC",
            CariTipi.Nakliyeci => "NAK",
            _ => "CRI"
        };

        var lastKod = await _context.CariHesaplar
            .IgnoreQueryFilters()
            .Where(c => c.Kod.StartsWith(prefix))
            .OrderByDescending(c => c.Kod)
            .Select(c => c.Kod)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (!string.IsNullOrEmpty(lastKod) && lastKod.Length > 3)
        {
            if (int.TryParse(lastKod.Substring(3), out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D5}";
    }

    public async Task<decimal> GetBakiyeAsync(Guid cariId)
    {
        var hareketler = await _context.CariHareketler
            .Where(h => h.CariId == cariId)
            .ToListAsync();
        
        decimal toplamBorc = hareketler
            .Where(h => h.HareketTipi == CariHareketTipi.Borc)
            .Sum(h => h.Tutar);
            
        decimal toplamAlacak = hareketler
            .Where(h => h.HareketTipi == CariHareketTipi.Alacak)
            .Sum(h => h.Tutar);
        
        return toplamBorc - toplamAlacak; // Pozitif = Borç, Negatif = Alacak
    }

    public async Task<(bool LimitAsildi, decimal MevcutBakiye, decimal RiskLimiti)> CheckRiskLimitiAsync(Guid cariId, decimal yeniIslemTutari)
    {
        var cari = await _context.CariHesaplar.FindAsync(cariId);
        if (cari == null)
            return (false, 0, 0);
        
        // Risk limiti 0 ise limit kontrolü yapma (sınırsız)
        if (cari.RiskLimiti == 0)
            return (false, 0, 0);
        
        // 1. Nakdi Bakiye
        var nakdiBakiye = await GetBakiyeAsync(cariId);
        
        // 2. Kasa Rehin Riski (Müşteride kalan kasaların bedeli)
        var kasaBorcu = await _kasaTakipService.GetToplamKasaBorcuAsync(cariId);
        
        // Toplam Risk = Nakdi Bakiye + Kasa Riski + Yeni İşlem
        var toplamRisk = nakdiBakiye + kasaBorcu + yeniIslemTutari;
        
        var limitAsildi = toplamRisk > cari.RiskLimiti;
        
        // Mevcut Bakiye olarak Toplam Riski dönüyoruz ki kullanıcı ne kadar dolu olduğunu görsün
        return (limitAsildi, nakdiBakiye + kasaBorcu, cari.RiskLimiti);
    }
}
