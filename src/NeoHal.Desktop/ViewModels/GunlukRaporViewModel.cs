using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeoHal.Services.Interfaces;

namespace NeoHal.Desktop.ViewModels;

/// <summary>
/// Günlük Rapor ekranı - Gün sonu özeti
/// </summary>
public partial class GunlukRaporViewModel : ViewModelBase
{
    private readonly ISatisFaturasiService _faturaService;
    private readonly IGirisIrsaliyesiService _irsaliyeService;
    private readonly IKasaHesabiService _kasaService;
    private readonly ICariHareketService _cariHareketService;

    [ObservableProperty]
    private DateTimeOffset? _raporTarihi = DateTimeOffset.Now;

    [ObservableProperty]
    private string _statusMessage = "Rapor hazır";

    // Satış Özeti
    [ObservableProperty]
    private int _toplamFaturaSayisi;

    [ObservableProperty]
    private decimal _toplamSatisTutari;

    [ObservableProperty]
    private decimal _toplamKdv;

    [ObservableProperty]
    private decimal _toplamRusum;

    [ObservableProperty]
    private decimal _toplamKomisyon;

    [ObservableProperty]
    private decimal _toplamKg;

    // Giriş Özeti
    [ObservableProperty]
    private int _toplamIrsaliyeSayisi;

    [ObservableProperty]
    private decimal _toplamGirisKg;

    [ObservableProperty]
    private int _toplamGirisKap;

    // Kasa Özeti
    [ObservableProperty]
    private decimal _kasaGirisi;

    [ObservableProperty]
    private decimal _kasaCikisi;

    [ObservableProperty]
    private decimal _kasaBakiye;

    // Detay listesi
    [ObservableProperty]
    private ObservableCollection<RaporSatirItem> _satisDetay = new();

    public GunlukRaporViewModel(
        ISatisFaturasiService faturaService,
        IGirisIrsaliyesiService irsaliyeService,
        IKasaHesabiService kasaService,
        ICariHareketService cariHareketService)
    {
        _faturaService = faturaService;
        _irsaliyeService = irsaliyeService;
        _kasaService = kasaService;
        _cariHareketService = cariHareketService;

        _ = RaporOlusturAsync();
    }

    [RelayCommand]
    private async Task RaporOlusturAsync()
    {
        try
        {
            StatusMessage = "Rapor hazırlanıyor...";

            var tarih = RaporTarihi?.Date ?? DateTime.Today;
            var sonrakiGun = tarih.AddDays(1);

            // Satış verilerini getir
            var faturalar = await _faturaService.GetAllAsync();
            var gunlukFaturalar = faturalar
                .Where(f => f.FaturaTarihi >= tarih && f.FaturaTarihi < sonrakiGun)
                .ToList();

            ToplamFaturaSayisi = gunlukFaturalar.Count;
            ToplamSatisTutari = gunlukFaturalar.Sum(f => f.GenelToplam);
            ToplamKdv = gunlukFaturalar.Sum(f => f.KdvTutari);
            ToplamRusum = gunlukFaturalar.Sum(f => f.RusumTutari);
            ToplamKomisyon = gunlukFaturalar.Sum(f => f.KomisyonTutari);
            ToplamKg = gunlukFaturalar.SelectMany(f => f.Kalemler).Sum(k => k.NetKg);

            // Satış detayı
            SatisDetay = new ObservableCollection<RaporSatirItem>(
                gunlukFaturalar.Select(f => new RaporSatirItem
                {
                    FaturaNo = f.FaturaNo,
                    CariAdi = f.Alici?.Unvan ?? "-",
                    Tutar = f.GenelToplam,
                    Kg = f.Kalemler.Sum(k => k.NetKg),
                    Saat = f.FaturaTarihi.ToString("HH:mm")
                }));

            // Giriş irsaliyeleri
            var irsaliyeler = await _irsaliyeService.GetAllAsync();
            var gunlukIrsaliyeler = irsaliyeler
                .Where(i => i.Tarih >= tarih && i.Tarih < sonrakiGun)
                .ToList();

            ToplamIrsaliyeSayisi = gunlukIrsaliyeler.Count;
            ToplamGirisKg = gunlukIrsaliyeler.Sum(i => i.ToplamNet);
            ToplamGirisKap = gunlukIrsaliyeler.Sum(i => i.ToplamKapAdet);

            // Kasa hareketleri
            var kasaHareketleri = await _kasaService.GetAllAsync();
            var gunlukKasa = kasaHareketleri
                .Where(k => k.Tarih >= tarih && k.Tarih < sonrakiGun)
                .ToList();

            KasaGirisi = gunlukKasa.Where(k => k.GirisHareketi).Sum(k => k.Tutar);
            KasaCikisi = gunlukKasa.Where(k => !k.GirisHareketi).Sum(k => k.Tutar);
            KasaBakiye = KasaGirisi - KasaCikisi;

            StatusMessage = $"✅ {tarih:dd.MM.yyyy} tarihli rapor hazır - {ToplamFaturaSayisi} fatura, {ToplamSatisTutari:N2} ₺ satış";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Rapor hatası: {ex.Message}";
        }
    }

    partial void OnRaporTarihiChanged(DateTimeOffset? value)
    {
        _ = RaporOlusturAsync();
    }
}

/// <summary>
/// Rapor satır item
/// </summary>
public class RaporSatirItem
{
    public string FaturaNo { get; set; } = string.Empty;
    public string CariAdi { get; set; } = string.Empty;
    public decimal Tutar { get; set; }
    public decimal Kg { get; set; }
    public string Saat { get; set; } = string.Empty;
}
