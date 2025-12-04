using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeoHal.Core.Entities;
using NeoHal.Core.Enums;
using NeoHal.Services.Interfaces;

namespace NeoHal.Desktop.ViewModels;

public partial class KasaHesabiViewModel : ViewModelBase
{
    private readonly IKasaHesabiService _kasaService;
    private readonly ICariHesapService _cariService;

    [ObservableProperty]
    private ObservableCollection<KasaHesabi> _hareketler = new();

    [ObservableProperty]
    private KasaHesabi? _selectedHareket;

    [ObservableProperty]
    private ObservableCollection<CariHesap> _cariler = new();

    [ObservableProperty]
    private CariHesap? _selectedCari;

    [ObservableProperty]
    private decimal _kasaBakiye;

    [ObservableProperty]
    private DateTimeOffset? _filtreBaslangic = DateTimeOffset.Now.AddDays(-30);

    [ObservableProperty]
    private DateTimeOffset? _filtreBitis = DateTimeOffset.Now;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // Yeni hareket için
    [ObservableProperty]
    private bool _isEditing = false;

    [ObservableProperty]
    private DateTimeOffset? _yeniTarih = DateTimeOffset.Now;

    [ObservableProperty]
    private bool _yeniGirisHareketi = true;

    [ObservableProperty]
    private decimal _yeniTutar;

    [ObservableProperty]
    private OdemeTuru _yeniOdemeTuru = OdemeTuru.Nakit;

    [ObservableProperty]
    private CariHesap? _yeniCari;

    [ObservableProperty]
    private string _yeniAciklama = string.Empty;

    public OdemeTuru[] OdemeTurleri { get; } = Enum.GetValues<OdemeTuru>();

    public KasaHesabiViewModel(IKasaHesabiService kasaService, ICariHesapService cariService)
    {
        _kasaService = kasaService;
        _cariService = cariService;
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            StatusMessage = "Yükleniyor...";
            
            // Carileri yükle
            var cariler = await _cariService.GetAllAsync();
            Cariler = new ObservableCollection<CariHesap>(cariler);
            
            // Hareketleri yükle
            var baslangic = FiltreBaslangic?.DateTime ?? DateTime.Today.AddDays(-30);
            var bitis = FiltreBitis?.DateTime.AddDays(1) ?? DateTime.Today.AddDays(1);
            
            var hareketler = await _kasaService.GetByDateRangeAsync(baslangic, bitis);
            Hareketler = new ObservableCollection<KasaHesabi>(hareketler);
            
            // Bakiyeyi hesapla
            KasaBakiye = await _kasaService.GetBakiyeAsync();
            
            StatusMessage = $"{Hareketler.Count} hareket listelendi. Bakiye: ₺{KasaBakiye:N2}";
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
    private void NewHareket()
    {
        IsEditing = true;
        YeniTarih = DateTimeOffset.Now;
        YeniGirisHareketi = true;
        YeniTutar = 0;
        YeniOdemeTuru = OdemeTuru.Nakit;
        YeniCari = null;
        YeniAciklama = string.Empty;
    }

    [RelayCommand]
    private async Task SaveHareketAsync()
    {
        try
        {
            if (YeniTutar <= 0)
            {
                StatusMessage = "Tutar 0'dan büyük olmalıdır!";
                return;
            }

            var hareket = new KasaHesabi
            {
                Tarih = YeniTarih?.DateTime ?? DateTime.Now,
                GirisHareketi = YeniGirisHareketi,
                Tutar = YeniTutar,
                OdemeTuru = YeniOdemeTuru,
                CariId = YeniCari?.Id,
                Aciklama = YeniAciklama
            };

            await _kasaService.CreateAsync(hareket);
            
            StatusMessage = YeniGirisHareketi 
                ? $"₺{YeniTutar:N2} kasa girişi kaydedildi." 
                : $"₺{YeniTutar:N2} kasa çıkışı kaydedildi.";
            
            IsEditing = false;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kayıt hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
    }

    [RelayCommand]
    private async Task DeleteHareketAsync()
    {
        if (SelectedHareket == null) return;

        try
        {
            await _kasaService.DeleteAsync(SelectedHareket.Id);
            StatusMessage = "Hareket silindi.";
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Silme hatası: {ex.Message}";
        }
    }
}
