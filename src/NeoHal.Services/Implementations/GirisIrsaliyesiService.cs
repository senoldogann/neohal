using Microsoft.EntityFrameworkCore;
using NeoHal.Core.Entities;
using NeoHal.Core.Enums;
using NeoHal.Data.Context;
using NeoHal.Services.Interfaces;

namespace NeoHal.Services.Implementations;

public class GirisIrsaliyesiService : IGirisIrsaliyesiService
{
    private readonly NeoHalDbContext _context;
    private readonly IKasaTakipService _kasaTakipService;

    public GirisIrsaliyesiService(NeoHalDbContext context, IKasaTakipService kasaTakipService)
    {
        _context = context;
        _kasaTakipService = kasaTakipService;
    }

    public async Task<IEnumerable<GirisIrsaliyesi>> GetAllAsync()
    {
        return await _context.GirisIrsaliyeleri
            .Include(i => i.Mustahsil)
            .Include(i => i.Kalemler)
            .OrderByDescending(i => i.Tarih)
            .ThenByDescending(i => i.OlusturmaTarihi)
            .ToListAsync();
    }

    public async Task<IEnumerable<GirisIrsaliyesi>> GetByDateRangeAsync(DateTime baslangic, DateTime bitis)
    {
        // Bitiş tarihinin gün sonuna kadar dahil olmasını sağla
        var bitisGunSonu = bitis.Date.AddDays(1).AddTicks(-1);
        
        return await _context.GirisIrsaliyeleri
            .Include(i => i.Mustahsil)
            .Include(i => i.Kalemler)
            .Where(i => i.Tarih >= baslangic.Date && i.Tarih <= bitisGunSonu)
            .OrderByDescending(i => i.Tarih)
            .ThenByDescending(i => i.OlusturmaTarihi)
            .ToListAsync();
    }

    public async Task<IEnumerable<GirisIrsaliyesi>> GetByMustahsilIdAsync(Guid mustahsilId)
    {
        return await _context.GirisIrsaliyeleri
            .Include(i => i.Kalemler)
            .Where(i => i.MustahsilId == mustahsilId)
            .OrderByDescending(i => i.Tarih)
            .ToListAsync();
    }

    public async Task<GirisIrsaliyesi?> GetByIdAsync(Guid id)
    {
        return await _context.GirisIrsaliyeleri
            .Include(i => i.Mustahsil)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<GirisIrsaliyesi?> GetByIdWithKalemlerAsync(Guid id)
    {
        return await _context.GirisIrsaliyeleri
            .Include(i => i.Mustahsil)
            .Include(i => i.Sevkiyatci)
            .Include(i => i.Nakliyeci)
            .Include(i => i.Kalemler)
                .ThenInclude(k => k.Urun)
            .Include(i => i.Kalemler)
                .ThenInclude(k => k.KapTipi)
            .Include(i => i.Kalemler)
                .ThenInclude(k => k.Komisyoncu)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<GirisIrsaliyesi> CreateAsync(GirisIrsaliyesi irsaliye)
    {
        irsaliye.IrsaliyeNo = await GenerateNewIrsaliyeNoAsync();
        irsaliye.OlusturmaTarihi = DateTime.UtcNow;
        irsaliye.Durum = BelgeDurumu.Taslak;

        // Toplamları hesapla
        RecalculateTotals(irsaliye);

        // Kalemler için kalan miktarları ayarla
        foreach (var kalem in irsaliye.Kalemler)
        {
            kalem.KalanKapAdet = kalem.KapAdet;
            kalem.KalanKg = kalem.NetKg;
        }

        _context.GirisIrsaliyeleri.Add(irsaliye);
        await _context.SaveChangesAsync();
        return irsaliye;
    }

    public async Task UpdateAsync(GirisIrsaliyesi irsaliye)
    {
        // 1. Mevcut irsaliyeyi veritabanından (tracked olarak) getir
        var existingIrsaliye = await _context.GirisIrsaliyeleri
            .Include(i => i.Kalemler)
            .FirstOrDefaultAsync(i => i.Id == irsaliye.Id);

        if (existingIrsaliye == null)
        {
            // Eğer bulunamazsa, belki yeni oluşturulmuştur ama context bilmiyordur.
            // Bu durumda Update yerine Add yapamayız çünkü ID çakışabilir.
            // Kullanıcıya net hata dönelim.
            throw new InvalidOperationException($"Güncellenecek irsaliye bulunamadı! (Id: {irsaliye.Id})");
        }

        // 2. Header bilgilerini güncelle
        existingIrsaliye.Tarih = irsaliye.Tarih;
        existingIrsaliye.MustahsilId = irsaliye.MustahsilId;
        existingIrsaliye.Aciklama = irsaliye.Aciklama;
        existingIrsaliye.Durum = irsaliye.Durum;
        existingIrsaliye.GuncellemeTarihi = DateTime.UtcNow;

        // 3. Kalemleri güncelle (Silip yeniden ekleme yöntemi)
        // Mevcut kalemleri context'ten sil
        _context.GirisIrsaliyesiKalemleri.RemoveRange(existingIrsaliye.Kalemler);
        
        // Yeni kalemleri ekle
        foreach (var kalem in irsaliye.Kalemler)
        {
            // Yeni kalem nesneleri oluşturuyoruz ki ID çakışması olmasın
            // Eğer kalem.Id zaten varsa ve DB'de yoksa sorun yok.
            // Ama en temizi, gelen kalemleri yeni entity olarak eklemektir.
            // Ancak UI tarafında ID'ler korunmuş olabilir.
            
            // Eğer UI'dan gelen kalem zaten tracked ise sorun çıkabilir.
            // Bu yüzden yeni bir instance oluşturup değerleri kopyalamak en güvenlisidir.
            // Veya direkt Add yaparız, EF Core ID'si varsa Insert Identity yapmaya çalışabilir.
            
            // Basit yöntem: Direkt ekle, ancak IrsaliyeId'yi güncelle
            kalem.IrsaliyeId = existingIrsaliye.Id;
            kalem.KalanKapAdet = kalem.KapAdet;
            kalem.KalanKg = kalem.NetKg;
            
            // Eğer kalem context tarafından izleniyorsa, Detach etmemiz gerekebilir.
            // Ama RemoveRange zaten sildi.
            
            _context.GirisIrsaliyesiKalemleri.Add(kalem);
        }

        // 4. Toplamları hesapla
        RecalculateTotals(existingIrsaliye);
        
        // 5. Kaydet
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var irsaliye = await _context.GirisIrsaliyeleri.FindAsync(id);
        if (irsaliye != null)
        {
            irsaliye.IsDeleted = true;
            irsaliye.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateNewIrsaliyeNoAsync()
    {
        var today = DateTime.Today;
        var prefix = $"GI{today:yyyyMMdd}";

        var lastNo = await _context.GirisIrsaliyeleri
            .IgnoreQueryFilters()
            .Where(i => i.IrsaliyeNo.StartsWith(prefix))
            .OrderByDescending(i => i.IrsaliyeNo)
            .Select(i => i.IrsaliyeNo)
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
        var irsaliye = await GetByIdWithKalemlerAsync(id);
        if (irsaliye == null) throw new InvalidOperationException("İrsaliye bulunamadı");
        if (irsaliye.Durum != BelgeDurumu.Taslak) throw new InvalidOperationException("Sadece taslak irsaliyeler onaylanabilir");

        // Her kalem için kasa girişi yap
        foreach (var kalem in irsaliye.Kalemler)
        {
            await _kasaTakipService.DoluKasaGirisiAsync(
                irsaliye.MustahsilId,
                kalem.KapTipiId,
                kalem.KapAdet,
                irsaliye.Id,
                "Irsaliye"
            );
        }

        irsaliye.Durum = BelgeDurumu.Onaylandi;
        irsaliye.GuncellemeTarihi = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task IptalEtAsync(Guid id)
    {
        var irsaliye = await _context.GirisIrsaliyeleri.FindAsync(id);
        if (irsaliye == null) throw new InvalidOperationException("İrsaliye bulunamadı");

        irsaliye.Durum = BelgeDurumu.Iptal;
        irsaliye.GuncellemeTarihi = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Belirtilen tarihe ait aktif irsaliyeyi getirir.
    /// Hal Kayıt'ta aynı güne ait tüm alımlar tek irsaliyede toplanır.
    /// </summary>
    public async Task<GirisIrsaliyesi?> GetByTarihAsync(DateTime tarih)
    {
        var gunBaslangic = tarih.Date;
        var gunBitis = gunBaslangic.AddDays(1).AddTicks(-1);
        
        return await _context.GirisIrsaliyeleri
            .Include(i => i.Mustahsil)
            .Include(i => i.Kalemler)
                .ThenInclude(k => k.Urun)
            .Include(i => i.Kalemler)
                .ThenInclude(k => k.KapTipi)
            .Include(i => i.Kalemler)
                .ThenInclude(k => k.Komisyoncu)
            .Where(i => i.Tarih >= gunBaslangic && i.Tarih <= gunBitis)
            .Where(i => i.Durum != BelgeDurumu.Iptal) // İptal edilmişleri hariç tut
            .OrderByDescending(i => i.OlusturmaTarihi)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Mevcut irsaliyeye yeni kalemler ekler ve toplamları günceller
    /// </summary>
    public async Task AddKalemlerAsync(Guid irsaliyeId, IEnumerable<GirisIrsaliyesiKalem> kalemler)
    {
        var irsaliye = await _context.GirisIrsaliyeleri
            .Include(i => i.Kalemler)
            .FirstOrDefaultAsync(i => i.Id == irsaliyeId);
            
        if (irsaliye == null)
            throw new InvalidOperationException("İrsaliye bulunamadı");

        foreach (var kalem in kalemler)
        {
            kalem.IrsaliyeId = irsaliyeId;
            kalem.KalanKapAdet = kalem.KapAdet;
            kalem.KalanKg = kalem.NetKg;
            irsaliye.Kalemler.Add(kalem);
        }

        // Toplamları yeniden hesapla
        RecalculateTotals(irsaliye);
        irsaliye.GuncellemeTarihi = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
    }

    private void RecalculateTotals(GirisIrsaliyesi irsaliye)
    {
        irsaliye.ToplamBrut = irsaliye.Kalemler.Sum(k => k.BrutKg);
        irsaliye.ToplamDara = irsaliye.Kalemler.Sum(k => k.DaraKg);
        irsaliye.ToplamNet = irsaliye.Kalemler.Sum(k => k.NetKg);
        irsaliye.ToplamKapAdet = irsaliye.Kalemler.Sum(k => k.KapAdet);
    }
}
