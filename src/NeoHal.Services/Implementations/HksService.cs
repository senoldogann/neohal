using System;
using System.Threading.Tasks;
using NeoHal.Core.Entities;
using NeoHal.Services.Interfaces;

namespace NeoHal.Services.Implementations;

/// <summary>
/// Mock HKS Servisi - Gerçek devlet API'si yerine simülasyon yapar
/// </summary>
public class HksService : IHksService
{
    public async Task<HksBildirimSonuc> BildirimYapAsync(HksBildirim bildirim)
    {
        // Simülasyon: 500ms bekle
        await Task.Delay(500);

        // %90 başarı oranı
        var random = new Random();
        bool basarili = random.Next(100) < 90;

        if (basarili)
        {
            return new HksBildirimSonuc
            {
                Basarili = true,
                KunyeNo = Guid.NewGuid().ToString("N").Substring(0, 20).ToUpper(),
                ReferansNo = Guid.NewGuid().ToString()
            };
        }
        else
        {
            return new HksBildirimSonuc
            {
                Basarili = false,
                HataMesaji = "HKS Servisi geçici olarak yanıt vermiyor (Simülasyon)"
            };
        }
    }

    public async Task<string> KunyeAlAsync(Guid urunId, decimal miktar, string plaka)
    {
        await Task.Delay(300);
        // Örnek Künye Formatı: TR-07-2023-12345678
        return $"TR-07-{DateTime.Now.Year}-{new Random().Next(10000000, 99999999)}";
    }

    public async Task<bool> StokSorgulaAsync(string kunyeNo)
    {
        await Task.Delay(200);
        return true; // Her zaman var kabul et
    }
}
