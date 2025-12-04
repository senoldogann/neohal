using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using NeoHal.Desktop.ViewModels;
using NeoHal.Desktop.Views;
using NeoHal.Data.Context;
using NeoHal.Services;
using System;

namespace NeoHal.Desktop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<NeoHalDbContext>();
            context.Database.Migrate();
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Logging servisi ekle
        services.AddLogging();
        
        var dbPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NeoHal", "neohal.db");
        
        var dbDir = System.IO.Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dbDir) && !System.IO.Directory.Exists(dbDir))
        {
            System.IO.Directory.CreateDirectory(dbDir);
        }

        services.AddDbContext<NeoHalDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}")
                   .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

        services.AddNeoHalServices();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<CariHesapViewModel>();
        services.AddTransient<UrunViewModel>();
        services.AddTransient<UrunGrubuViewModel>();
        services.AddTransient<KapTipiViewModel>();
        services.AddTransient<GirisIrsaliyesiListViewModel>();
        services.AddTransient<GirisIrsaliyesiEditViewModel>();
        services.AddTransient<SatisFaturasiListViewModel>();
        services.AddTransient<SatisFaturasiEditViewModel>();
        services.AddTransient<KasaHesabiViewModel>();
        services.AddTransient<HizliSatisViewModel>();
        services.AddTransient<HalKayitViewModel>();
        services.AddTransient<SevkiyatGirisViewModel>();
        services.AddTransient<SubeBorcRaporuViewModel>();
        services.AddTransient<FaturaListesiViewModel>();
        services.AddTransient<StokDurumuViewModel>();
        services.AddTransient<CariExtreViewModel>();
        services.AddTransient<KasaTakipViewModel>();
        services.AddTransient<GunlukRaporViewModel>();
        services.AddTransient<SubeTahsilatViewModel>();
        services.AddTransient<KullaniciViewModel>();
        services.AddTransient<BackupViewModel>();
        services.AddTransient<RaporViewModel>();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
