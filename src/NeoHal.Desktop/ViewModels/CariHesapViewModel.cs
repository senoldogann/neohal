using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeoHal.Core.Entities;
using NeoHal.Core.Enums;
using NeoHal.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace NeoHal.Desktop.ViewModels;

public partial class CariHesapViewModel : ViewModelBase
{
    private readonly ICariHesapService _cariHesapService;

    [ObservableProperty]
    private ObservableCollection<CariHesap> _cariHesaplar = new();

    [ObservableProperty]
    private CariHesap? _selectedCariHesap;

    [ObservableProperty]
    private CariHesap _editingCari = new();

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isNewRecord;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private CariTipi? _filterCariTipi;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CariTipiItem> _cariTipleri = new();

    public CariHesapViewModel(ICariHesapService cariHesapService)
    {
        _cariHesapService = cariHesapService;
        InitializeCariTipleri();
        _ = LoadDataAsync();
    }

    public CariHesapViewModel() : this(null!) { }

    private void InitializeCariTipleri()
    {
        CariTipleri = new ObservableCollection<CariTipiItem>
        {
            new(CariTipi.Mustahsil, "Müstahsil (Üretici)"),
            new(CariTipi.Alici, "Alıcı (Dükkan)"),
            new(CariTipi.Komisyoncu, "Komisyoncu"),
            new(CariTipi.Sevkiyatci, "Sevkiyatçı"),
            new(CariTipi.Nakliyeci, "Nakliyeci")
        };
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            StatusMessage = "Yükleniyor...";
            var cariler = await _cariHesapService.GetAllAsync();
            
            if (FilterCariTipi.HasValue)
                cariler = cariler.Where(c => c.CariTipi == FilterCariTipi.Value);
            
            if (!string.IsNullOrWhiteSpace(SearchText))
                cariler = cariler.Where(c => 
                    c.Unvan.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    c.Kod.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            
            CariHesaplar = new ObservableCollection<CariHesap>(cariler);
            StatusMessage = $"{CariHesaplar.Count} kayıt listelendi.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task NewCariAsync()
    {
        IsNewRecord = true;
        IsEditing = true;
        
        var yeniKod = await _cariHesapService.GenerateNewKodAsync(CariTipi.Mustahsil);
        EditingCari = new CariHesap
        {
            Kod = yeniKod,
            CariTipi = CariTipi.Mustahsil,
            Aktif = true
        };
        StatusMessage = "Yeni cari hesap oluşturuluyor...";
    }

    [RelayCommand]
    private void EditCari()
    {
        if (SelectedCariHesap == null) return;
        
        IsNewRecord = false;
        IsEditing = true;
        
        // Deep copy
        EditingCari = new CariHesap
        {
            Id = SelectedCariHesap.Id,
            Kod = SelectedCariHesap.Kod,
            Unvan = SelectedCariHesap.Unvan,
            CariTipi = SelectedCariHesap.CariTipi,
            TcKimlikNo = SelectedCariHesap.TcKimlikNo,
            VergiNo = SelectedCariHesap.VergiNo,
            VergiDairesi = SelectedCariHesap.VergiDairesi,
            Telefon = SelectedCariHesap.Telefon,
            Telefon2 = SelectedCariHesap.Telefon2,
            Email = SelectedCariHesap.Email,
            Adres = SelectedCariHesap.Adres,
            HksSicilNo = SelectedCariHesap.HksSicilNo,
            Aktif = SelectedCariHesap.Aktif
        };
        StatusMessage = "Düzenleniyor...";
    }

    [RelayCommand]
    private async Task SaveCariAsync()
    {
        try
        {
            // Ünvan kontrolü
            if (string.IsNullOrWhiteSpace(EditingCari.Unvan))
            {
                StatusMessage = "⚠️ Ünvan boş olamaz!";
                return;
            }

            // TC Kimlik No kontrolü (varsa)
            if (!string.IsNullOrEmpty(EditingCari.TcKimlikNo))
            {
                if (EditingCari.TcKimlikNo.Length != 11 || !EditingCari.TcKimlikNo.All(char.IsDigit))
                {
                    StatusMessage = "⚠️ TC Kimlik No 11 haneli sayı olmalıdır!";
                    return;
                }
            }

            // Vergi No kontrolü (varsa)
            if (!string.IsNullOrEmpty(EditingCari.VergiNo))
            {
                if (EditingCari.VergiNo.Length != 10 || !EditingCari.VergiNo.All(char.IsDigit))
                {
                    StatusMessage = "⚠️ Vergi No 10 haneli sayı olmalıdır!";
                    return;
                }
            }

            // Email kontrolü (varsa)
            if (!string.IsNullOrEmpty(EditingCari.Email))
            {
                if (!EditingCari.Email.Contains("@") || !EditingCari.Email.Contains("."))
                {
                    StatusMessage = "⚠️ Geçerli bir e-posta adresi giriniz!";
                    return;
                }
            }

            // Telefon kontrolü (varsa)
            if (!string.IsNullOrEmpty(EditingCari.Telefon))
            {
                var telefonRakam = new string(EditingCari.Telefon.Where(char.IsDigit).ToArray());
                if (telefonRakam.Length < 10)
                {
                    StatusMessage = "⚠️ Telefon numarası en az 10 haneli olmalıdır!";
                    return;
                }
            }

            if (IsNewRecord)
            {
                // Yeni kayıt için kod oluştur
                EditingCari.Kod = await _cariHesapService.GenerateNewKodAsync(EditingCari.CariTipi);
                await _cariHesapService.CreateAsync(EditingCari);
                StatusMessage = $"✅ {EditingCari.Unvan} başarıyla eklendi.";
            }
            else
            {
                await _cariHesapService.UpdateAsync(EditingCari);
                StatusMessage = $"✅ {EditingCari.Unvan} başarıyla güncellendi.";
            }

            IsEditing = false;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Kayıt hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        EditingCari = new CariHesap();
        StatusMessage = "İptal edildi.";
    }

    [RelayCommand]
    private async Task DeleteCariAsync()
    {
        if (SelectedCariHesap == null) return;

        try
        {
            await _cariHesapService.DeleteAsync(SelectedCariHesap.Id);
            StatusMessage = $"{SelectedCariHesap.Unvan} silindi.";
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Silme hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task FilterByTypeAsync(CariTipi? tip)
    {
        FilterCariTipi = tip;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadDataAsync();
    }

    partial void OnEditingCariChanged(CariHesap value)
    {
        if (value != null && IsNewRecord)
        {
            // Cari tipi değiştiğinde kodu güncelle
            _ = UpdateKodForCariTipiAsync();
        }
    }

    private async Task UpdateKodForCariTipiAsync()
    {
        if (_cariHesapService != null && IsNewRecord)
        {
            EditingCari.Kod = await _cariHesapService.GenerateNewKodAsync(EditingCari.CariTipi);
        }
    }
}

public record CariTipiItem(CariTipi Value, string DisplayName);
