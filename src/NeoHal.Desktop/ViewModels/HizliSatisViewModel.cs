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

/// <summary>
/// Excel tarzı hızlı satış giriş ekranı ViewModel
/// Özellikler:
/// - PAT yazınca Patlıcan gelir (AutoComplete)
/// - F10 ile ürün listesi, F12 ile kap listesi
/// - Enter ile yeni satıra geç
/// - Tab ile sütunlar arası geç
/// </summary>
public partial class HizliSatisViewModel : ViewModelBase
{
    private readonly ISatisFaturasiService _faturaService;
    private readonly ICariHesapService _cariHesapService;
    private readonly IUrunService _urunService;
    private readonly IKapTipiService _kapTipiService;
    private readonly IGirisIrsaliyesiService _irsaliyeService;
    private readonly Action<bool>? _closeCallback;

    [ObservableProperty]
    private string _faturaNo = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _faturaTarihi = DateTimeOffset.Now;

    [ObservableProperty]
    private CariHesap? _selectedAlici;

    [ObservableProperty]
    private string _statusMessage = "Hazır. F9=Fatura Oluştur, F10=Ürün Listesi, F12=Kap Listesi";

    // Listeler
    [ObservableProperty]
    private ObservableCollection<CariHesap> _cariler = new();

    [ObservableProperty]
    private ObservableCollection<Urun> _urunler = new();

    [ObservableProperty]
    private ObservableCollection<KapTipi> _kapTipleri = new();

    [ObservableProperty]
    private ObservableCollection<HizliSatisKalem> _kalemler = new();

    [ObservableProperty]
    private HizliSatisKalem? _selectedKalem;

    // Toplamlar
    public int ToplamKap => Kalemler.Sum(k => k.KapAdet);
    public decimal ToplamKg => Kalemler.Sum(k => k.NetKg);
    public decimal AraToplam => Kalemler.Sum(k => k.Tutar);
    public decimal ToplamKdv => AraToplam * 0.01m; // %1 KDV
    public decimal ToplamRusum => AraToplam * 0.01m; // %1 Rüsum
    public decimal GenelToplam => AraToplam + ToplamKdv;

    // Window close event
    public event EventHandler<bool>? CloseRequested;

    public HizliSatisViewModel(
        ISatisFaturasiService faturaService,
        ICariHesapService cariHesapService,
        IUrunService urunService,
        IKapTipiService kapTipiService,
        IGirisIrsaliyesiService irsaliyeService,
        Action<bool>? closeCallback = null)
    {
        _faturaService = faturaService;
        _cariHesapService = cariHesapService;
        _urunService = urunService;
        _kapTipiService = kapTipiService;
        _irsaliyeService = irsaliyeService;
        _closeCallback = closeCallback;

        // Boş bir satır ekle (hızlı giriş için)
        AddNewKalem();

        _ = LoadDataAsync();
    }
    
    private void AddNewKalem()
    {
        var kalem = new HizliSatisKalem();
        kalem.SetParent(this);
        Kalemler.Add(kalem);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var alicilar = await _cariHesapService.GetByCariTipiAsync(CariTipi.Alici);
            Cariler = new ObservableCollection<CariHesap>(alicilar);

            var urunler = await _urunService.GetActiveAsync();
            Urunler = new ObservableCollection<Urun>(urunler);

            var kapTipleri = await _kapTipiService.GetActiveAsync();
            KapTipleri = new ObservableCollection<KapTipi>(kapTipleri);

            FaturaNo = await _faturaService.GenerateNewFaturaNoAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Veri yükleme hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private void YeniSatir()
    {
        AddNewKalem();
        RefreshTotals();
    }

    [RelayCommand]
    private void SatirSil(HizliSatisKalem kalem)
    {
        if (Kalemler.Count > 1)
        {
            Kalemler.Remove(kalem);
            RefreshTotals();
        }
    }

    // Lot seçimi için property'ler
    [ObservableProperty]
    private bool _lotSecimDialogAcik;

    [ObservableProperty]
    private ObservableCollection<LotSecimItem> _mevcutLotlar = new();

    [ObservableProperty]
    private LotSecimItem? _selectedLot;

    [ObservableProperty]
    private HizliSatisKalem? _lotSecimKalem;

    [RelayCommand]
    private async Task LotSecAsync(HizliSatisKalem kalem)
    {
        if (kalem.Urun == null)
        {
            StatusMessage = "Önce ürün seçmelisiniz!";
            return;
        }

        try
        {
            StatusMessage = "Lotlar yükleniyor...";
            LotSecimKalem = kalem;

            // İrsaliye kalemlerinden mevcut stokları FIFO sırasıyla getir
            var irsaliyeler = await _irsaliyeService.GetAllAsync();
            var lotlar = new List<LotSecimItem>();

            foreach (var irsaliye in irsaliyeler.OrderBy(i => i.Tarih))
            {
                foreach (var ik in irsaliye.Kalemler.Where(k => k.UrunId == kalem.Urun.Id && k.KalanKg > 0))
                {
                    lotlar.Add(new LotSecimItem
                    {
                        IrsaliyeKalemId = ik.Id,
                        IrsaliyeNo = irsaliye.IrsaliyeNo,
                        IrsaliyeTarihi = irsaliye.Tarih,
                        MustahsilAdi = irsaliye.Mustahsil?.Unvan ?? "-",
                        KalanKg = ik.KalanKg,
                        KalanKap = ik.KalanKapAdet,
                        BirimFiyat = ik.BirimFiyat,
                        KapTipiAdi = ik.KapTipi?.Ad ?? "-",
                        Kalem = ik
                    });
                }
            }

            MevcutLotlar = new ObservableCollection<LotSecimItem>(lotlar);
            
            if (!MevcutLotlar.Any())
            {
                StatusMessage = $"⚠️ {kalem.Urun.Ad} için stokta mal bulunamadı!";
                return;
            }

            LotSecimDialogAcik = true;
            StatusMessage = $"📦 {MevcutLotlar.Count} lot mevcut - FIFO sırasıyla listelendi";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lot yükleme hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private void LotSecimOnayla()
    {
        if (SelectedLot == null || LotSecimKalem == null)
        {
            StatusMessage = "Lütfen bir lot seçin!";
            return;
        }

        // Seçilen lotu kaleme ata
        LotSecimKalem.LotNo = SelectedLot.IrsaliyeNo;
        LotSecimKalem.GirisKalemId = SelectedLot.IrsaliyeKalemId;
        LotSecimKalem.KapTipi = SelectedLot.Kalem.KapTipi;
        
        // Varsayılan olarak mevcut miktarı set et
        if (LotSecimKalem.KapAdet == 0)
            LotSecimKalem.KapAdet = SelectedLot.KalanKap;
        if (LotSecimKalem.BrutKg == 0)
            LotSecimKalem.BrutKg = SelectedLot.KalanKg + (SelectedLot.Kalem.DaraKg / SelectedLot.Kalem.KapAdet * SelectedLot.KalanKap);
        
        // Müstahsil fiyatını öner
        if (SelectedLot.BirimFiyat.HasValue && LotSecimKalem.BirimFiyat == 0)
            LotSecimKalem.BirimFiyat = SelectedLot.BirimFiyat.Value;

        LotSecimDialogAcik = false;
        StatusMessage = $"✅ Lot seçildi: {SelectedLot.IrsaliyeNo} - {SelectedLot.MustahsilAdi}";
        RefreshTotals();
    }

    [RelayCommand]
    private void LotSecimIptal()
    {
        LotSecimDialogAcik = false;
        StatusMessage = "Lot seçimi iptal edildi.";
    }

    private void RefreshTotals()
    {
        OnPropertyChanged(nameof(ToplamKap));
        OnPropertyChanged(nameof(ToplamKg));
        OnPropertyChanged(nameof(AraToplam));
        OnPropertyChanged(nameof(ToplamKdv));
        OnPropertyChanged(nameof(ToplamRusum));
        OnPropertyChanged(nameof(GenelToplam));
    }

    [RelayCommand]
    private async Task TaslakKaydetAsync()
    {
        try
        {
            StatusMessage = "Taslak kaydediliyor...";
            // TODO: Taslak kaydetme
            await Task.Delay(500);
            StatusMessage = "Taslak kaydedildi.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task FaturaOlusturAsync()
    {
        try
        {
            if (SelectedAlici == null)
            {
                StatusMessage = "Lütfen alıcı seçin!";
                return;
            }

            var gecerliKalemler = Kalemler.Where(k => k.Urun != null && k.NetKg > 0).ToList();
            if (!gecerliKalemler.Any())
            {
                StatusMessage = "En az bir geçerli kalem girmelisiniz!";
                return;
            }

            // Risk limiti kontrolü
            var riskDurumu = await _cariHesapService.CheckRiskLimitiAsync(SelectedAlici.Id, GenelToplam);
            if (riskDurumu.LimitAsildi)
            {
                StatusMessage = $"🚫 RİSK LİMİTİ AŞILDI! Bakiye: {riskDurumu.MevcutBakiye:N2} ₺, Limit: {riskDurumu.RiskLimiti:N2} ₺, İşlem: {GenelToplam:N2} ₺";
                return;
            }

            var fatura = new SatisFaturasi
            {
                FaturaNo = FaturaNo,
                FaturaTarihi = FaturaTarihi?.DateTime ?? DateTime.Today,
                AliciId = SelectedAlici.Id,
                FaturaTipi = FaturaTipi.Toptan,
                AraToplam = AraToplam,
                KdvTutari = ToplamKdv,
                RusumTutari = ToplamRusum,
                GenelToplam = GenelToplam,
                Kalemler = gecerliKalemler.Select(k => new SatisFaturasiKalem
                {
                    Id = Guid.NewGuid(),
                    UrunId = k.Urun!.Id,
                    Urun = k.Urun,
                    KapTipiId = k.KapTipi?.Id ?? Guid.Empty,
                    KapTipi = k.KapTipi!,
                    KapAdet = k.KapAdet,
                    BrutKg = k.BrutKg,
                    DaraKg = k.DaraKg,
                    NetKg = k.NetKg,
                    BirimFiyat = k.BirimFiyat,
                    Tutar = k.Tutar,
                    RusumOrani = 1,
                    KomisyonOrani = 8,
                    StopajOrani = 4,
                    RusumTutari = k.Tutar * 0.01m,
                    KomisyonTutari = k.Tutar * 0.08m,
                    StopajTutari = k.Tutar * 0.04m
                }).ToList()
            };

            await _faturaService.CreateAsync(fatura);
            StatusMessage = $"Fatura {FaturaNo} oluşturuldu!";
            
            await Task.Delay(1000);
            _closeCallback?.Invoke(true);
            CloseRequested?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fatura hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Iptal()
    {
        _closeCallback?.Invoke(false);
        CloseRequested?.Invoke(this, false);
    }

    // Kalem değişikliklerinde toplamları güncelle
    public void OnKalemChanged()
    {
        RefreshTotals();
        // NOT: Otomatik satır ekleme kaldırıldı - kullanıcı F5 veya "Yeni Satır" butonu kullanmalı
    }
}

/// <summary>
/// Hızlı satış kalemi - Excel satırı gibi
/// AutoComplete ile hızlı seçim destekler
/// </summary>
public partial class HizliSatisKalem : ObservableObject
{
    private HizliSatisViewModel? _parentViewModel;
    
    public void SetParent(HizliSatisViewModel parent)
    {
        _parentViewModel = parent;
    }
    
    [ObservableProperty]
    private Urun? _urun;

    [ObservableProperty]
    private KapTipi? _kapTipi;

    [ObservableProperty]
    private int _kapAdet;

    [ObservableProperty]
    private decimal _brutKg;

    [ObservableProperty]
    private decimal _daraKg;

    [ObservableProperty]
    private decimal _birimFiyat;

    [ObservableProperty]
    private string? _lotNo;

    [ObservableProperty]
    private Guid? _girisKalemId;  // FIFO takibi için

    public decimal NetKg => BrutKg - DaraKg;
    public decimal Tutar => NetKg * BirimFiyat;

    partial void OnKapTipiChanged(KapTipi? value)
    {
        if (value != null && KapAdet > 0)
        {
            DaraKg = value.DaraAgirlik * KapAdet;
        }
        OnPropertyChanged(nameof(NetKg));
        OnPropertyChanged(nameof(Tutar));
    }

    partial void OnKapAdetChanged(int value)
    {
        if (KapTipi != null)
        {
            DaraKg = KapTipi.DaraAgirlik * value;
        }
        OnPropertyChanged(nameof(NetKg));
        OnPropertyChanged(nameof(Tutar));
    }

    partial void OnBrutKgChanged(decimal value)
    {
        OnPropertyChanged(nameof(NetKg));
        OnPropertyChanged(nameof(Tutar));
    }

    partial void OnDaraKgChanged(decimal value)
    {
        OnPropertyChanged(nameof(NetKg));
        OnPropertyChanged(nameof(Tutar));
    }

    partial void OnBirimFiyatChanged(decimal value)
    {
        OnPropertyChanged(nameof(Tutar));
        _parentViewModel?.OnKalemChanged();
    }
    
    partial void OnUrunChanged(Urun? value)
    {
        _parentViewModel?.OnKalemChanged();
    }
}

/// <summary>
/// Lot seçimi için item
/// </summary>
public class LotSecimItem
{
    public Guid IrsaliyeKalemId { get; set; }
    public string IrsaliyeNo { get; set; } = string.Empty;
    public DateTime IrsaliyeTarihi { get; set; }
    public string MustahsilAdi { get; set; } = string.Empty;
    public decimal KalanKg { get; set; }
    public int KalanKap { get; set; }
    public decimal? BirimFiyat { get; set; }
    public string KapTipiAdi { get; set; } = string.Empty;
    public GirisIrsaliyesiKalem Kalem { get; set; } = null!;
    
    public string Ozet => $"{IrsaliyeNo} | {IrsaliyeTarihi:dd.MM.yy} | {MustahsilAdi} | {KalanKg:N1}kg / {KalanKap} kap";
}
