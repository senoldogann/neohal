using Microsoft.EntityFrameworkCore;
using NeoHal.Core.Entities;
using NeoHal.Data.Context;
using NeoHal.Services.Interfaces;

namespace NeoHal.Services.Implementations;

public class UrunService : IUrunService
{
    private readonly NeoHalDbContext _context;

    public UrunService(NeoHalDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Urun>> GetAllAsync()
    {
        return await _context.Urunler
            .Include(u => u.Grup)
            .OrderBy(u => u.Ad)
            .ToListAsync();
    }

    public async Task<IEnumerable<Urun>> GetActiveAsync()
    {
        return await _context.Urunler
            .Include(u => u.Grup)
            .Where(u => u.Aktif)
            .OrderBy(u => u.Ad)
            .ToListAsync();
    }

    public async Task<IEnumerable<Urun>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();
            
        var term = searchTerm.ToLower();
        return await _context.Urunler
            .Include(u => u.Grup)
            .Where(u => u.Ad.ToLower().Contains(term) || 
                        u.Kod.ToLower().Contains(term))
            .OrderBy(u => u.Ad)
            .ToListAsync();
    }

    public async Task<IEnumerable<Urun>> GetByGrupIdAsync(Guid grupId)
    {
        return await _context.Urunler
            .Where(u => u.GrupId == grupId)
            .OrderBy(u => u.Ad)
            .ToListAsync();
    }

    public async Task<Urun?> GetByIdAsync(Guid id)
    {
        return await _context.Urunler
            .Include(u => u.Grup)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Urun?> GetByKodAsync(string kod)
    {
        return await _context.Urunler
            .Include(u => u.Grup)
            .FirstOrDefaultAsync(u => u.Kod == kod);
    }

    public async Task<Urun> CreateAsync(Urun urun)
    {
        if (string.IsNullOrEmpty(urun.Kod))
            urun.Kod = await GenerateNewKodAsync();
            
        urun.OlusturmaTarihi = DateTime.Now;
        _context.Urunler.Add(urun);
        await _context.SaveChangesAsync();
        return urun;
    }

    public async Task<Urun> UpdateAsync(Urun urun)
    {
        var existing = await _context.Urunler.FindAsync(urun.Id);
        if (existing == null)
            throw new Exception("Ürün bulunamadı");

        existing.Kod = urun.Kod;
        existing.Ad = urun.Ad;
        existing.GrupId = urun.GrupId;
        existing.Birim = urun.Birim;
        existing.KdvOrani = urun.KdvOrani;
        existing.RusumOrani = urun.RusumOrani;
        existing.StopajOrani = urun.StopajOrani;
        existing.HksUrunKodu = urun.HksUrunKodu;
        existing.Aktif = urun.Aktif;
        existing.GuncellemeTarihi = DateTime.Now;
        
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(Guid id)
    {
        var urun = await _context.Urunler.FindAsync(id);
        if (urun != null)
        {
            _context.Urunler.Remove(urun);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateNewKodAsync()
    {
        var prefix = "URN";
        var lastKod = await _context.Urunler
            .IgnoreQueryFilters()
            .Where(u => u.Kod.StartsWith(prefix))
            .OrderByDescending(u => u.Kod)
            .Select(u => u.Kod)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (!string.IsNullOrEmpty(lastKod) && lastKod.Length > prefix.Length + 1)
        {
            if (int.TryParse(lastKod.Substring(prefix.Length + 1), out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}-{nextNumber:D4}";
    }
}
