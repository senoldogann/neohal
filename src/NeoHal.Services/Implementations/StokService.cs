using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NeoHal.Core.Entities;
using NeoHal.Data.Context;
using NeoHal.Services.Interfaces;

namespace NeoHal.Services.Implementations;

/// <summary>
/// Stok takip servisi - FIFO bazlı stok yönetimi
/// </summary>
public class StokService : IStokService
{
    private readonly NeoHalDbContext _context;

    public StokService(NeoHalDbContext context)
    {
        _context = context;
    }

    public async Task<List<StokDurumuItem>> GetStokDurumuAsync(Guid? urunId = null, bool sadeceMevcut = true)
    {
        var query = _context.Set<GirisIrsaliyesiKalem>()
            .Include(k => k.Urun)
            .ThenInclude(u => u.Grup)
            .Include(k => k.Irsaliye)
            .ThenInclude(i => i.Mustahsil)
            .AsQueryable();

        if (urunId.HasValue)
        {
            query = query.Where(k => k.UrunId == urunId.Value);
        }

        if (sadeceMevcut)
        {
            query = query.Where(k => k.KalanKg > 0);
        }

        var kalemler = await query.ToListAsync();

        // Ürün bazlı gruplama
        var stoklar = kalemler
            .GroupBy(k => k.UrunId)
            .Select(g => new StokDurumuItem
            {
                UrunId = g.Key,
                UrunKodu = g.First().Urun?.Kod ?? "",
                UrunAdi = g.First().Urun?.Ad ?? "",
                UrunGrubu = g.First().Urun?.Grup?.Ad ?? "",
                ToplamKalanKg = g.Sum(k => k.KalanKg),
                ToplamKalanKap = g.Sum(k => k.KalanKapAdet),
                LotSayisi = g.Count(k => k.KalanKg > 0),
                SonGirisTarihi = g.Max(k => k.Irsaliye?.Tarih),
                SonGirisKaynagi = g.OrderByDescending(k => k.Irsaliye?.Tarih).First().Irsaliye?.Mustahsil?.Unvan ?? "",
                OrtalamaFiyat = g.Where(k => k.KalanKg > 0 && k.BirimFiyat.HasValue).Any() 
                    ? g.Where(k => k.KalanKg > 0 && k.BirimFiyat.HasValue)
                       .Average(k => k.BirimFiyat ?? 0)
                    : 0
            })
            .OrderBy(s => s.UrunAdi)
            .ToList();

        return stoklar;
    }

    public async Task<List<StokDetayItem>> GetStokDetayAsync(Guid urunId)
    {
        var kalemler = await _context.Set<GirisIrsaliyesiKalem>()
            .Include(k => k.Irsaliye)
            .ThenInclude(i => i.Mustahsil)
            .Include(k => k.KapTipi)
            .Where(k => k.UrunId == urunId)
            .OrderBy(k => k.Irsaliye!.Tarih) // FIFO sırası
            .ThenBy(k => k.OlusturmaTarihi)
            .ToListAsync();

        int sira = 1;
        var detaylar = kalemler.Select(k => new StokDetayItem
        {
            IrsaliyeKalemId = k.Id,
            IrsaliyeNo = k.Irsaliye?.IrsaliyeNo ?? "",
            GirisTarihi = k.Irsaliye?.Tarih ?? DateTime.MinValue,
            MustahsilAdi = k.Irsaliye?.Mustahsil?.Unvan ?? "",
            KapTipiAdi = k.KapTipi?.Ad ?? "",
            BaslangicKap = k.KapAdet,
            BaslangicKg = k.NetKg,
            KalanKap = k.KalanKapAdet,
            KalanKg = k.KalanKg,
            BirimFiyat = k.BirimFiyat ?? 0,
            FifoSira = sira++
        }).ToList();

        return detaylar;
    }

    public async Task<List<StokRezervasyon>> RezerveStokAsync(Guid urunId, decimal miktar)
    {
        var rezervasyonlar = new List<StokRezervasyon>();
        var kalanMiktar = miktar;

        // FIFO sırasına göre al
        var kalemler = await _context.Set<GirisIrsaliyesiKalem>()
            .Include(k => k.Irsaliye)
            .Where(k => k.UrunId == urunId && k.KalanKg > 0)
            .OrderBy(k => k.Irsaliye!.Tarih)
            .ThenBy(k => k.OlusturmaTarihi)
            .ToListAsync();

        foreach (var kalem in kalemler)
        {
            if (kalanMiktar <= 0) break;

            var alinacak = Math.Min(kalem.KalanKg, kalanMiktar);
            rezervasyonlar.Add(new StokRezervasyon
            {
                IrsaliyeKalemId = kalem.Id,
                Miktar = alinacak,
                BirimFiyat = kalem.BirimFiyat ?? 0
            });

            kalanMiktar -= alinacak;
        }

        if (kalanMiktar > 0)
        {
            throw new InvalidOperationException($"Yetersiz stok! {kalanMiktar:N2} kg eksik.");
        }

        return rezervasyonlar;
    }

    public async Task DusStokAsync(List<StokRezervasyon> rezervasyonlar)
    {
        foreach (var rez in rezervasyonlar)
        {
            var kalem = await _context.Set<GirisIrsaliyesiKalem>()
                .FindAsync(rez.IrsaliyeKalemId);

            if (kalem != null)
            {
                kalem.KalanKg -= rez.Miktar;
                
                // Kap adedini oranla düş
                if (kalem.NetKg > 0)
                {
                    var oran = rez.Miktar / kalem.NetKg;
                    var dusulecekKap = (int)Math.Round(kalem.KapAdet * oran);
                    kalem.KalanKapAdet = Math.Max(0, kalem.KalanKapAdet - dusulecekKap);
                }
            }
        }

        await _context.SaveChangesAsync();
    }
}
