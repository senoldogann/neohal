using Microsoft.EntityFrameworkCore;
using NeoHal.Core.Entities;
using NeoHal.Data.Context;
using NeoHal.Services.Interfaces;

namespace NeoHal.Services.Implementations;

public class KapTipiService : IKapTipiService
{
    private readonly NeoHalDbContext _context;

    public KapTipiService(NeoHalDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<KapTipi>> GetAllAsync()
    {
        return await _context.KapTipleri
            .OrderBy(x => x.Ad)
            .ToListAsync();
    }

    public async Task<IEnumerable<KapTipi>> GetActiveAsync()
    {
        return await _context.KapTipleri
            .Where(x => x.Aktif)
            .OrderBy(x => x.Ad)
            .ToListAsync();
    }

    public async Task<KapTipi?> GetByIdAsync(Guid id)
    {
        return await _context.KapTipleri.FindAsync(id);
    }

    public async Task<KapTipi> CreateAsync(KapTipi kapTipi)
    {
        kapTipi.Id = Guid.NewGuid();
        kapTipi.OlusturmaTarihi = DateTime.Now;
        
        _context.KapTipleri.Add(kapTipi);
        await _context.SaveChangesAsync();
        return kapTipi;
    }

    public async Task<KapTipi> UpdateAsync(KapTipi kapTipi)
    {
        var existing = await _context.KapTipleri.FindAsync(kapTipi.Id);
        if (existing == null)
            throw new Exception("Kap tipi bulunamadı");

        existing.Kod = kapTipi.Kod;
        existing.Ad = kapTipi.Ad;
        existing.DaraAgirlik = kapTipi.DaraAgirlik;
        existing.RehinBedeli = kapTipi.RehinBedeli;
        existing.Aktif = kapTipi.Aktif;
        existing.GuncellemeTarihi = DateTime.Now;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(Guid id)
    {
        var kapTipi = await _context.KapTipleri.FindAsync(id);
        if (kapTipi != null)
        {
            _context.KapTipleri.Remove(kapTipi);
            await _context.SaveChangesAsync();
        }
    }
}
