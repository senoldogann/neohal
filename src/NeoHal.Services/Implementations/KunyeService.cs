using System;
using System.Threading.Tasks;
using NeoHal.Core.Entities;
using NeoHal.Core.Enums;
using NeoHal.Data.Context;
using NeoHal.Services.Interfaces;

namespace NeoHal.Services.Implementations;

public class KunyeService : IKunyeService
{
    private readonly IHksService _hksService;
    private readonly NeoHalDbContext _context;

    public KunyeService(IHksService hksService, NeoHalDbContext context)
    {
        _hksService = hksService;
        _context = context;
    }

    public async Task<string> GirisIrsaliyesiIcinKunyeOlusturAsync(GirisIrsaliyesi irsaliye)
    {
        if (irsaliye == null) throw new ArgumentNullException(nameof(irsaliye));

        // İrsaliyedeki ilk ürün için künye alalım (Basitleştirilmiş mantık)
        // Gerçekte her kalem için ayrı künye olabilir veya parti künyesi olabilir
        var ilkKalem = irsaliye.Kalemler.FirstOrDefault();
        if (ilkKalem == null) return string.Empty;

        var kunyeNo = await _hksService.KunyeAlAsync(
            ilkKalem.UrunId, 
            irsaliye.ToplamNet, 
            irsaliye.Plaka ?? "000000"
        );

        // İrsaliyeyi güncelle
        irsaliye.KunyeNo = kunyeNo;
        irsaliye.HksBildirimNo = Guid.NewGuid().ToString(); // Referans
        
        await _context.SaveChangesAsync();

        return kunyeNo;
    }

    public async Task SatisBildirimiYapAsync(SatisFaturasi fatura)
    {
        if (fatura == null) throw new ArgumentNullException(nameof(fatura));

        // Satış bildirimi yap
        var bildirim = new HksBildirim
        {
            BildirimTipi = "SATIS",
            ReferansBelgeId = fatura.Id,
            ReferansBelgeTipi = "SatisFaturasi",
            GonderimTarihi = DateTime.UtcNow
        };

        var sonuc = await _hksService.BildirimYapAsync(bildirim);

        if (sonuc.Basarili)
        {
            fatura.HksBildirimNo = sonuc.ReferansNo;
            await _context.SaveChangesAsync();
        }
        else
        {
            // Hata loglanabilir veya faturaya not düşülebilir
            fatura.Aciklama += $" [HKS Hatası: {sonuc.HataMesaji}]";
            await _context.SaveChangesAsync();
        }
    }
}
