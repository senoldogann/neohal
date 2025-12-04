using Microsoft.Extensions.DependencyInjection;
using NeoHal.Services.Implementations;
using NeoHal.Services.Interfaces;

namespace NeoHal.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNeoHalServices(this IServiceCollection services)
    {
        // Cari Hesap
        services.AddScoped<ICariHesapService, CariHesapService>();
        
        // Kasa Takip
        services.AddScoped<IKasaTakipService, KasaTakipService>();
        
        // Ürün Yönetimi
        services.AddScoped<IUrunService, UrunService>();
        services.AddScoped<IKapTipiService, KapTipiService>();
        services.AddScoped<IUrunGrubuService, UrunGrubuService>();
        
        // Operasyon
        services.AddScoped<IGirisIrsaliyesiService, GirisIrsaliyesiService>();
        services.AddScoped<ISatisFaturasiService, SatisFaturasiService>();
        
        // Finans
        services.AddScoped<IKasaHesabiService, KasaHesabiService>();
        services.AddScoped<ICariHareketService, CariHareketService>();
        services.AddScoped<IKasaStokService, KasaStokService>();
        services.AddScoped<ICekSenetService, CekSenetService>();
        
        // Stok
        services.AddScoped<IStokService, StokService>();
        
        // Kullanıcı Yönetimi
        services.AddSingleton<IKullaniciService, KullaniciService>();
        
        // Yedekleme
        services.AddSingleton<IBackupService, BackupService>();
        
        // Raporlama
        services.AddScoped<IRaporService, RaporService>();
        
        // HKS Entegrasyon
        services.AddScoped<IHksService, HksService>();
        services.AddScoped<IKunyeService, KunyeService>();
        
        return services;
    }
}
