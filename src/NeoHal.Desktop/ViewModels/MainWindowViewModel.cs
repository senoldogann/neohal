using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NeoHal.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NeoHal.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ICariHesapService _cariHesapService;
    private readonly IGirisIrsaliyesiService _irsaliyeService;
    private readonly ISatisFaturasiService _faturaService;
    private readonly IUrunService _urunService;

    [ObservableProperty]
    private string _title = "NeoHal - Hal Otomasyon Sistemi";

    [ObservableProperty]
    private string _statusMessage = "Hazır";

    [ObservableProperty]
    private string _activeModule = "Dashboard";

    [ObservableProperty]
    private ViewModelBase? _currentViewModel;

    // Dashboard istatistikleri
    [ObservableProperty]
    private int _toplamCariHesap;

    [ObservableProperty]
    private int _bugunkuIrsaliye;

    [ObservableProperty]
    private int _bugunkuFatura;

    [ObservableProperty]
    private int _toplamUrun;

    [ObservableProperty]
    private decimal _gunlukSatis;

    [ObservableProperty]
    private decimal _gunlukGiris;

    public MainWindowViewModel(
        ICariHesapService cariHesapService,
        IGirisIrsaliyesiService irsaliyeService,
        ISatisFaturasiService faturaService,
        IUrunService urunService)
    {
        _cariHesapService = cariHesapService;
        _irsaliyeService = irsaliyeService;
        _faturaService = faturaService;
        _urunService = urunService;
        
        _ = LoadDashboardStatsAsync();
    }

    private async Task LoadDashboardStatsAsync()
    {
        try
        {
            // Cari hesap sayısı
            var cariHesaplar = await _cariHesapService.GetAllAsync();
            ToplamCariHesap = cariHesaplar.Count();

            // Bugünkü irsaliyeler
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var irsaliyeler = await _irsaliyeService.GetByDateRangeAsync(today, tomorrow);
            BugunkuIrsaliye = irsaliyeler.Count();
            GunlukGiris = irsaliyeler.Sum(i => i.ToplamNet);

            // Bugünkü faturalar
            var faturalar = await _faturaService.GetByDateRangeAsync(today, tomorrow);
            BugunkuFatura = faturalar.Count();
            GunlukSatis = faturalar.Sum(f => f.GenelToplam);

            // Toplam ürün
            var urunler = await _urunService.GetAllAsync();
            ToplamUrun = urunler.Count();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Dashboard hata: {ex.Message}";
        }
    }

    [RelayCommand]
    private void NavigateTo(string module)
    {
        ActiveModule = module;
        StatusMessage = $"{module} modülü açılıyor...";

        try
        {
            CurrentViewModel = module switch
            {
                "CariHesaplar" => App.Services.GetRequiredService<CariHesapViewModel>(),
                "Urunler" => App.Services.GetRequiredService<UrunViewModel>(),
                "UrunGruplari" => App.Services.GetRequiredService<UrunGrubuViewModel>(),
                "KapTipleri" => App.Services.GetRequiredService<KapTipiViewModel>(),
                "GirisIrsaliye" => App.Services.GetRequiredService<GirisIrsaliyesiListViewModel>(),
                "TaslakIrsaliyeler" => CreateTaslakIrsaliyelerViewModel(),
                "SatisFatura" => App.Services.GetRequiredService<SatisFaturasiListViewModel>(),
                "HizliSatis" => App.Services.GetRequiredService<HizliSatisViewModel>(),
                "KasaHesabi" => App.Services.GetRequiredService<KasaHesabiViewModel>(),
                "KasaTakip" => App.Services.GetRequiredService<KasaTakipViewModel>(),
                "HalKayit" => App.Services.GetRequiredService<HalKayitViewModel>(),
                "SevkiyatGiris" => App.Services.GetRequiredService<SevkiyatGirisViewModel>(),
                "SubeBorcRaporu" => App.Services.GetRequiredService<SubeBorcRaporuViewModel>(),
                "FaturaListesi" => App.Services.GetRequiredService<FaturaListesiViewModel>(),
                "StokDurumu" => App.Services.GetRequiredService<StokDurumuViewModel>(),
                "CariEkstre" or "CariExtre" => App.Services.GetRequiredService<CariExtreViewModel>(),
                "GunlukRapor" or "Raporlar" => App.Services.GetRequiredService<GunlukRaporViewModel>(),
                "SubeTahsilat" => App.Services.GetRequiredService<SubeTahsilatViewModel>(),
                "Kullanicilar" or "KullaniciYonetimi" => App.Services.GetRequiredService<KullaniciViewModel>(),
                "Yedekleme" or "Backup" => App.Services.GetRequiredService<BackupViewModel>(),
                "RaporMerkezi" or "Raporlama" => App.Services.GetRequiredService<RaporViewModel>(),
                _ => null
            };
            
            if (CurrentViewModel != null)
            {
                StatusMessage = $"✅ {module} modülü açıldı.";
            }
            else
            {
                StatusMessage = $"⚠️ {module} modülü bulunamadı.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Hata: {ex.Message}";
            Console.WriteLine($"NavigateTo Error for {module}: {ex}");
        }
    }

    [RelayCommand]
    private void GoToDashboard()
    {
        ActiveModule = "Dashboard";
        CurrentViewModel = null;
        StatusMessage = "Ana sayfa";
        _ = LoadDashboardStatsAsync();
    }

    [RelayCommand]
    private async Task RefreshDashboardAsync()
    {
        await LoadDashboardStatsAsync();
    }

    // Menü Komutları
    [RelayCommand]
    private void NewCariHesap()
    {
        NavigateTo("CariHesaplar");
        StatusMessage = "Yeni cari hesap eklemek için formu kullanın.";
    }

    [RelayCommand]
    private async Task NewIrsaliyeAsync()
    {
        NavigateTo("GirisIrsaliye");
        StatusMessage = "Yeni irsaliye oluşturmak için 'Yeni' butonuna tıklayın.";
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task NewFaturaAsync()
    {
        NavigateTo("SatisFatura");
        StatusMessage = "Yeni fatura oluşturmak için 'Yeni' butonuna tıklayın.";
        await Task.CompletedTask;
    }

    /// <summary>
    /// Sadece taslak irsaliyeleri gösteren ViewModel oluşturur
    /// </summary>
    private GirisIrsaliyesiListViewModel CreateTaslakIrsaliyelerViewModel()
    {
        var vm = App.Services.GetRequiredService<GirisIrsaliyesiListViewModel>();
        vm.SadeceTaslaklar = true;
        return vm;
    }

    [RelayCommand]
    private void OpenSettings()
    {
        StatusMessage = "Ayarlar sayfası henüz eklenmedi.";
    }

    [RelayCommand]
    private void OpenCariExtre()
    {
        NavigateTo("CariHesaplar");
        StatusMessage = "Cari hesap seçerek ekstre görüntüleyebilirsiniz.";
    }

    [RelayCommand]
    private void OpenKasaRaporu()
    {
        NavigateTo("KasaHesabi");
        StatusMessage = "Kasa raporu görüntüleniyor.";
    }

    [RelayCommand]
    private void OpenGunlukSatis()
    {
        NavigateTo("SatisFatura");
        StatusMessage = "Günlük satış raporu için tarih filtresi kullanın.";
    }

    [RelayCommand]
    private void ShowAbout()
    {
        StatusMessage = "NeoHal v1.0.0 - Hal Otomasyon Sistemi - © 2025";
    }

    [RelayCommand]
    private void ExitApp()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}
