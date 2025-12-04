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
/// Kasa Takip - Kasa rehin alma ve iade işlemleri
/// </summary>
public partial class KasaTakipViewModel : ViewModelBase
{
    private readonly IKasaTakipService _kasaTakipService;
    private readonly ICariHesapService _cariHesapService;
    private readonly IKapTipiService _kapTipiService;

    [ObservableProperty]
    private ObservableCollection<KasaHareket> _hareketler = new();

    [ObservableProperty]
    private ObservableCollection<RehinFisi> _rehinFisleri = new();

    [ObservableProperty]
    private ObservableCollection<CariHesap> _cariler = new();

    [ObservableProperty]
    private ObservableCollection<KapTipi> _kapTipleri = new();

    [ObservableProperty]
    private KasaHareket? _selectedHareket;

    [ObservableProperty]
    private RehinFisi? _selectedRehinFisi;

    // Yeni rehin fişi için
    [ObservableProperty]
    private bool _rehinDialogAcik;

    [ObservableProperty]
    private CariHesap? _selectedCari;

    [ObservableProperty]
    private KapTipi? _selectedKapTipi;

    [ObservableProperty]
    private int _kasaAdet = 1;

    [ObservableProperty]
    private bool _islemTipiAl = true; // true: Al, false: İade

    [ObservableProperty]
    private string _statusMessage = "Hazır";

    [ObservableProperty]
    private DateTimeOffset? _filtreBaslangic = DateTimeOffset.Now.AddDays(-30);

    [ObservableProperty]
    private DateTimeOffset? _filtreBitis = DateTimeOffset.Now;

    // Özet bilgiler
    public decimal ToplamRehinAlacak => RehinFisleri.Where(r => r.IslemTipiAl).Sum(r => r.ToplamTutar);
    public decimal ToplamRehinVerecek => RehinFisleri.Where(r => !r.IslemTipiAl).Sum(r => r.ToplamTutar);
    public decimal NetRehin => ToplamRehinAlacak - ToplamRehinVerecek;

    public KasaTakipViewModel(
        IKasaTakipService kasaTakipService,
        ICariHesapService cariHesapService,
        IKapTipiService kapTipiService)
    {
        _kasaTakipService = kasaTakipService;
        _cariHesapService = cariHesapService;
        _kapTipiService = kapTipiService;

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            StatusMessage = "Veriler yükleniyor...";

            var cariler = await _cariHesapService.GetAllAsync();
            Cariler = new ObservableCollection<CariHesap>(cariler);

            var kapTipleri = await _kapTipiService.GetActiveAsync();
            KapTipleri = new ObservableCollection<KapTipi>(kapTipleri);

            await YenileAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Yükleme hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task YenileAsync()
    {
        try
        {
            StatusMessage = "Rehin fişleri yükleniyor...";

            var baslangic = FiltreBaslangic?.DateTime ?? DateTime.Today.AddDays(-30);
            var bitis = FiltreBitis?.DateTime ?? DateTime.Today;

            var fisler = await _kasaTakipService.GetRehinFisleriAsync(baslangic, bitis);
            RehinFisleri = new ObservableCollection<RehinFisi>(fisler);

            OnPropertyChanged(nameof(ToplamRehinAlacak));
            OnPropertyChanged(nameof(ToplamRehinVerecek));
            OnPropertyChanged(nameof(NetRehin));

            StatusMessage = $"✅ {RehinFisleri.Count} rehin fişi listelendi";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Yenileme hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private void YeniRehinAl()
    {
        IslemTipiAl = true;
        KasaAdet = 1;
        SelectedCari = null;
        SelectedKapTipi = null;
        RehinDialogAcik = true;
        StatusMessage = "📥 Yeni rehin alınıyor...";
    }

    [RelayCommand]
    private void YeniRehinIade()
    {
        IslemTipiAl = false;
        KasaAdet = 1;
        SelectedCari = null;
        SelectedKapTipi = null;
        RehinDialogAcik = true;
        StatusMessage = "📤 Rehin iadesi yapılıyor...";
    }

    [RelayCommand]
    private async Task RehinKaydetAsync()
    {
        try
        {
            if (SelectedCari == null)
            {
                StatusMessage = "⚠️ Lütfen cari seçin!";
                return;
            }

            if (SelectedKapTipi == null)
            {
                StatusMessage = "⚠️ Lütfen kap tipi seçin!";
                return;
            }

            if (KasaAdet <= 0)
            {
                StatusMessage = "⚠️ Kasa adedi en az 1 olmalı!";
                return;
            }

            StatusMessage = "Rehin kaydediliyor...";

            RehinFisi fis;
            if (IslemTipiAl)
            {
                fis = await _kasaTakipService.RehinAlAsync(SelectedCari.Id, SelectedKapTipi.Id, KasaAdet);
                StatusMessage = $"✅ Rehin alındı: {fis.FisNo} - {fis.ToplamTutar:N2} ₺";
            }
            else
            {
                fis = await _kasaTakipService.RehinIadeEtAsync(SelectedCari.Id, SelectedKapTipi.Id, KasaAdet);
                StatusMessage = $"✅ Rehin iade edildi: {fis.FisNo} - {fis.ToplamTutar:N2} ₺";
            }

            RehinDialogAcik = false;
            await YenileAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Kayıt hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private void RehinIptal()
    {
        RehinDialogAcik = false;
        StatusMessage = "İşlem iptal edildi.";
    }
}
