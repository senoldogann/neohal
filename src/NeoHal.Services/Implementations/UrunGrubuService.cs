using Microsoft.EntityFrameworkCore;
using NeoHal.Core.Entities;
using NeoHal.Data.Context;
using NeoHal.Services.Interfaces;

namespace NeoHal.Services.Implementations;

public class UrunGrubuService : IUrunGrubuService
{
    private readonly NeoHalDbContext _context;

    public UrunGrubuService(NeoHalDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UrunGrubu>> GetAllAsync()
    {
        return await _context.UrunGruplari
            .OrderBy(x => x.Ad)
            .ToListAsync();
    }

    public async Task<IEnumerable<UrunGrubu>> GetActiveAsync()
    {
        return await _context.UrunGruplari
            .Where(x => x.Aktif)
            .OrderBy(x => x.Ad)
            .ToListAsync();
    }

    public async Task<UrunGrubu?> GetByIdAsync(Guid id)
    {
        return await _context.UrunGruplari
            .Include(x => x.Urunler)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<UrunGrubu> CreateAsync(UrunGrubu grup)
    {
        grup.Id = Guid.NewGuid();
        grup.OlusturmaTarihi = DateTime.Now;
        
        _context.UrunGruplari.Add(grup);
        await _context.SaveChangesAsync();
        return grup;
    }

    public async Task<UrunGrubu> UpdateAsync(UrunGrubu grup)
    {
        var existing = await _context.UrunGruplari.FindAsync(grup.Id);
        if (existing == null)
            throw new Exception("Ürün grubu bulunamadı");

        existing.Kod = grup.Kod;
        existing.Ad = grup.Ad;
        existing.Aktif = grup.Aktif;
        existing.GuncellemeTarihi = DateTime.Now;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(Guid id)
    {
        var grup = await _context.UrunGruplari.FindAsync(id);
        if (grup != null)
        {
            _context.UrunGruplari.Remove(grup);
            await _context.SaveChangesAsync();
        }
    }
}
