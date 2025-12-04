using Microsoft.EntityFrameworkCore;
using NeoHal.Core.Entities;
using NeoHal.Data.Context;
using NeoHal.Services.Interfaces;

namespace NeoHal.Services.Implementations;

public class KasaHesabiService : IKasaHesabiService
{
    private readonly NeoHalDbContext _context;

    public KasaHesabiService(NeoHalDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<KasaHesabi>> GetAllAsync()
    {
        return await _context.Set<KasaHesabi>()
            .Include(k => k.Cari)
            .OrderByDescending(k => k.Tarih)
            .ToListAsync();
    }

    public async Task<IEnumerable<KasaHesabi>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Set<KasaHesabi>()
            .Include(k => k.Cari)
            .Where(k => k.Tarih >= startDate && k.Tarih < endDate)
            .OrderByDescending(k => k.Tarih)
            .ToListAsync();
    }

    public async Task<IEnumerable<KasaHesabi>> GetByCariIdAsync(Guid cariId)
    {
        return await _context.Set<KasaHesabi>()
            .Where(k => k.CariId == cariId)
            .OrderByDescending(k => k.Tarih)
            .ToListAsync();
    }

    public async Task<KasaHesabi?> GetByIdAsync(Guid id)
    {
        return await _context.Set<KasaHesabi>()
            .Include(k => k.Cari)
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<KasaHesabi> CreateAsync(KasaHesabi kasaHesabi)
    {
        kasaHesabi.Id = Guid.NewGuid();
        kasaHesabi.OlusturmaTarihi = DateTime.Now;
        _context.Set<KasaHesabi>().Add(kasaHesabi);
        await _context.SaveChangesAsync();
        return kasaHesabi;
    }

    public async Task UpdateAsync(KasaHesabi kasaHesabi)
    {
        kasaHesabi.GuncellemeTarihi = DateTime.Now;
        _context.Set<KasaHesabi>().Update(kasaHesabi);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Set<KasaHesabi>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<KasaHesabi>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetBakiyeAsync()
    {
        var girisler = await _context.Set<KasaHesabi>()
            .Where(k => k.GirisHareketi)
            .SumAsync(k => k.Tutar);
        
        var cikislar = await _context.Set<KasaHesabi>()
            .Where(k => !k.GirisHareketi)
            .SumAsync(k => k.Tutar);
        
        return girisler - cikislar;
    }

    public async Task<decimal> GetBakiyeByDateAsync(DateTime date)
    {
        var girisler = await _context.Set<KasaHesabi>()
            .Where(k => k.GirisHareketi && k.Tarih <= date)
            .SumAsync(k => k.Tutar);
        
        var cikislar = await _context.Set<KasaHesabi>()
            .Where(k => !k.GirisHareketi && k.Tarih <= date)
            .SumAsync(k => k.Tutar);
        
        return girisler - cikislar;
    }
}

public class CariHareketService : ICariHareketService
{
    private readonly NeoHalDbContext _context;

    public CariHareketService(NeoHalDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CariHareket>> GetByCariIdAsync(Guid cariId)
    {
        return await _context.Set<CariHareket>()
            .Where(h => h.CariId == cariId)
            .OrderByDescending(h => h.Tarih)
            .ToListAsync();
    }

    public async Task<IEnumerable<CariHareket>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Set<CariHareket>()
            .Include(h => h.Cari)
            .Where(h => h.Tarih >= startDate && h.Tarih < endDate)
            .OrderByDescending(h => h.Tarih)
            .ToListAsync();
    }

    public async Task<CariHareket> CreateAsync(CariHareket hareket)
    {
        hareket.Id = Guid.NewGuid();
        hareket.OlusturmaTarihi = DateTime.Now;
        _context.Set<CariHareket>().Add(hareket);
        await _context.SaveChangesAsync();
        return hareket;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Set<CariHareket>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<CariHareket>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetCariBakiyeAsync(Guid cariId)
    {
        var borclar = await _context.Set<CariHareket>()
            .Where(h => h.CariId == cariId && h.HareketTipi == Core.Enums.CariHareketTipi.Borc)
            .SumAsync(h => h.Tutar);
        
        var alacaklar = await _context.Set<CariHareket>()
            .Where(h => h.CariId == cariId && h.HareketTipi == Core.Enums.CariHareketTipi.Alacak)
            .SumAsync(h => h.Tutar);
        
        return borclar - alacaklar;
    }
}

public class KasaStokService : IKasaStokService
{
    private readonly NeoHalDbContext _context;

    public KasaStokService(NeoHalDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<KasaStokDurumu>> GetAllAsync()
    {
        return await _context.Set<KasaStokDurumu>()
            .Include(k => k.Cari)
            .Include(k => k.KapTipi)
            .ToListAsync();
    }

    public async Task<IEnumerable<KasaStokDurumu>> GetByCariIdAsync(Guid cariId)
    {
        return await _context.Set<KasaStokDurumu>()
            .Include(k => k.KapTipi)
            .Where(k => k.CariId == cariId)
            .ToListAsync();
    }

    public async Task<KasaStokDurumu?> GetByIdAsync(Guid id)
    {
        return await _context.Set<KasaStokDurumu>()
            .Include(k => k.Cari)
            .Include(k => k.KapTipi)
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<KasaStokDurumu?> GetByCariAndKapTipiAsync(Guid cariId, Guid kapTipiId)
    {
        return await _context.Set<KasaStokDurumu>()
            .Include(k => k.Cari)
            .Include(k => k.KapTipi)
            .FirstOrDefaultAsync(k => k.CariId == cariId && k.KapTipiId == kapTipiId);
    }

    public async Task<KasaStokDurumu> CreateOrUpdateAsync(Guid cariId, Guid kapTipiId, int doluAdet, int bosAdet)
    {
        var existing = await GetByCariAndKapTipiAsync(cariId, kapTipiId);
        
        if (existing != null)
        {
            existing.DoluKasaAdet = doluAdet;
            existing.BosKasaAdet = bosAdet;
            existing.GuncellemeTarihi = DateTime.Now;
            _context.Set<KasaStokDurumu>().Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }
        
        var newStok = new KasaStokDurumu
        {
            Id = Guid.NewGuid(),
            CariId = cariId,
            KapTipiId = kapTipiId,
            DoluKasaAdet = doluAdet,
            BosKasaAdet = bosAdet,
            OlusturmaTarihi = DateTime.Now
        };
        
        _context.Set<KasaStokDurumu>().Add(newStok);
        await _context.SaveChangesAsync();
        return newStok;
    }
}

public class CekSenetService : ICekSenetService
{
    private readonly NeoHalDbContext _context;
    private readonly IKasaHesabiService _kasaHesabiService;

    public CekSenetService(NeoHalDbContext context, IKasaHesabiService kasaHesabiService)
    {
        _context = context;
        _kasaHesabiService = kasaHesabiService;
    }

    public async Task<IEnumerable<CekSenet>> GetAllAsync()
    {
        return await _context.Set<CekSenet>()
            .Include(c => c.Cari)
            .OrderByDescending(c => c.VadeTarihi)
            .ToListAsync();
    }

    public async Task<IEnumerable<CekSenet>> GetByCariIdAsync(Guid cariId)
    {
        return await _context.Set<CekSenet>()
            .Where(c => c.CariId == cariId)
            .OrderByDescending(c => c.VadeTarihi)
            .ToListAsync();
    }

    public async Task<IEnumerable<CekSenet>> GetByVadeRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Set<CekSenet>()
            .Include(c => c.Cari)
            .Where(c => c.VadeTarihi >= startDate && c.VadeTarihi < endDate)
            .OrderBy(c => c.VadeTarihi)
            .ToListAsync();
    }

    public async Task<CekSenet?> GetByIdAsync(Guid id)
    {
        return await _context.Set<CekSenet>()
            .Include(c => c.Cari)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CekSenet> CreateAsync(CekSenet cekSenet)
    {
        cekSenet.Id = Guid.NewGuid();
        cekSenet.OlusturmaTarihi = DateTime.Now;
        _context.Set<CekSenet>().Add(cekSenet);
        await _context.SaveChangesAsync();
        return cekSenet;
    }

    public async Task UpdateAsync(CekSenet cekSenet)
    {
        cekSenet.GuncellemeTarihi = DateTime.Now;
        _context.Set<CekSenet>().Update(cekSenet);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Set<CekSenet>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<CekSenet>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task TahsilEtAsync(Guid id)
    {
        var entity = await _context.Set<CekSenet>().FindAsync(id);
        if (entity != null)
        {
            if (entity.Durum == Core.Enums.CekSenetDurumu.TahsilEdildi)
                throw new InvalidOperationException("Bu çek/senet zaten tahsil edilmiş.");

            entity.Durum = Core.Enums.CekSenetDurumu.TahsilEdildi;
            entity.TahsilTarihi = DateTime.Now;
            entity.GuncellemeTarihi = DateTime.Now;
            
            // Kasa Hesabına İşle
            var kasaHareketi = new KasaHesabi
            {
                Tarih = DateTime.Now,
                GirisHareketi = true, // Tahsilat = Giriş
                Tutar = entity.Tutar,
                OdemeTuru = entity.CekMi ? Core.Enums.OdemeTuru.Cek : Core.Enums.OdemeTuru.Senet,
                CariId = entity.CariId,
                ReferansBelgeId = entity.Id,
                ReferansBelgeTipi = entity.CekMi ? "Cek" : "Senet",
                Aciklama = $"{(entity.CekMi ? "Çek" : "Senet")} tahsilatı: {entity.BelgeNo}"
            };
            
            await _kasaHesabiService.CreateAsync(kasaHareketi);
            
            await _context.SaveChangesAsync();
        }
    }
}
