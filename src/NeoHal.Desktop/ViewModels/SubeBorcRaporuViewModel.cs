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
/// Şube Borç/Alacak Raporu
/// Şubelerin bize olan borçlarını gösterir
/// </summary>
public partial class SubeBorcRaporuViewModel : ViewModelBase
{
    private readonly ICariHesapService _cariService;
    private readonly ISatisFaturasiService _faturaService;

    [ObservableProperty]
    private ObservableCollection<SubeBorcOzeti> _subeBorclari = new();

    [ObservableProperty]
    private SubeBorcOzeti? _selectedSube;

    [ObservableProperty]
    private ObservableCollection<SatisFaturasi> _subeFaturalari = new();

    [ObservableProperty]
    private string _statusMessage = "Yükleniyor...";

    [ObservableProperty]
    private decimal _toplamBorc;

    [ObservableProperty]
    private decimal _toplamAlacak;

    [ObservableProperty]
    private decimal _netBakiye;

    [ObservableProperty]
    private DateTimeOffset? _baslangicTarih = DateTimeOffset.Now.AddMonths(-1);

    [ObservableProperty]
    private DateTimeOffset? _bitisTarih = DateTimeOffset.Now;

    public SubeBorcRaporuViewModel(
        ICariHesapService cariService,
        ISatisFaturasiService faturaService)
    {
        _cariService = cariService;
        _faturaService = faturaService;

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            StatusMessage = "Şube borçları yükleniyor...";

            var tumCariler = await _cariService.GetAllAsync();
            var subeler = tumCariler.Where(c => 
                c.CariTipi == CariTipi.Alici ||
                c.CariTipiDetay == CariTipiDetay.Tuccar ||
                c.CariTipiDetay == CariTipiDetay.MarketZinciri ||
                c.CariTipiDetay == CariTipiDetay.ManavDukkan).ToList();

            var tumFaturalar = await _faturaService.GetAllAsync();

            var borcListesi = new ObservableCollection<SubeBorcOzeti>();

            foreach (var sube in subeler)
            {
                var subeFaturalari = tumFaturalar.Where(f => f.AliciId == sube.Id).ToList();
                
                var toplamFatura = subeFaturalari.Sum(f => f.GenelToplam);
                var odenen = subeFaturalari.Sum(f => f.OdenenTutar);
                var kalanBorc = toplamFatura - odenen;

                if (toplamFatura > 0 || sube.Bakiye != 0)
                {
                    borcListesi.Add(new SubeBorcOzeti
                    {
                        Sube = sube,
                        ToplamFatura = toplamFatura,
                        OdenenTutar = odenen,
                        KalanBorc = kalanBorc,
                        FaturaSayisi = subeFaturalari.Count,
                        SonFaturaTarihi = subeFaturalari.OrderByDescending(f => f.FaturaTarihi).FirstOrDefault()?.FaturaTarihi
                    });
                }
            }

            SubeBorclari = new ObservableCollection<SubeBorcOzeti>(
                borcListesi.OrderByDescending(b => b.KalanBorc));

            ToplamBorc = SubeBorclari.Sum(s => s.KalanBorc > 0 ? s.KalanBorc : 0);
            ToplamAlacak = SubeBorclari.Sum(s => s.KalanBorc < 0 ? Math.Abs(s.KalanBorc) : 0);
            NetBakiye = SubeBorclari.Sum(s => s.KalanBorc);

            StatusMessage = $"{SubeBorclari.Count} şube listelendi. Toplam alacak: {ToplamBorc:N2} ₺";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Hata: {ex.Message}";
        }
    }

    partial void OnSelectedSubeChanged(SubeBorcOzeti? value)
    {
        if (value != null)
        {
            _ = LoadSubeFaturalariAsync(value.Sube.Id);
        }
        else
        {
            SubeFaturalari.Clear();
        }
    }

    private async Task LoadSubeFaturalariAsync(Guid subeId)
    {
        try
        {
            var tumFaturalar = await _faturaService.GetAllAsync();
            var subeFaturalari = tumFaturalar
                .Where(f => f.AliciId == subeId)
                .OrderByDescending(f => f.FaturaTarihi)
                .ToList();

            SubeFaturalari = new ObservableCollection<SatisFaturasi>(subeFaturalari);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fatura yükleme hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task YenileAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task FiltreleAsync()
    {
        // Tarih aralığına göre filtrele
        await LoadDataAsync();
    }
}

/// <summary>
/// Şube borç özeti modeli
/// </summary>
public class SubeBorcOzeti
{
    public CariHesap Sube { get; set; } = null!;
    public decimal ToplamFatura { get; set; }
    public decimal OdenenTutar { get; set; }
    public decimal KalanBorc { get; set; }
    public int FaturaSayisi { get; set; }
    public DateTime? SonFaturaTarihi { get; set; }
    
    public string DurumText => KalanBorc > 0 ? "Borçlu" : KalanBorc < 0 ? "Alacaklı" : "Denk";
    public string DurumRenk => KalanBorc > 0 ? "#F44336" : KalanBorc < 0 ? "#4CAF50" : "#888";
}
