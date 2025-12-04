using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeoHal.Core.Entities;
using NeoHal.Services.Interfaces;

namespace NeoHal.Desktop.ViewModels;

public partial class UrunGrubuViewModel : ViewModelBase
{
    private readonly IUrunGrubuService _urunGrubuService;

    [ObservableProperty]
    private ObservableCollection<UrunGrubu> _gruplar = new();

    [ObservableProperty]
    private UrunGrubu? _selectedGrup;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isNew;

    [ObservableProperty]
    private string _editKod = string.Empty;

    [ObservableProperty]
    private string _editAd = string.Empty;

    [ObservableProperty]
    private bool _editAktif = true;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public UrunGrubuViewModel(IUrunGrubuService urunGrubuService)
    {
        _urunGrubuService = urunGrubuService;
        _ = LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        try
        {
            var gruplar = await _urunGrubuService.GetAllAsync();
            Gruplar = new ObservableCollection<UrunGrubu>(gruplar);
            StatusMessage = $"{Gruplar.Count} grup listelendi.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
        }
    }

    partial void OnSelectedGrupChanged(UrunGrubu? value)
    {
        if (value != null && !IsNew)
        {
            EditKod = value.Kod;
            EditAd = value.Ad;
            EditAktif = value.Aktif;
            IsEditing = true;
        }
    }

    [RelayCommand]
    private void New()
    {
        SelectedGrup = null;
        EditKod = string.Empty;
        EditAd = string.Empty;
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
                StatusMessage = "Kod ve Ad zorunludur!";
                return;
            }

            if (IsNew)
            {
                var newGrup = new UrunGrubu
                {
                    Kod = EditKod,
                    Ad = EditAd,
                    Aktif = EditAktif
                };
                await _urunGrubuService.CreateAsync(newGrup);
                StatusMessage = "Grup eklendi.";
            }
            else if (SelectedGrup != null)
            {
                SelectedGrup.Kod = EditKod;
                SelectedGrup.Ad = EditAd;
                SelectedGrup.Aktif = EditAktif;
                await _urunGrubuService.UpdateAsync(SelectedGrup);
                StatusMessage = "Grup güncellendi.";
            }

            IsNew = false;
            IsEditing = false;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kayıt hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        IsNew = false;
        IsEditing = false;
        SelectedGrup = null;
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedGrup == null) return;

        try
        {
            await _urunGrubuService.DeleteAsync(SelectedGrup.Id);
            StatusMessage = "Grup silindi.";
            IsEditing = false;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Silme hatası: {ex.Message}";
        }
    }
}
