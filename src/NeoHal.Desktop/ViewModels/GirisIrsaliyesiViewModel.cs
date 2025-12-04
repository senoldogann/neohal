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

public partial class GirisIrsaliyesiViewModel : ViewModelBase
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
    private bool _isEditing;

    [ObservableProperty]
    private bool _isNew;

    // Müstahsil listesi
    [ObservableProperty]
    private ObservableCollection<CariHesap> _mustahsiller = new();

    // Ürün listesi
    [ObservableProperty]
    private ObservableCollection<Urun> _urunler = new();

    // Kap tipi listesi
    [ObservableProperty]
    private ObservableCollection<KapTipi> _kapTipleri = new();

    // Edit form alanları
    [ObservableProperty]
    private string _editIrsaliyeNo = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _editTarih = DateTimeOffset.Now;

    [ObservableProperty]
    private CariHesap? _editMustahsil;

    [ObservableProperty]
    private string _editAciklama = string.Empty;

    // Kalem listesi
    [ObservableProperty]
    private ObservableCollection<GirisIrsaliyesiKalem> _editKalemler = new();

    // Yeni kalem ekleme alanları
    [ObservableProperty]
    private Urun? _yeniKalemUrun;

    [ObservableProperty]
    private KapTipi? _yeniKalemKapTipi;

    [ObservableProperty]
    private int _yeniKalemKapAdet = 1;

    [ObservableProperty]
    private decimal _yeniKalemBrutKg;

    [ObservableProperty]
    private decimal _yeniKalemDaraKg;

    [ObservableProperty]
    private decimal _yeniKalemBirimFiyat;

    // Filtre
    [ObservableProperty]
    private DateTimeOffset? _filtreBaslangic = DateTimeOffset.Now.AddDays(-30);

    [ObservableProperty]
    private DateTimeOffset? _filtreBitis = DateTimeOffset.Now;

    // Hesaplanan değerler
    public decimal YeniKalemNetKg => YeniKalemBrutKg - YeniKalemDaraKg;
    public decimal EditToplamBrut => EditKalemler.Sum(k => k.BrutKg);
    public decimal EditToplamDara => EditKalemler.Sum(k => k.DaraKg);
    public decimal EditToplamNet => EditKalemler.Sum(k => k.NetKg);
    public int EditToplamKapAdet => EditKalemler.Sum(k => k.KapAdet);

    public GirisIrsaliyesiViewModel(
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

    public async Task LoadDataAsync()
    {
        try
        {
            var baslangic = FiltreBaslangic?.DateTime ?? DateTime.Today.AddDays(-30);
            var bitis = FiltreBitis?.DateTime ?? DateTime.Today;
            var irsaliyeler = await _irsaliyeService.GetByDateRangeAsync(baslangic, bitis);
            Irsaliyeler = new ObservableCollection<GirisIrsaliyesi>(irsaliyeler);

            var mustahsiller = await _cariHesapService.GetByCariTipiAsync(CariTipi.Mustahsil);
            Mustahsiller = new ObservableCollection<CariHesap>(mustahsiller);

            var urunler = await _urunService.GetActiveAsync();
            Urunler = new ObservableCollection<Urun>(urunler);

            var kapTipleri = await _kapTipiService.GetActiveAsync();
            KapTipleri = new ObservableCollection<KapTipi>(kapTipleri);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Hata: {ex.Message}");
        }
    }

    partial void OnYeniKalemKapTipiChanged(KapTipi? value)
    {
        if (value != null)
        {
            YeniKalemDaraKg = value.DaraAgirlik * YeniKalemKapAdet;
        }
        OnPropertyChanged(nameof(YeniKalemNetKg));
    }

    partial void OnYeniKalemKapAdetChanged(int value)
    {
        if (YeniKalemKapTipi != null)
        {
            YeniKalemDaraKg = YeniKalemKapTipi.DaraAgirlik * value;
        }
        OnPropertyChanged(nameof(YeniKalemNetKg));
    }

    partial void OnYeniKalemBrutKgChanged(decimal value)
    {
        OnPropertyChanged(nameof(YeniKalemNetKg));
    }

    partial void OnYeniKalemDaraKgChanged(decimal value)
    {
        OnPropertyChanged(nameof(YeniKalemNetKg));
    }

    partial void OnSelectedIrsaliyeChanged(GirisIrsaliyesi? value)
    {
        if (value != null && !IsNew)
        {
            EditIrsaliyeNo = value.IrsaliyeNo;
            EditTarih = new DateTimeOffset(value.Tarih);
            EditMustahsil = Mustahsiller.FirstOrDefault(m => m.Id == value.MustahsilId);
            EditAciklama = value.Aciklama ?? string.Empty;
            EditKalemler = new ObservableCollection<GirisIrsaliyesiKalem>(value.Kalemler);
            IsEditing = true;
            RefreshTotals();
        }
    }

    private void RefreshTotals()
    {
        OnPropertyChanged(nameof(EditToplamBrut));
        OnPropertyChanged(nameof(EditToplamDara));
        OnPropertyChanged(nameof(EditToplamNet));
        OnPropertyChanged(nameof(EditToplamKapAdet));
    }

    [RelayCommand]
    private async Task FilterAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task NewAsync()
    {
        SelectedIrsaliye = null;
        EditIrsaliyeNo = await _irsaliyeService.GenerateNewIrsaliyeNoAsync();
        EditTarih = DateTimeOffset.Now;
        EditMustahsil = null;
        EditAciklama = string.Empty;
        EditKalemler = new ObservableCollection<GirisIrsaliyesiKalem>();
        ClearYeniKalem();
        IsNew = true;
        IsEditing = true;
        RefreshTotals();
    }

    private void ClearYeniKalem()
    {
        YeniKalemUrun = null;
        YeniKalemKapTipi = null;
        YeniKalemKapAdet = 1;
        YeniKalemBrutKg = 0;
        YeniKalemDaraKg = 0;
        YeniKalemBirimFiyat = 0;
    }

    [RelayCommand]
    private void AddKalem()
    {
        if (YeniKalemUrun == null || YeniKalemKapTipi == null) return;

        var netKg = YeniKalemBrutKg - YeniKalemDaraKg;
        var kalem = new GirisIrsaliyesiKalem
        {
            Id = Guid.NewGuid(),
            UrunId = YeniKalemUrun.Id,
            Urun = YeniKalemUrun,
            KapTipiId = YeniKalemKapTipi.Id,
            KapTipi = YeniKalemKapTipi,
            KapAdet = YeniKalemKapAdet,
            BrutKg = YeniKalemBrutKg,
            DaraKg = YeniKalemDaraKg,
            NetKg = netKg,
            BirimFiyat = YeniKalemBirimFiyat,
            KalanKapAdet = YeniKalemKapAdet,
            KalanKg = netKg
        };

        EditKalemler.Add(kalem);
        ClearYeniKalem();
        RefreshTotals();
    }

    [RelayCommand]
    private void RemoveKalem(GirisIrsaliyesiKalem kalem)
    {
        EditKalemler.Remove(kalem);
        RefreshTotals();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            if (EditMustahsil == null || !EditKalemler.Any())
            {
                return;
            }

            if (IsNew)
            {
                var irsaliye = new GirisIrsaliyesi
                {
                    IrsaliyeNo = EditIrsaliyeNo,
                    Tarih = EditTarih?.DateTime ?? DateTime.Today,
                    MustahsilId = EditMustahsil.Id,
                    Aciklama = EditAciklama,
                    ToplamBrut = EditToplamBrut,
                    ToplamDara = EditToplamDara,
                    ToplamNet = EditToplamNet,
                    ToplamKapAdet = EditToplamKapAdet,
                    Kalemler = EditKalemler.ToList()
                };
                await _irsaliyeService.CreateAsync(irsaliye);
            }
            else if (SelectedIrsaliye != null)
            {
                SelectedIrsaliye.Tarih = EditTarih?.DateTime ?? DateTime.Today;
                SelectedIrsaliye.MustahsilId = EditMustahsil.Id;
                SelectedIrsaliye.Aciklama = EditAciklama;
                await _irsaliyeService.UpdateAsync(SelectedIrsaliye);
            }

            IsNew = false;
            IsEditing = false;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Kayıt hatası: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        IsNew = false;
        IsEditing = false;
        SelectedIrsaliye = null;
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedIrsaliye == null) return;

        try
        {
            await _irsaliyeService.DeleteAsync(SelectedIrsaliye.Id);
            IsEditing = false;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Silme hatası: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task OnaylaAsync()
    {
        if (SelectedIrsaliye == null) return;

        try
        {
            await _irsaliyeService.OnaylaAsync(SelectedIrsaliye.Id);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Onay hatası: {ex.Message}");
        }
    }
}
