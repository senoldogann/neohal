using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeoHal.Core.Entities;
using NeoHal.Services.Interfaces;

namespace NeoHal.Desktop.ViewModels;

/// <summary>
/// Stok Durumu görüntüleme - Ürün bazlı FIFO takibi
/// </summary>
public partial class StokDurumuViewModel : ViewModelBase
{
    private readonly IStokService _stokService;
    private readonly IUrunService _urunService;
    private readonly IKapTipiService _kapTipiService;

    [ObservableProperty]
    private ObservableCollection<StokDurumuItem> _stoklar = new();

    [ObservableProperty]
    private ObservableCollection<StokDetayItem> _seciliStokDetaylari = new();

    [ObservableProperty]
    private StokDurumuItem? _selectedStok;

    [ObservableProperty]
    private ObservableCollection<Urun> _urunler = new();

    [ObservableProperty]
    private Urun? _filtreUrun;

    [ObservableProperty]
    private bool _sadeceMevcutGoster = true;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // Özet bilgiler
    public int ToplamUrunCesidi => Stoklar.Count;
    public decimal ToplamStokKg => Stoklar.Sum(s => s.ToplamKalanKg);
    public int ToplamKapAdet => Stoklar.Sum(s => s.ToplamKalanKap);

    public StokDurumuViewModel(
        IStokService stokService,
        IUrunService urunService,
        IKapTipiService kapTipiService)
    {
        _stokService = stokService;
        _urunService = urunService;
        _kapTipiService = kapTipiService;
        
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            StatusMessage = "Yükleniyor...";
            
            // Ürünleri yükle
            var urunler = await _urunService.GetActiveAsync();
            Urunler = new ObservableCollection<Urun>(urunler);
            
            // Stok durumunu yükle
            await YenileAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Hata: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task YenileAsync()
    {
        try
        {
            StatusMessage = "Stok durumu hesaplanıyor...";
            
            var stokDurumu = await _stokService.GetStokDurumuAsync(FiltreUrun?.Id, SadeceMevcutGoster);
            Stoklar = new ObservableCollection<StokDurumuItem>(stokDurumu);
            
            OnPropertyChanged(nameof(ToplamUrunCesidi));
            OnPropertyChanged(nameof(ToplamStokKg));
            OnPropertyChanged(nameof(ToplamKapAdet));
            
            StatusMessage = $"✅ {Stoklar.Count} ürün listelendi. Toplam: {ToplamStokKg:N2} kg";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Hata: {ex.Message}";
        }
    }

    partial void OnSelectedStokChanged(StokDurumuItem? value)
    {
        if (value != null)
        {
            _ = LoadStokDetayAsync(value);
        }
        else
        {
            SeciliStokDetaylari.Clear();
        }
    }

    private async Task LoadStokDetayAsync(StokDurumuItem stok)
    {
        try
        {
            var detaylar = await _stokService.GetStokDetayAsync(stok.UrunId);
            SeciliStokDetaylari = new ObservableCollection<StokDetayItem>(detaylar);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Detay yüklenemedi: {ex.Message}";
        }
    }

    partial void OnFiltreUrunChanged(Urun? value)
    {
        _ = YenileAsync();
    }

    partial void OnSadeceMevcutGosterChanged(bool value)
    {
        _ = YenileAsync();
    }

    [RelayCommand]
    private void FiltreTemizle()
    {
        FiltreUrun = null;
        SadeceMevcutGoster = true;
    }
}
