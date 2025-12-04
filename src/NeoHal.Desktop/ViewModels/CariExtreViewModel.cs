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

/// <summary>
/// Cari Hesap Ekstre görüntüleme
/// </summary>
public partial class CariExtreViewModel : ViewModelBase
{
    private readonly ICariHesapService _cariHesapService;
    private readonly ICariHareketService _cariHareketService;

    [ObservableProperty]
    private ObservableCollection<CariHesap> _cariHesaplar = new();

    [ObservableProperty]
    private CariHesap? _selectedCari;

    [ObservableProperty]
    private ObservableCollection<CariHareket> _hareketler = new();

    [ObservableProperty]
    private DateTimeOffset? _baslangicTarihi = DateTimeOffset.Now.AddMonths(-1);

    [ObservableProperty]
    private DateTimeOffset? _bitisTarihi = DateTimeOffset.Now;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // Özet Bilgiler
    [ObservableProperty]
    private decimal _toplamBorc;

    [ObservableProperty]
    private decimal _toplamAlacak;

    [ObservableProperty]
    private decimal _bakiye;

    [ObservableProperty]
    private string _bakiyeDurumu = string.Empty;

    public CariExtreViewModel(
        ICariHesapService cariHesapService,
        ICariHareketService cariHareketService)
    {
        _cariHesapService = cariHesapService;
        _cariHareketService = cariHareketService;
        
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            StatusMessage = "Yükleniyor...";
            var cariler = await _cariHesapService.GetAllAsync();
            CariHesaplar = new ObservableCollection<CariHesap>(cariler.OrderBy(c => c.Unvan));
            StatusMessage = $"{CariHesaplar.Count} cari hesap yüklendi.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Hata: {ex.Message}";
        }
    }

    partial void OnSelectedCariChanged(CariHesap? value)
    {
        if (value != null)
        {
            _ = LoadExtreAsync();
        }
        else
        {
            Hareketler.Clear();
            ToplamBorc = 0;
            ToplamAlacak = 0;
            Bakiye = 0;
            BakiyeDurumu = "";
        }
    }

    [RelayCommand]
    private async Task LoadExtreAsync()
    {
        if (SelectedCari == null) return;

        try
        {
            StatusMessage = "Ekstre yükleniyor...";
            
            var baslangic = BaslangicTarihi?.DateTime ?? DateTime.Today.AddMonths(-1);
            var bitis = BitisTarihi?.DateTime ?? DateTime.Today;
            
            var hareketler = await _cariHareketService.GetByCariIdAsync(SelectedCari.Id);
            
            // Tarih filtrelemesi
            var filtrelenmis = hareketler
                .Where(h => h.Tarih >= baslangic && h.Tarih <= bitis)
                .OrderBy(h => h.Tarih);
            
            Hareketler = new ObservableCollection<CariHareket>(filtrelenmis);
            
            // Özetleri hesapla - Borç: pozitif, Alacak: negatif tutarlar
            ToplamBorc = Hareketler.Where(h => h.Tutar > 0).Sum(h => h.Tutar);
            ToplamAlacak = Hareketler.Where(h => h.Tutar < 0).Sum(h => Math.Abs(h.Tutar));
            Bakiye = ToplamBorc - ToplamAlacak;
            
            BakiyeDurumu = Bakiye > 0 ? "BORÇLU" : Bakiye < 0 ? "ALACAKLI" : "SIFIR";
            
            StatusMessage = $"✅ {Hareketler.Count} hareket listelendi.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Hata: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task YenileAsync()
    {
        await LoadExtreAsync();
    }

    [RelayCommand]
    private void FiltreTemizle()
    {
        BaslangicTarihi = DateTimeOffset.Now.AddMonths(-1);
        BitisTarihi = DateTimeOffset.Now;
        _ = LoadExtreAsync();
    }
}
