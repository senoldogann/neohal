using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeoHal.Core.Entities;
using NeoHal.Core.Enums;
using NeoHal.Desktop.Views;
using NeoHal.Services.Interfaces;

namespace NeoHal.Desktop.ViewModels;

public partial class SatisFaturasiListViewModel : ViewModelBase
{
    private readonly ISatisFaturasiService _faturaService;
    private readonly ICariHesapService _cariHesapService;
    private readonly IGirisIrsaliyesiService _irsaliyeService;
    private readonly IUrunService _urunService;
    private readonly IKapTipiService _kapTipiService;

    [ObservableProperty]
    private ObservableCollection<SatisFaturasi> _faturalar = new();

    [ObservableProperty]
    private SatisFaturasi? _selectedFatura;

    [ObservableProperty]
    private DateTimeOffset? _filtreBaslangic = DateTimeOffset.Now.AddDays(-30);

    [ObservableProperty]
    private DateTimeOffset? _filtreBitis = DateTimeOffset.Now;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public SatisFaturasiListViewModel(
        ISatisFaturasiService faturaService,
        ICariHesapService cariHesapService,
        IGirisIrsaliyesiService irsaliyeService,
        IUrunService urunService,
        IKapTipiService kapTipiService)
    {
        _faturaService = faturaService;
        _cariHesapService = cariHesapService;
        _irsaliyeService = irsaliyeService;
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
            
            var faturalar = await _faturaService.GetByDateRangeAsync(baslangic, bitis);
            Faturalar = new ObservableCollection<SatisFaturasi>(faturalar);
            StatusMessage = $"{Faturalar.Count} fatura listelendi.";
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
    private async Task NewFaturaAsync()
    {
        var vm = new SatisFaturasiEditViewModel(
            _faturaService,
            _cariHesapService,
            _irsaliyeService,
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

        var window = new SatisFaturasiWindow
        {
            DataContext = vm
        };

        if (App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            await window.ShowDialog(desktop.MainWindow!);
        }
    }

    [RelayCommand]
    private async Task EditFaturaAsync()
    {
        if (SelectedFatura == null) return;

        var fatura = await _faturaService.GetByIdWithKalemlerAsync(SelectedFatura.Id);
        if (fatura == null) return;

        var vm = new SatisFaturasiEditViewModel(
            _faturaService,
            _cariHesapService,
            _irsaliyeService,
            _urunService,
            _kapTipiService,
            async (saved) =>
            {
                if (saved)
                {
                    await LoadDataAsync();
                }
            },
            fatura
        );

        var window = new SatisFaturasiWindow
        {
            DataContext = vm
        };

        if (App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            await window.ShowDialog(desktop.MainWindow!);
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedFatura == null) return;

        try
        {
            await _faturaService.DeleteAsync(SelectedFatura.Id);
            StatusMessage = $"{SelectedFatura.FaturaNo} silindi.";
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Silme hatası: {ex.Message}";
        }
    }
}
