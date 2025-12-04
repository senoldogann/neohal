using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeoHal.Core.Entities;
using NeoHal.Core.Enums;
using NeoHal.Services.Interfaces;

namespace NeoHal.Desktop.ViewModels;

public partial class UrunViewModel : ViewModelBase
{
    private readonly IUrunService _urunService;
    private readonly IUrunGrubuService _urunGrubuService;

    [ObservableProperty]
    private ObservableCollection<Urun> _urunler = new();

    [ObservableProperty]
    private ObservableCollection<UrunGrubu> _gruplar = new();

    [ObservableProperty]
    private Urun? _selectedUrun;

    [ObservableProperty]
    private UrunGrubu? _selectedGrup;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // Edit form fields
    [ObservableProperty]
    private string _editKod = string.Empty;

    [ObservableProperty]
    private string _editAd = string.Empty;

    [ObservableProperty]
    private BirimTuru _editBirim = BirimTuru.Kilogram;

    [ObservableProperty]
    private decimal _editKdvOrani = 1;

    [ObservableProperty]
    private decimal _editRusumOrani = 8;

    [ObservableProperty]
    private decimal _editStopajOrani = 4;

    [ObservableProperty]
    private UrunGrubu? _editGrup;

    [ObservableProperty]
    private string? _editHksUrunKodu;

    // Birim seçenekleri
    public BirimTuru[] BirimSecenekleri { get; } = Enum.GetValues<BirimTuru>();

    public UrunViewModel(IUrunService urunService, IUrunGrubuService urunGrubuService)
    {
        _urunService = urunService;
        _urunGrubuService = urunGrubuService;
        _ = LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var urunler = await _urunService.GetAllAsync();
            Urunler = new ObservableCollection<Urun>(urunler);

            var gruplar = await _urunGrubuService.GetAllAsync();
            Gruplar = new ObservableCollection<UrunGrubu>(gruplar);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        IsLoading = true;
        try
        {
            var results = await _urunService.SearchAsync(SearchText);
            if (SelectedGrup != null)
            {
                results = results.Where(u => u.GrupId == SelectedGrup.Id);
            }
            Urunler = new ObservableCollection<Urun>(results);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task FilterByGrupAsync()
    {
        IsLoading = true;
        try
        {
            IEnumerable<Urun> results;
            if (SelectedGrup != null)
            {
                results = await _urunService.GetByGrupIdAsync(SelectedGrup.Id);
            }
            else
            {
                results = await _urunService.GetAllAsync();
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var term = SearchText.ToLower();
                results = results.Where(u => u.Ad.ToLower().Contains(term) || 
                                              u.Kod.ToLower().Contains(term));
            }

            Urunler = new ObservableCollection<Urun>(results);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NewUrunAsync()
    {
        EditKod = await _urunService.GenerateNewKodAsync();
        EditAd = string.Empty;
        EditBirim = BirimTuru.Kilogram;
        EditKdvOrani = 1;
        EditRusumOrani = 8;
        EditStopajOrani = 4;
        EditGrup = Gruplar.FirstOrDefault();
        EditHksUrunKodu = null;
        SelectedUrun = null;
        IsEditMode = true;
    }

    [RelayCommand]
    private void EditUrun()
    {
        if (SelectedUrun == null) return;

        EditKod = SelectedUrun.Kod;
        EditAd = SelectedUrun.Ad;
        EditBirim = SelectedUrun.Birim;
        EditKdvOrani = SelectedUrun.KdvOrani;
        EditRusumOrani = SelectedUrun.RusumOrani;
        EditStopajOrani = SelectedUrun.StopajOrani;
        EditGrup = Gruplar.FirstOrDefault(g => g.Id == SelectedUrun.GrupId);
        EditHksUrunKodu = SelectedUrun.HksUrunKodu;
        IsEditMode = true;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        // Ürün adı kontrolü
        if (string.IsNullOrWhiteSpace(EditAd))
        {
            StatusMessage = "⚠️ Ürün adı boş olamaz!";
            return;
        }

        // Grup kontrolü
        if (EditGrup == null)
        {
            StatusMessage = "⚠️ Ürün grubu seçilmelidir!";
            return;
        }

        // Oran kontrolleri
        if (EditKdvOrani < 0 || EditKdvOrani > 50)
        {
            StatusMessage = "⚠️ KDV oranı 0-50 arasında olmalıdır!";
            return;
        }

        if (EditRusumOrani < 0 || EditRusumOrani > 20)
        {
            StatusMessage = "⚠️ Rüsum oranı 0-20 arasında olmalıdır!";
            return;
        }

        if (EditStopajOrani < 0 || EditStopajOrani > 20)
        {
            StatusMessage = "⚠️ Stopaj oranı 0-20 arasında olmalıdır!";
            return;
        }

        IsLoading = true;
        try
        {
            if (SelectedUrun == null)
            {
                // Yeni ürün
                var yeniUrun = new Urun
                {
                    Kod = EditKod,
                    Ad = EditAd,
                    Birim = EditBirim,
                    KdvOrani = EditKdvOrani,
                    RusumOrani = EditRusumOrani,
                    StopajOrani = EditStopajOrani,
                    GrupId = EditGrup.Id,
                    HksUrunKodu = EditHksUrunKodu
                };
                await _urunService.CreateAsync(yeniUrun);
                StatusMessage = $"✅ {EditAd} başarıyla eklendi.";
            }
            else
            {
                // Güncelleme
                SelectedUrun.Kod = EditKod;
                SelectedUrun.Ad = EditAd;
                SelectedUrun.Birim = EditBirim;
                SelectedUrun.KdvOrani = EditKdvOrani;
                SelectedUrun.RusumOrani = EditRusumOrani;
                SelectedUrun.StopajOrani = EditStopajOrani;
                SelectedUrun.GrupId = EditGrup.Id;
                SelectedUrun.HksUrunKodu = EditHksUrunKodu;
                await _urunService.UpdateAsync(SelectedUrun);
                StatusMessage = $"✅ {EditAd} başarıyla güncellendi.";
            }

            await LoadDataAsync();
            IsEditMode = false;
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Kayıt hatası: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        IsEditMode = false;
        SelectedUrun = null;
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedUrun == null) return;

        IsLoading = true;
        try
        {
            await _urunService.DeleteAsync(SelectedUrun.Id);
            await LoadDataAsync();
            IsEditMode = false;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
