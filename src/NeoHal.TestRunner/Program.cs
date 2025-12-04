using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NeoHal.Core.Entities;
using NeoHal.Core.Enums;
using NeoHal.Data.Context;
using NeoHal.Services;
using NeoHal.Services.Interfaces;

namespace NeoHal.TestRunner;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("NeoHal Test Runner Başlatılıyor...");

        // 1. Servisleri Hazırla
        var services = new ServiceCollection();
        
        // InMemory DB
        services.AddDbContext<NeoHalDbContext>(options =>
            options.UseInMemoryDatabase("NeoHalTestDb"));
            
        // Servisleri Kaydet
        services.AddNeoHalServices();
        
        var provider = services.BuildServiceProvider();
        
        // 2. Test Senaryosu: Giriş İrsaliyesi ve Künye
        Console.WriteLine("\n--- TEST 1: Giriş İrsaliyesi ve Künye ---");
        await TestGirisVeKunye(provider);
        
        // 3. Test Senaryosu: Satış ve Vergi
        Console.WriteLine("\n--- TEST 2: Satış Faturası ve Vergi ---");
        await TestSatisVeVergi(provider);
        
        // 4. Test Senaryosu: Finansal Entegrasyon
        Console.WriteLine("\n--- TEST 3: Finansal Entegrasyon (Eksik Giderilenler) ---");
        await TestFinansalEntegrasyon(provider);
        
        Console.WriteLine("\nTestler Tamamlandı.");
    }
    
    static async Task TestGirisVeKunye(IServiceProvider provider)
    {
        var kunyeService = provider.GetRequiredService<IKunyeService>();
        var context = provider.GetRequiredService<NeoHalDbContext>();
        
        // Örnek Ürün
        var urun = new Urun { Id = Guid.NewGuid(), Ad = "Domates", Kod = "DOM001" };
        context.Urunler.Add(urun);
        await context.SaveChangesAsync();
        
        // İrsaliye
        var irsaliye = new GirisIrsaliyesi
        {
            Id = Guid.NewGuid(),
            Tarih = DateTime.Today,
            Plaka = "07 ABC 123",
            ToplamNet = 1000,
            Kalemler = new List<GirisIrsaliyesiKalem>
            {
                new GirisIrsaliyesiKalem { UrunId = urun.Id, NetKg = 1000, KapAdet = 100 }
            }
        };
        context.GirisIrsaliyeleri.Add(irsaliye);
        await context.SaveChangesAsync();
        
        // Künye Al
        Console.WriteLine("Künye alınıyor...");
        var kunyeNo = await kunyeService.GirisIrsaliyesiIcinKunyeOlusturAsync(irsaliye);
        
        Console.WriteLine($"Künye No: {kunyeNo}");
        
        if (!string.IsNullOrEmpty(kunyeNo) && kunyeNo.StartsWith("TR-"))
            Console.WriteLine("✅ BAŞARILI: Künye formatı doğru.");
        else
            Console.WriteLine("❌ HATA: Künye alınamadı veya format yanlış.");
    }
    
    static async Task TestSatisVeVergi(IServiceProvider provider)
    {
        var context = provider.GetRequiredService<NeoHalDbContext>();
        
        // Fatura
        var fatura = new SatisFaturasi
        {
            FaturaTipi = FaturaTipi.Toptan, // Kesinti olmalı
            Kalemler = new List<SatisFaturasiKalem>
            {
                new SatisFaturasiKalem
                {
                    Tutar = 10000, // 10.000 TL
                    RusumTutari = 100, // %1
                    KomisyonTutari = 800, // %8
                    StopajTutari = 200 // %2
                }
            }
        };
        
        // Hesapla
        fatura.HesaplaToplamlar();
        
        Console.WriteLine($"Ara Toplam: {fatura.AraToplam:C2}");
        Console.WriteLine($"Rüsum: {fatura.RusumTutari:C2}");
        Console.WriteLine($"Komisyon: {fatura.KomisyonTutari:C2}");
        Console.WriteLine($"Genel Toplam: {fatura.GenelToplam:C2}");
        
        if (fatura.RusumTutari == 100 && fatura.KomisyonTutari == 800)
            Console.WriteLine("✅ BAŞARILI: Kesintiler doğru hesaplandı.");
        else
            Console.WriteLine("❌ HATA: Kesinti hesabı yanlış.");
            
        // Sevkiyat Testi
        var sevkiyatFatura = new SatisFaturasi
        {
            FaturaTipi = FaturaTipi.Sevkiyat, // Kesinti OLMAMALI
            Kalemler = new List<SatisFaturasiKalem>
            {
                new SatisFaturasiKalem
                {
                    Tutar = 5000,
                    RusumTutari = 50, // Bunlar sıfırlanmalı
                    KomisyonTutari = 400
                }
            }
        };
        
        sevkiyatFatura.HesaplaToplamlar();
        Console.WriteLine($"\nSevkiyat Rüsum (Beklenen 0): {sevkiyatFatura.RusumTutari:C2}");
        
        if (sevkiyatFatura.RusumTutari == 0)
             Console.WriteLine("✅ BAŞARILI: Sevkiyat faturasında kesinti yok.");
        else
             Console.WriteLine("❌ HATA: Sevkiyat faturasında kesinti var!");
    }

    static async Task TestFinansalEntegrasyon(IServiceProvider provider)
    {
        var cekService = provider.GetRequiredService<ICekSenetService>();
        var kasaHesabiService = provider.GetRequiredService<IKasaHesabiService>();
        var cariService = provider.GetRequiredService<ICariHesapService>();
        var kasaTakipService = provider.GetRequiredService<IKasaTakipService>();
        var context = provider.GetRequiredService<NeoHalDbContext>();

        // 1. Çek Tahsilat Testi
        Console.WriteLine("\n[1] Çek Tahsilat Testi");
        var cek = new CekSenet
        {
            Id = Guid.NewGuid(),
            Tutar = 5000,
            VadeTarihi = DateTime.Today,
            BelgeNo = "CEK-001",
            CariId = Guid.NewGuid()
        };
        await cekService.CreateAsync(cek);
        
        Console.WriteLine("Çek tahsil ediliyor...");
        await cekService.TahsilEtAsync(cek.Id);
        
        var kasaBakiye = await kasaHesabiService.GetBakiyeAsync();
        Console.WriteLine($"Kasa Bakiyesi: {kasaBakiye:C2}");
        
        if (kasaBakiye == 5000)
            Console.WriteLine("✅ BAŞARILI: Çek tahsilatı kasaya işlendi.");
        else
            Console.WriteLine("❌ HATA: Çek tahsilatı kasaya yansımadı!");

        // 2. Risk Limiti Testi (Rehin Dahil)
        Console.WriteLine("\n[2] Risk Limiti Testi (Rehin Dahil)");
        var musteri = new CariHesap
        {
            Id = Guid.NewGuid(),
            Unvan = "Riskli Müşteri",
            RiskLimiti = 10000, // 10.000 TL Limit
            Kod = "RISK001"
        };
        await cariService.CreateAsync(musteri);
        
        // Kap Tipi Oluştur
        var kapTipi = new KapTipi { Id = Guid.NewGuid(), Ad = "Plastik Kasa", RehinBedeli = 100 };
        context.KapTipleri.Add(kapTipi);
        await context.SaveChangesAsync();
        
        // Müşteriye 60 adet kasa ver (60 * 100 = 6000 TL Rehin Riski)
        await kasaTakipService.RehinAlAsync(musteri.Id, kapTipi.Id, 60);
        
        // Şimdi 5000 TL'lik mal satmaya çalışalım
        // Toplam Risk = 0 (Nakit) + 6000 (Rehin) + 5000 (Yeni) = 11.000 > 10.000
        var riskDurumu = await cariService.CheckRiskLimitiAsync(musteri.Id, 5000);
        
        Console.WriteLine($"Risk Limiti: {riskDurumu.RiskLimiti:C2}");
        Console.WriteLine($"Mevcut Risk (Rehin Dahil): {riskDurumu.MevcutBakiye:C2}");
        Console.WriteLine($"Limit Aşıldı mı?: {riskDurumu.LimitAsildi}");
        
        if (riskDurumu.LimitAsildi && riskDurumu.MevcutBakiye == 6000)
             Console.WriteLine("✅ BAŞARILI: Rehin riski limite dahil edildi.");
        else
             Console.WriteLine("❌ HATA: Risk hesabı yanlış.");
    }
}
