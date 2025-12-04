using Microsoft.EntityFrameworkCore;
using NeoHal.Core.Entities;
using NeoHal.Core.Enums;
using NeoHal.Data.Context;
using NeoHal.Services.Interfaces;

namespace NeoHal.Services.Implementations;

public class SatisFaturasiService : ISatisFaturasiService
{
    private readonly NeoHalDbContext _context;
    private readonly IKasaTakipService _kasaTakipService;

    public SatisFaturasiService(NeoHalDbContext context, IKasaTakipService kasaTakipService)
    {
        _context = context;
        _kasaTakipService = kasaTakipService;
    }

    public async Task<IEnumerable<SatisFaturasi>> GetAllAsync()
    {
        return await _context.SatisFaturalari
            .Include(f => f.Alici)
            .Include(f => f.Mustahsil)
            .Include(f => f.Kalemler)
            .OrderByDescending(f => f.FaturaTarihi)
            .ThenByDescending(f => f.OlusturmaTarihi)
            .ToListAsync();
    }

    public async Task<IEnumerable<SatisFaturasi>> GetByDateRangeAsync(DateTime baslangic, DateTime bitis)
    {
        // Bitiş tarihinin gün sonuna kadar dahil olmasını sağla
        var bitisGunSonu = bitis.Date.AddDays(1).AddTicks(-1);

        return await _context.SatisFaturalari
            .Include(f => f.Alici)
            .Include(f => f.Mustahsil)
            .Include(f => f.Kalemler)
            .Where(f => f.FaturaTarihi >= baslangic.Date && f.FaturaTarihi <= bitisGunSonu)
            .OrderByDescending(f => f.FaturaTarihi)
            .ToListAsync();
    }

    public async Task<IEnumerable<SatisFaturasi>> GetByAliciIdAsync(Guid aliciId)
    {
        return await _context.SatisFaturalari
            .Include(f => f.Kalemler)
            .Where(f => f.AliciId == aliciId)
            .OrderByDescending(f => f.FaturaTarihi)
            .ToListAsync();
    }

    public async Task<IEnumerable<SatisFaturasi>> GetByMustahsilIdAsync(Guid mustahsilId)
    {
        return await _context.SatisFaturalari
            .Include(f => f.Kalemler)
            .Where(f => f.MustahsilId == mustahsilId)
            .OrderByDescending(f => f.FaturaTarihi)
            .ToListAsync();
    }

    public async Task<SatisFaturasi?> GetByIdAsync(Guid id)
    {
        return await _context.SatisFaturalari
            .Include(f => f.Alici)
            .Include(f => f.Mustahsil)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<SatisFaturasi?> GetByIdWithKalemlerAsync(Guid id)
    {
        return await _context.SatisFaturalari
            .Include(f => f.Alici)
            .Include(f => f.Mustahsil)
            .Include(f => f.Kalemler)
                .ThenInclude(k => k.GirisKalem)
                    .ThenInclude(gk => gk!.Urun)
            .Include(f => f.Kalemler)
                .ThenInclude(k => k.GirisKalem)
                    .ThenInclude(gk => gk!.KapTipi)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<SatisFaturasi> CreateAsync(SatisFaturasi fatura)
    {
        fatura.FaturaNo = await GenerateNewFaturaNoAsync();
        fatura.OlusturmaTarihi = DateTime.UtcNow;
        fatura.Durum = BelgeDurumu.Taslak;

        RecalculateTotals(fatura);

        _context.SatisFaturalari.Add(fatura);
        await _context.SaveChangesAsync();
        return fatura;
    }

    public async Task UpdateAsync(SatisFaturasi fatura)
    {
        RecalculateTotals(fatura);
        fatura.GuncellemeTarihi = DateTime.UtcNow;
        _context.SatisFaturalari.Update(fatura);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var fatura = await _context.SatisFaturalari.FindAsync(id);
        if (fatura != null)
        {
            fatura.IsDeleted = true;
            fatura.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateNewFaturaNoAsync()
    {
        var today = DateTime.Today;
        var prefix = $"SF{today:yyyyMMdd}";

        var lastNo = await _context.SatisFaturalari
            .IgnoreQueryFilters()
            .Where(f => f.FaturaNo.StartsWith(prefix))
            .OrderByDescending(f => f.FaturaNo)
            .Select(f => f.FaturaNo)
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

    public async Task OnaylaAsync(Guid id)
    {
        var fatura = await GetByIdWithKalemlerAsync(id);
        if (fatura == null) throw new InvalidOperationException("Fatura bulunamadı");
        if (fatura.Durum != BelgeDurumu.Taslak) throw new InvalidOperationException("Sadece taslak faturalar onaylanabilir");

        // Her kalem için kasa çıkışı yap (satış = müşteriye teslim)
        foreach (var kalem in fatura.Kalemler)
        {
            if (kalem.GirisKalem != null)
            {
                await _kasaTakipService.DoluKasaCikisiAsync(
                    fatura.AliciId,
                    kalem.KapTipiId,
                    kalem.KapAdet,
                    fatura.Id,
                    "Fatura"
                );
            }
        }

        fatura.Durum = BelgeDurumu.Onaylandi;
        fatura.GuncellemeTarihi = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task IptalEtAsync(Guid id)
    {
        var fatura = await _context.SatisFaturalari.FindAsync(id);
        if (fatura == null) throw new InvalidOperationException("Fatura bulunamadı");

        fatura.Durum = BelgeDurumu.Iptal;
        fatura.GuncellemeTarihi = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<SatisFaturasi> CreateFromIrsaliyeKalemAsync(Guid irsaliyeKalemId, Guid aliciId, decimal fiyat, int kapAdet)
    {
        // İrsaliye kalemini bul
        var irsaliyeKalemi = await _context.GirisIrsaliyesiKalemleri
            .Include(k => k.Irsaliye)
            .Include(k => k.Urun)
            .Include(k => k.KapTipi)
            .FirstOrDefaultAsync(k => k.Id == irsaliyeKalemId);

        if (irsaliyeKalemi == null)
            throw new InvalidOperationException("İrsaliye kalemi bulunamadı");

        if (kapAdet > irsaliyeKalemi.KalanKapAdet)
            throw new InvalidOperationException($"Kalan kap adedi yetersiz. Mevcut: {irsaliyeKalemi.KalanKapAdet}");

        // Alıcıyı bul
        var alici = await _context.CariHesaplar.FindAsync(aliciId);
        if (alici == null)
            throw new InvalidOperationException("Alıcı bulunamadı");

        // Kg hesapla (orantılı)
        decimal kg = (irsaliyeKalemi.NetKg / irsaliyeKalemi.KapAdet) * kapAdet;

        // Yeni fatura oluştur
        var fatura = new SatisFaturasi
        {
            FaturaTarihi = DateTime.Today,
            AliciId = aliciId,
            MustahsilId = irsaliyeKalemi.Irsaliye!.MustahsilId,
            Aciklama = $"İrsaliye No: {irsaliyeKalemi.Irsaliye.IrsaliyeNo}",
            Kalemler = new List<SatisFaturasiKalem>
            {
                new SatisFaturasiKalem
                {
                    GirisKalemId = irsaliyeKalemId,
                    UrunId = irsaliyeKalemi.UrunId,
                    KapTipiId = irsaliyeKalemi.KapTipiId,
                    KapAdet = kapAdet,
                    BrutKg = kg + (irsaliyeKalemi.DaraKg / irsaliyeKalemi.KapAdet * kapAdet),
                    DaraKg = irsaliyeKalemi.DaraKg / irsaliyeKalemi.KapAdet * kapAdet,
                    NetKg = kg,
                    BirimFiyat = fiyat,
                    Tutar = kg * fiyat
                }
            }
        };

        // Faturayı kaydet
        var savedFatura = await CreateAsync(fatura);

        // İrsaliye kaleminin kalan miktarını güncelle
        irsaliyeKalemi.KalanKapAdet -= kapAdet;
        irsaliyeKalemi.KalanKg -= kg;
        await _context.SaveChangesAsync();

        return savedFatura;
    }

    private void RecalculateTotals(SatisFaturasi fatura)
    {
        // Fatura tipine göre toplamları hesapla
        fatura.HesaplaToplamlar();
    }
    
    public async Task<IEnumerable<SatisFaturasi>> GetByFaturaTipiAsync(FaturaTipi tip)
    {
        return await _context.SatisFaturalari
            .Include(f => f.Alici)
            .Include(f => f.Kalemler)
            .Where(f => f.FaturaTipi == tip)
            .OrderByDescending(f => f.FaturaTarihi)
            .ToListAsync();
    }
    
    /// <summary>
    /// Sevkiyatçı satış faturası oluşturur - KESİNTİ UYGULANMAZ
    /// </summary>
    public async Task<SatisFaturasi> CreateSevkiyatFaturasiAsync(SatisFaturasi fatura)
    {
        // Sevkiyat tipi zorla
        fatura.FaturaTipi = FaturaTipi.Sevkiyat;
        
        // Tüm kalemlerde kesintileri sıfırla
        foreach (var kalem in fatura.Kalemler)
        {
            kalem.RusumOrani = 0;
            kalem.RusumTutari = 0;
            kalem.KomisyonOrani = 0;
            kalem.KomisyonTutari = 0;
            kalem.StopajOrani = 0;
            kalem.StopajTutari = 0;
            
            // Tutar = Net x Birim Fiyat
            kalem.Tutar = kalem.NetKg * kalem.BirimFiyat;
        }
        
        // Müstahsil bağlantısı kaldır (sevkiyatçı müstahsille işlem yapmaz)
        fatura.MustahsilId = null;
        
        return await CreateAsync(fatura);
    }
    
    /// <summary>
    /// İç transfer faturası oluşturur - Maliyet fiyatı üzerinden
    /// </summary>
    public async Task<SatisFaturasi> CreateIcTransferFaturasiAsync(SatisFaturasi fatura)
    {
        // İç transfer tipi zorla
        fatura.FaturaTipi = FaturaTipi.IcTransfer;
        
        // Tüm kalemlerde kesintileri sıfırla ve maliyet fiyatı kullan
        foreach (var kalem in fatura.Kalemler)
        {
            kalem.RusumOrani = 0;
            kalem.RusumTutari = 0;
            kalem.KomisyonOrani = 0;
            kalem.KomisyonTutari = 0;
            kalem.StopajOrani = 0;
            kalem.StopajTutari = 0;
            
            // İç transferde alış fiyatı kullan (kar yok)
            if (kalem.AlisFiyati > 0)
            {
                kalem.BirimFiyat = kalem.AlisFiyati;
            }
            
            kalem.Tutar = kalem.NetKg * kalem.BirimFiyat;
        }
        
        fatura.MustahsilId = null;
        
        return await CreateAsync(fatura);
    }
    
    /// <summary>
    /// Fatura için otomatik rehin fişi oluşturur
    /// </summary>
    public async Task CreateRehinFisiForFaturaAsync(Guid faturaId)
    {
        var fatura = await GetByIdWithKalemlerAsync(faturaId);
        if (fatura == null) throw new InvalidOperationException("Fatura bulunamadı");
        
        // Faturadaki kapları topla
        var kaplar = fatura.Kalemler
            .GroupBy(k => k.KapTipiId)
            .Select(g => new 
            { 
                KapTipiId = g.Key, 
                Adet = g.Sum(k => k.KapAdet) 
            })
            .ToList();
            
        foreach (var kap in kaplar)
        {
            if (kap.Adet > 0)
            {
                await _kasaTakipService.RehinAlAsync(fatura.AliciId, kap.KapTipiId, kap.Adet);
            }
        }
    }
}
