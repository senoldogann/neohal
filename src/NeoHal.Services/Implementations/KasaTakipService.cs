using Microsoft.EntityFrameworkCore;
using NeoHal.Core.Entities;
using NeoHal.Core.Enums;
using NeoHal.Data.Context;
using NeoHal.Services.Interfaces;

namespace NeoHal.Services.Implementations;

/// <summary>
/// Kasa Takip Servisi - Dolu/Boş kasa döngüsünü yönetir
/// </summary>
public class KasaTakipService : IKasaTakipService
{
    private readonly NeoHalDbContext _context;

    public KasaTakipService(NeoHalDbContext context)
    {
        _context = context;
    }

    public async Task<KasaStokDurumu?> GetKasaDurumuAsync(Guid cariId, Guid kapTipiId)
    {
        return await _context.KasaStokDurumlari
            .Include(k => k.KapTipi)
            .FirstOrDefaultAsync(k => k.CariId == cariId && k.KapTipiId == kapTipiId);
    }

    public async Task<IEnumerable<KasaStokDurumu>> GetCariKasaDurumlariAsync(Guid cariId)
    {
        return await _context.KasaStokDurumlari
            .Include(k => k.KapTipi)
            .Where(k => k.CariId == cariId)
            .ToListAsync();
    }

    /// <summary>
    /// Dolu kasa girişi - Müstahsilden mal geldiğinde
    /// Müstahsilin kasası bizde, dolayısıyla onun adına dolu kasa kaydediyoruz
    /// </summary>
    public async Task DoluKasaGirisiAsync(Guid cariId, Guid kapTipiId, int adet, Guid? referansBelgeId = null, string? referansBelgeTipi = null)
    {
        var kasaStok = await GetOrCreateKasaStokAsync(cariId, kapTipiId);
        
        kasaStok.DoluKasaAdet += adet;
        kasaStok.GuncellemeTarihi = DateTime.UtcNow;

        // Hareket kaydı
        var hareket = new KasaHareket
        {
            KasaStokId = kasaStok.Id,
            HareketTipi = KasaHareketTipi.GirisDolu,
            Adet = adet,
            Tarih = DateTime.Now,
            ReferansBelgeId = referansBelgeId,
            ReferansBelgeTipi = referansBelgeTipi,
            Aciklama = $"{adet} adet dolu kasa girişi"
        };
        _context.KasaHareketleri.Add(hareket);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Dolu kasa çıkışı - Alıcıya mal gittiğinde
    /// Müstahsilin kasasından düşüyoruz, Alıcıya ekliyoruz
    /// </summary>
    public async Task DoluKasaCikisiAsync(Guid cariId, Guid kapTipiId, int adet, Guid? referansBelgeId = null, string? referansBelgeTipi = null)
    {
        var kasaStok = await GetOrCreateKasaStokAsync(cariId, kapTipiId);
        
        kasaStok.DoluKasaAdet += adet; // Alıcının zimmetine dolu kasa ekleniyor
        kasaStok.GuncellemeTarihi = DateTime.UtcNow;

        // Rehin hesapla
        var kapTipi = await _context.KapTipleri.FindAsync(kapTipiId);
        if (kapTipi != null)
        {
            kasaStok.RehinToplam = (kasaStok.DoluKasaAdet + kasaStok.BosKasaAdet) * kapTipi.RehinBedeli;
        }

        var hareket = new KasaHareket
        {
            KasaStokId = kasaStok.Id,
            HareketTipi = KasaHareketTipi.GirisDolu, // Alıcıya giriş
            Adet = adet,
            Tarih = DateTime.Now,
            ReferansBelgeId = referansBelgeId,
            ReferansBelgeTipi = referansBelgeTipi,
            Aciklama = $"{adet} adet dolu kasa teslim alındı"
        };
        _context.KasaHareketleri.Add(hareket);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Boş kasa iadesi al - Alıcıdan boş kasa geldiğinde
    /// </summary>
    public async Task BosKasaIadesiAlAsync(Guid cariId, Guid kapTipiId, int adet, Guid? referansBelgeId = null, string? referansBelgeTipi = null)
    {
        var kasaStok = await GetOrCreateKasaStokAsync(cariId, kapTipiId);
        
        // Alıcının dolu kasası azalıyor (boşalttı)
        kasaStok.DoluKasaAdet = Math.Max(0, kasaStok.DoluKasaAdet - adet);
        kasaStok.GuncellemeTarihi = DateTime.UtcNow;

        // Rehin güncelle
        var kapTipi = await _context.KapTipleri.FindAsync(kapTipiId);
        if (kapTipi != null)
        {
            kasaStok.RehinToplam = (kasaStok.DoluKasaAdet + kasaStok.BosKasaAdet) * kapTipi.RehinBedeli;
        }

        var hareket = new KasaHareket
        {
            KasaStokId = kasaStok.Id,
            HareketTipi = KasaHareketTipi.GirisBos,
            Adet = adet,
            Tarih = DateTime.Now,
            ReferansBelgeId = referansBelgeId,
            ReferansBelgeTipi = referansBelgeTipi,
            Aciklama = $"{adet} adet boş kasa iade alındı"
        };
        _context.KasaHareketleri.Add(hareket);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Boş kasa iadesi ver - Müstahsile boş kasa verildiğinde
    /// </summary>
    public async Task BosKasaIadesiVerAsync(Guid cariId, Guid kapTipiId, int adet, Guid? referansBelgeId = null, string? referansBelgeTipi = null)
    {
        var kasaStok = await GetOrCreateKasaStokAsync(cariId, kapTipiId);
        
        // Müstahsilin bizde olan kasası azalıyor
        kasaStok.DoluKasaAdet = Math.Max(0, kasaStok.DoluKasaAdet - adet);
        kasaStok.GuncellemeTarihi = DateTime.UtcNow;

        var hareket = new KasaHareket
        {
            KasaStokId = kasaStok.Id,
            HareketTipi = KasaHareketTipi.CikisBos,
            Adet = adet,
            Tarih = DateTime.Now,
            ReferansBelgeId = referansBelgeId,
            ReferansBelgeTipi = referansBelgeTipi,
            Aciklama = $"{adet} adet boş kasa iade edildi"
        };
        _context.KasaHareketleri.Add(hareket);

        await _context.SaveChangesAsync();
    }

    public async Task<RehinFisi> RehinAlAsync(Guid cariId, Guid kapTipiId, int adet)
    {
        var kapTipi = await _context.KapTipleri.FindAsync(kapTipiId)
            ?? throw new InvalidOperationException("Kap tipi bulunamadı");

        var fis = new RehinFisi
        {
            FisNo = await GenerateRehinFisNoAsync(),
            Tarih = DateTime.Today,
            CariId = cariId,
            KapTipiId = kapTipiId,
            IslemTipiAl = true,
            KasaAdet = adet,
            BirimBedel = kapTipi.RehinBedeli,
            ToplamTutar = adet * kapTipi.RehinBedeli,
            Odendi = false
        };

        _context.RehinFisleri.Add(fis);

        // Kasa stok güncelle
        var kasaStok = await GetOrCreateKasaStokAsync(cariId, kapTipiId);
        kasaStok.RehinToplam += fis.ToplamTutar;
        kasaStok.GuncellemeTarihi = DateTime.UtcNow;

        // Hareket kaydet
        var hareket = new KasaHareket
        {
            KasaStokId = kasaStok.Id,
            HareketTipi = KasaHareketTipi.RehinAl,
            Adet = adet,
            Tarih = DateTime.Now,
            ReferansBelgeId = fis.Id,
            ReferansBelgeTipi = "RehinFisi",
            Aciklama = $"{adet} adet kasa için {fis.ToplamTutar:N2} TL rehin alındı"
        };
        _context.KasaHareketleri.Add(hareket);

        await _context.SaveChangesAsync();
        return fis;
    }

    public async Task<RehinFisi> RehinIadeEtAsync(Guid cariId, Guid kapTipiId, int adet)
    {
        var kapTipi = await _context.KapTipleri.FindAsync(kapTipiId)
            ?? throw new InvalidOperationException("Kap tipi bulunamadı");

        var fis = new RehinFisi
        {
            FisNo = await GenerateRehinFisNoAsync(),
            Tarih = DateTime.Today,
            CariId = cariId,
            KapTipiId = kapTipiId,
            IslemTipiAl = false, // İade
            KasaAdet = adet,
            BirimBedel = kapTipi.RehinBedeli,
            ToplamTutar = adet * kapTipi.RehinBedeli,
            Odendi = true,
            OdemeTarihi = DateTime.Now
        };

        _context.RehinFisleri.Add(fis);

        // Kasa stok güncelle
        var kasaStok = await GetOrCreateKasaStokAsync(cariId, kapTipiId);
        kasaStok.RehinToplam = Math.Max(0, kasaStok.RehinToplam - fis.ToplamTutar);
        kasaStok.GuncellemeTarihi = DateTime.UtcNow;

        // Hareket kaydet
        var hareket = new KasaHareket
        {
            KasaStokId = kasaStok.Id,
            HareketTipi = KasaHareketTipi.RehinIade,
            Adet = adet,
            Tarih = DateTime.Now,
            ReferansBelgeId = fis.Id,
            ReferansBelgeTipi = "RehinFisi",
            Aciklama = $"{adet} adet kasa için {fis.ToplamTutar:N2} TL rehin iade edildi"
        };
        _context.KasaHareketleri.Add(hareket);

        await _context.SaveChangesAsync();
        return fis;
    }

    public async Task<IEnumerable<KasaHareket>> GetKasaHareketleriAsync(Guid cariId, Guid? kapTipiId = null, DateTime? baslangic = null, DateTime? bitis = null)
    {
        var query = _context.KasaHareketleri
            .Include(h => h.KasaStok)
            .ThenInclude(k => k.KapTipi)
            .Where(h => h.KasaStok.CariId == cariId);

        if (kapTipiId.HasValue)
            query = query.Where(h => h.KasaStok.KapTipiId == kapTipiId.Value);

        if (baslangic.HasValue)
            query = query.Where(h => h.Tarih >= baslangic.Value);

        if (bitis.HasValue)
            query = query.Where(h => h.Tarih <= bitis.Value);

        return await query.OrderByDescending(h => h.Tarih).ToListAsync();
    }

    public async Task<decimal> GetToplamKasaBorcuAsync(Guid cariId)
    {
        return await _context.KasaStokDurumlari
            .Where(k => k.CariId == cariId)
            .SumAsync(k => k.RehinToplam);
    }

    private async Task<KasaStokDurumu> GetOrCreateKasaStokAsync(Guid cariId, Guid kapTipiId)
    {
        var kasaStok = await _context.KasaStokDurumlari
            .FirstOrDefaultAsync(k => k.CariId == cariId && k.KapTipiId == kapTipiId);

        if (kasaStok == null)
        {
            kasaStok = new KasaStokDurumu
            {
                CariId = cariId,
                KapTipiId = kapTipiId,
                DoluKasaAdet = 0,
                BosKasaAdet = 0,
                RehinToplam = 0
            };
            _context.KasaStokDurumlari.Add(kasaStok);
            await _context.SaveChangesAsync();
        }

        return kasaStok;
    }

    private async Task<string> GenerateRehinFisNoAsync()
    {
        var today = DateTime.Today;
        var prefix = $"RF{today:yyyyMMdd}";
        
        var lastNo = await _context.RehinFisleri
            .Where(f => f.FisNo.StartsWith(prefix))
            .OrderByDescending(f => f.FisNo)
            .Select(f => f.FisNo)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (!string.IsNullOrEmpty(lastNo) && lastNo.Length > prefix.Length)
        {
            if (int.TryParse(lastNo.Substring(prefix.Length), out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }
    
    public async Task<IEnumerable<RehinFisi>> GetRehinFisleriAsync(DateTime baslangic, DateTime bitis)
    {
        return await _context.RehinFisleri
            .Include(r => r.Cari)
            .Include(r => r.KapTipi)
            .Where(r => r.Tarih >= baslangic && r.Tarih <= bitis)
            .OrderByDescending(r => r.Tarih)
            .ThenByDescending(r => r.OlusturmaTarihi)
            .ToListAsync();
    }
}
