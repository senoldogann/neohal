using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeoHal.Core.Entities;
using NeoHal.Services.Interfaces;

namespace NeoHal.Desktop.ViewModels;

public partial class KapTipiViewModel : ViewModelBase
{
    private readonly IKapTipiService _kapTipiService;

    [ObservableProperty]
    private ObservableCollection<KapTipi> _kapTipleri = new();

    [ObservableProperty]
    private KapTipi? _selectedKapTipi;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isNew;

    // Edit form alanları
    [ObservableProperty]
    private string _editKod = string.Empty;

    [ObservableProperty]
    private string _editAd = string.Empty;

    [ObservableProperty]
    private decimal _editDaraAgirlik;

    [ObservableProperty]
    private decimal _editRehinBedeli;

    [ObservableProperty]
    private bool _editAktif = true;

    [ObservableProperty]
    private bool _sadecekAktifler = true;

    public KapTipiViewModel(IKapTipiService kapTipiService)
    {
        _kapTipiService = kapTipiService;
        _ = LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        try
        {
            var kapTipleri = SadecekAktifler 
                ? await _kapTipiService.GetActiveAsync()
                : await _kapTipiService.GetAllAsync();
            
            KapTipleri = new ObservableCollection<KapTipi>(kapTipleri);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Hata: {ex.Message}");
        }
    }

    partial void OnSadecekAktiflerChanged(bool value)
    {
        _ = LoadDataAsync();
    }

    partial void OnSelectedKapTipiChanged(KapTipi? value)
    {
        if (value != null && !IsNew)
        {
            EditKod = value.Kod;
            EditAd = value.Ad;
            EditDaraAgirlik = value.DaraAgirlik;
            EditRehinBedeli = value.RehinBedeli;
            EditAktif = value.Aktif;
            IsEditing = true;
        }
    }

    [RelayCommand]
    private void New()
    {
        SelectedKapTipi = null;
        EditKod = string.Empty;
        EditAd = string.Empty;
        EditDaraAgirlik = 0;
        EditRehinBedeli = 0;
        EditAktif = true;
        IsNew = true;
        IsEditing = true;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(EditKod) || string.IsNullOrWhiteSpace(EditAd))
            {
                return;
            }

            if (IsNew)
            {
                var newKapTipi = new KapTipi
                {
                    Kod = EditKod,
                    Ad = EditAd,
                    DaraAgirlik = EditDaraAgirlik,
                    RehinBedeli = EditRehinBedeli,
                    Aktif = EditAktif
                };
                await _kapTipiService.CreateAsync(newKapTipi);
            }
            else if (SelectedKapTipi != null)
            {
                SelectedKapTipi.Kod = EditKod;
                SelectedKapTipi.Ad = EditAd;
                SelectedKapTipi.DaraAgirlik = EditDaraAgirlik;
                SelectedKapTipi.RehinBedeli = EditRehinBedeli;
                SelectedKapTipi.Aktif = EditAktif;
                await _kapTipiService.UpdateAsync(SelectedKapTipi);
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
        SelectedKapTipi = null;
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedKapTipi == null) return;

        try
        {
            await _kapTipiService.DeleteAsync(SelectedKapTipi.Id);
            IsEditing = false;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Silme hatası: {ex.Message}");
        }
    }
}
