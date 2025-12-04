using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NeoHal.Core.Entities;
using NeoHal.Core.Enums;
using NeoHal.Desktop.Views;
using NeoHal.Services.Interfaces;

namespace NeoHal.Desktop.ViewModels;

public partial class GirisIrsaliyesiListViewModel : ViewModelBase
{
    private readonly IGirisIrsaliyesiService _irsaliyeService;
    private readonly ICariHesapService _cariHesapService;
    private readonly IUrunService _urunService;
    private readonly IKapTipiService _kapTipiService;

    [ObservableProperty]
    private ObservableCollection<GirisIrsaliyesi> _irsaliyeler = new();

    [ObservableProperty]
    private GirisIrsaliyesi? _selectedIrsaliye;

    [ObservableProperty]
    private DateTimeOffset? _filtreBaslangic = DateTimeOffset.Now.AddDays(-30);

    [ObservableProperty]
    private DateTimeOffset? _filtreBitis = DateTimeOffset.Now;

    [ObservableProperty]
    private string _statusMessage = string.Empty;
    
    /// <summary>
    /// True ise sadece Taslak durumundaki irsaliyeleri gösterir
    /// </summary>
    [ObservableProperty]
    private bool _sadeceTaslaklar = false;
    
    public string PageTitle => SadeceTaslaklar ? "📂 Taslak İrsaliyeler" : "📋 Giriş İrsaliyeleri";
    
    /// <summary>
    /// SadeceTaslaklar değiştiğinde veriyi yeniden yükle
    /// </summary>
    partial void OnSadeceTaslaklarChanged(bool value)
    {
        _ = LoadDataAsync();
        OnPropertyChanged(nameof(PageTitle));
    }

    public GirisIrsaliyesiListViewModel(
        IGirisIrsaliyesiService irsaliyeService,
        ICariHesapService cariHesapService,
        IUrunService urunService,
        IKapTipiService kapTipiService)
    {
        _irsaliyeService = irsaliyeService;
        _cariHesapService = cariHesapService;
        _urunService = urunService;
        _kapTipiService = kapTipiService;
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            StatusMessage = "Yükleniyor...";
            var baslangic = FiltreBaslangic?.DateTime ?? DateTime.Today.AddDays(-30);
            var bitis = FiltreBitis?.DateTime ?? DateTime.Today;
            
            var irsaliyeler = await _irsaliyeService.GetByDateRangeAsync(baslangic, bitis);
            
            // Sadece taslaklar filtresi
            if (SadeceTaslaklar)
            {
                irsaliyeler = irsaliyeler.Where(i => i.Durum == BelgeDurumu.Taslak).ToList();
            }
            
            Irsaliyeler = new ObservableCollection<GirisIrsaliyesi>(irsaliyeler);
            
            var durumText = SadeceTaslaklar ? "taslak irsaliye" : "irsaliye";
            StatusMessage = $"{Irsaliyeler.Count} {durumText} listelendi.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task FilterAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task NewIrsaliyeAsync()
    {
        var vm = new GirisIrsaliyesiEditViewModel(
            _irsaliyeService,
            _cariHesapService,
            _urunService,
            _kapTipiService,
            async (saved) =>
            {
                if (saved)
                {
                    await LoadDataAsync();
                }
            }
        );

        var window = new GirisIrsaliyesiWindow
        {
            DataContext = vm
        };

        if (App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            await window.ShowDialog(desktop.MainWindow!);
        }
    }

    [RelayCommand]
    private async Task EditIrsaliyeAsync()
    {
        if (SelectedIrsaliye == null) return;

        // İrsaliyeyi kalemlerle birlikte yükle
        var irsaliye = await _irsaliyeService.GetByIdWithKalemlerAsync(SelectedIrsaliye.Id);
        if (irsaliye == null) return;

        var vm = new GirisIrsaliyesiEditViewModel(
            _irsaliyeService,
            _cariHesapService,
            _urunService,
            _kapTipiService,
            async (saved) =>
            {
                if (saved)
                {
                    await LoadDataAsync();
                }
            },
            irsaliye
        );

        var window = new GirisIrsaliyesiWindow
        {
            DataContext = vm
        };

        if (App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            await window.ShowDialog(desktop.MainWindow!);
        }
    }

    [RelayCommand]
    private async Task OnaylaAsync()
    {
        if (SelectedIrsaliye == null) return;

        try
        {
            await _irsaliyeService.OnaylaAsync(SelectedIrsaliye.Id);
            StatusMessage = $"{SelectedIrsaliye.IrsaliyeNo} onaylandı.";
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Onay hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedIrsaliye == null) return;

        try
        {
            await _irsaliyeService.DeleteAsync(SelectedIrsaliye.Id);
            StatusMessage = $"{SelectedIrsaliye.IrsaliyeNo} silindi.";
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Silme hatası: {ex.Message}";
        }
    }
}
