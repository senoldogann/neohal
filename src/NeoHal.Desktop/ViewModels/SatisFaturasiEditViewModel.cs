using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeoHal.Core.Entities;
using NeoHal.Core.Enums;
using NeoHal.Services.Interfaces;

namespace NeoHal.Desktop.ViewModels;

public partial class SatisFaturasiEditViewModel : ViewModelBase
{
    private readonly ISatisFaturasiService _faturaService;
    private readonly ICariHesapService _cariHesapService;
    private readonly IGirisIrsaliyesiService _irsaliyeService;
    private readonly IUrunService _urunService;
    private readonly IKapTipiService _kapTipiService;
    private readonly Action<bool>? _closeCallback;
    
    private SatisFaturasi? _existingFatura;
    
    // Window events
    public event EventHandler<bool>? CloseRequested;
    public event EventHandler<(string Title, string Html, Action<bool> Callback)>? PrintPreviewRequested;

    [ObservableProperty]
    private string _windowTitle = "Yeni Satış Faturası";

    [ObservableProperty]
    private string _faturaNo = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _tarih = DateTimeOffset.Now;

    [ObservableProperty]
    private CariHesap? _selectedAlici;

    [ObservableProperty]
    private string _aciklama = string.Empty;

    [ObservableProperty]
    private FaturaTipi _selectedFaturaTipi = FaturaTipi.Toptan;

    [ObservableProperty]
    private decimal _birimFiyat;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // Listeler
    [ObservableProperty]
    private ObservableCollection<CariHesap> _alicilar = new();

    [ObservableProperty]
    private ObservableCollection<GirisIrsaliyesi> _bekleyenIrsaliyeler = new();

    [ObservableProperty]
    private GirisIrsaliyesi? _selectedIrsaliye;

    [ObservableProperty]
    private ObservableCollection<SatisFaturasiKalem> _kalemler = new();

    [ObservableProperty]
    private SatisFaturasiKalem? _selectedKalem;
    
    [ObservableProperty]
    private ObservableCollection<Urun> _urunler = new();

    [ObservableProperty]
    private ObservableCollection<KapTipi> _kapTipleri = new();

    public FaturaTipi[] FaturaTipleri { get; } = Enum.GetValues<FaturaTipi>();

    // Hesaplanan - Sadece girilen değerler toplanır, otomatik KDV eklenmez
    public decimal AraToplam => Kalemler.Sum(k => k.Tutar);
    public decimal ToplamKdv => 0; // KDV otomatik eklenmez, kullanıcı isterse manuel girer
    public decimal ToplamRusum => Kalemler.Sum(k => k.RusumTutari);
    public decimal ToplamKomisyon => Kalemler.Sum(k => k.KomisyonTutari);
    public decimal ToplamStopaj => Kalemler.Sum(k => k.StopajTutari);
    public decimal GenelToplam => AraToplam; // Sadece ara toplam, KDV dahil değil
    public int ToplamKap => Kalemler.Sum(k => k.KapAdet);
    public decimal ToplamKg => Kalemler.Sum(k => k.NetKg);

    public SatisFaturasiEditViewModel(
        ISatisFaturasiService faturaService,
        ICariHesapService cariHesapService,
        IGirisIrsaliyesiService irsaliyeService,
        IUrunService urunService,
        IKapTipiService kapTipiService,
        Action<bool>? closeCallback = null,
        SatisFaturasi? existingFatura = null)
    {
        _faturaService = faturaService;
        _cariHesapService = cariHesapService;
        _irsaliyeService = irsaliyeService;
        _urunService = urunService;
        _kapTipiService = kapTipiService;
        _closeCallback = closeCallback;
        _existingFatura = existingFatura;

        if (existingFatura != null)
        {
            WindowTitle = "Fatura Düzenle";
            FaturaNo = existingFatura.FaturaNo;
            Tarih = new DateTimeOffset(existingFatura.FaturaTarihi);
            Aciklama = existingFatura.Aciklama ?? string.Empty;
            SelectedFaturaTipi = existingFatura.FaturaTipi;
            Kalemler = new ObservableCollection<SatisFaturasiKalem>(existingFatura.Kalemler);
        }

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var alicilar = await _cariHesapService.GetByCariTipiAsync(CariTipi.Alici);
            Alicilar = new ObservableCollection<CariHesap>(alicilar);

            var urunler = await _urunService.GetActiveAsync();
            Urunler = new ObservableCollection<Urun>(urunler);

            var kapTipleri = await _kapTipiService.GetActiveAsync();
            KapTipleri = new ObservableCollection<KapTipi>(kapTipleri);

            // Onaylanmış irsaliyeleri getir (durum = Onaylandi)
            var baslangic = DateTime.Today.AddDays(-90);
            var bitis = DateTime.Today.AddDays(1);
            var tumIrsaliyeler = await _irsaliyeService.GetByDateRangeAsync(baslangic, bitis);
            var onayliIrsaliyeler = tumIrsaliyeler.Where(i => i.Durum == BelgeDurumu.Onaylandi);
            BekleyenIrsaliyeler = new ObservableCollection<GirisIrsaliyesi>(onayliIrsaliyeler);

            if (_existingFatura == null)
            {
                FaturaNo = await _faturaService.GenerateNewFaturaNoAsync();
            }
            else if (_existingFatura != null)
            {
                SelectedAlici = Alicilar.FirstOrDefault(a => a.Id == _existingFatura.AliciId);
                
                // Mevcut fatura için kaynak irsaliyeleri bul ve göster
                var kaynakIrsaliyeIdler = Kalemler
                    .Where(k => k.GirisKalemId.HasValue && k.GirisKalem?.Irsaliye != null)
                    .Select(k => k.GirisKalem!.Irsaliye!.IrsaliyeNo)
                    .Distinct()
                    .ToList();
                
                if (kaynakIrsaliyeIdler.Any())
                {
                    StatusMessage = $"📋 Kaynak İrsaliyeler: {string.Join(", ", kaynakIrsaliyeIdler)}";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AddKalem()
    {
        var kalem = new SatisFaturasiKalem
        {
            Id = Guid.NewGuid(),
            KapAdet = 1,
            NetKg = 0,
            BirimFiyat = 0,
            RusumOrani = 0,
            KomisyonOrani = 0,
            StopajOrani = 0
        };
        Kalemler.Add(kalem);
        SelectedKalem = kalem;
        RefreshTotals();
    }

    /// <summary>
    /// Seçili kalem için tutarları hesapla
    /// </summary>
    [RelayCommand]
    public void RecalculateKalem(SatisFaturasiKalem? kalem = null)
    {
        var targetKalem = kalem ?? SelectedKalem;
        if (targetKalem == null) return;

        targetKalem.Tutar = targetKalem.NetKg * targetKalem.BirimFiyat;
        targetKalem.RusumTutari = targetKalem.Tutar * targetKalem.RusumOrani / 100;
        targetKalem.KomisyonTutari = targetKalem.Tutar * targetKalem.KomisyonOrani / 100;
        targetKalem.StopajTutari = targetKalem.Tutar * targetKalem.StopajOrani / 100;

        // UI'ı güncelle - kalem listesini yenile
        var index = Kalemler.IndexOf(targetKalem);
        if (index >= 0)
        {
            Kalemler.RemoveAt(index);
            Kalemler.Insert(index, targetKalem);
            SelectedKalem = targetKalem;
        }
        
        RefreshTotals();
    }

    [RelayCommand]
    private void AddIrsaliye()
    {
        if (SelectedIrsaliye == null)
        {
            StatusMessage = "İrsaliye seçmelisiniz!";
            return;
        }

        foreach (var irsaliyeKalem in SelectedIrsaliye.Kalemler)
        {
            if (irsaliyeKalem.KalanKg <= 0) continue;

            var kalem = new SatisFaturasiKalem
            {
                Id = Guid.NewGuid(),
                GirisKalemId = irsaliyeKalem.Id,
                UrunId = irsaliyeKalem.UrunId,
                Urun = irsaliyeKalem.Urun!,
                KapTipiId = irsaliyeKalem.KapTipiId,
                KapTipi = irsaliyeKalem.KapTipi!,
                KapAdet = irsaliyeKalem.KalanKapAdet,
                BrutKg = irsaliyeKalem.BrutKg,
                DaraKg = irsaliyeKalem.DaraKg,
                NetKg = irsaliyeKalem.KalanKg,
                BirimFiyat = BirimFiyat,
                RusumOrani = 0,
                KomisyonOrani = 0,
                StopajOrani = 0
            };
            
            kalem.Tutar = kalem.NetKg * kalem.BirimFiyat;
            kalem.RusumTutari = kalem.Tutar * kalem.RusumOrani / 100;
            kalem.KomisyonTutari = kalem.Tutar * kalem.KomisyonOrani / 100;
            kalem.StopajTutari = kalem.Tutar * kalem.StopajOrani / 100;

            Kalemler.Add(kalem);
        }

        BekleyenIrsaliyeler.Remove(SelectedIrsaliye);
        SelectedIrsaliye = null;
        RefreshTotals();
        StatusMessage = "İrsaliye kalemleri eklendi.";
    }

    [RelayCommand]
    private void ApplyPrice()
    {
        if (SelectedKalem == null)
        {
            StatusMessage = "Önce bir kalem seçin!";
            return;
        }

        SelectedKalem.BirimFiyat = BirimFiyat;
        SelectedKalem.Tutar = SelectedKalem.NetKg * BirimFiyat;
        SelectedKalem.RusumTutari = SelectedKalem.Tutar * SelectedKalem.RusumOrani / 100;
        SelectedKalem.KomisyonTutari = SelectedKalem.Tutar * SelectedKalem.KomisyonOrani / 100;
        SelectedKalem.StopajTutari = SelectedKalem.Tutar * SelectedKalem.StopajOrani / 100;
        
        // Refresh
        var index = Kalemler.IndexOf(SelectedKalem);
        var item = SelectedKalem;
        Kalemler.RemoveAt(index);
        Kalemler.Insert(index, item);
        SelectedKalem = item;
        
        RefreshTotals();
        StatusMessage = "Fiyat uygulandı.";
    }

    [RelayCommand]
    private void RemoveKalem(SatisFaturasiKalem kalem)
    {
        Kalemler.Remove(kalem);
        RefreshTotals();
    }

    private void RefreshTotals()
    {
        OnPropertyChanged(nameof(AraToplam));
        OnPropertyChanged(nameof(ToplamKdv));
        OnPropertyChanged(nameof(ToplamRusum));
        OnPropertyChanged(nameof(ToplamKomisyon));
        OnPropertyChanged(nameof(ToplamStopaj));
        OnPropertyChanged(nameof(GenelToplam));
        OnPropertyChanged(nameof(ToplamKap));
        OnPropertyChanged(nameof(ToplamKg));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            if (SelectedAlici == null)
            {
                StatusMessage = "⚠️ Alıcı seçmelisiniz!";
                return;
            }

            if (!Kalemler.Any())
            {
                StatusMessage = "⚠️ En az bir kalem eklemelisiniz!";
                return;
            }

            // Kalem kontrolü: Tutar 0 olan satırlar var mı?
            var sifirTutarliKalemler = Kalemler.Where(k => k.Tutar <= 0).ToList();
            if (sifirTutarliKalemler.Any())
            {
                StatusMessage = "⚠️ Tutarı 0 veya negatif olan kalemler var! Lütfen kontrol edin.";
                return;
            }

            // Net kg 0 olan satırlar var mı?
            var sifirKgKalemler = Kalemler.Where(k => k.NetKg <= 0).ToList();
            if (sifirKgKalemler.Any())
            {
                StatusMessage = "⚠️ Net kg değeri 0 veya negatif olan kalemler var!";
                return;
            }

            if (_existingFatura == null)
            {
                var fatura = new SatisFaturasi
                {
                    FaturaNo = FaturaNo,
                    FaturaTarihi = Tarih?.DateTime ?? DateTime.Today,
                    AliciId = SelectedAlici.Id,
                    FaturaTipi = SelectedFaturaTipi,
                    Aciklama = Aciklama,
                    AraToplam = AraToplam,
                    KdvTutari = ToplamKdv,
                    RusumTutari = ToplamRusum,
                    KomisyonTutari = ToplamKomisyon,
                    GenelToplam = GenelToplam,
                    Kalemler = Kalemler.ToList()
                };
                await _faturaService.CreateAsync(fatura);
                StatusMessage = "✅ Fatura kaydedildi!";
            }
            else
            {
                _existingFatura.FaturaTarihi = Tarih?.DateTime ?? DateTime.Today;
                _existingFatura.AliciId = SelectedAlici.Id;
                _existingFatura.FaturaTipi = SelectedFaturaTipi;
                _existingFatura.Aciklama = Aciklama;
                _existingFatura.AraToplam = AraToplam;
                _existingFatura.KdvTutari = ToplamKdv;
                _existingFatura.RusumTutari = ToplamRusum;
                _existingFatura.KomisyonTutari = ToplamKomisyon;
                _existingFatura.GenelToplam = GenelToplam;
                await _faturaService.UpdateAsync(_existingFatura);
                StatusMessage = "✅ Fatura güncellendi!";
            }

            await Task.Delay(500);
            _closeCallback?.Invoke(true);
            CloseRequested?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Kayıt hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task PrintAsync()
    {
        try
        {
            if (SelectedAlici == null)
            {
                StatusMessage = "Yazdırmak için önce alıcı seçmelisiniz!";
                return;
            }
            
            if (!Kalemler.Any())
            {
                StatusMessage = "Yazdırmak için en az bir kalem eklemelisiniz!";
                return;
            }
            
            // Fatura verilerini oluştur
            var fatura = new SatisFaturasi
            {
                FaturaNo = FaturaNo,
                FaturaTarihi = Tarih?.DateTime ?? DateTime.Today,
                Alici = SelectedAlici,
                Kalemler = Kalemler.ToList(),
                AraToplam = AraToplam,
                KdvTutari = ToplamKdv,
                RusumTutari = ToplamRusum,
                KomisyonTutari = ToplamKomisyon,
                StopajTutari = ToplamStopaj,
                GenelToplam = GenelToplam
            };
            
            // Print servisinden HTML oluştur
            var printService = new NeoHal.Services.PrintService();
            var html = await printService.GenerateFaturaPreviewHtmlAsync(fatura);
            
            // Print preview event'i tetikle (View tarafında yakalanacak)
            PrintPreviewRequested?.Invoke(this, ($"Satış Faturası - {FaturaNo}", html, (printed) =>
            {
                if (printed)
                    StatusMessage = "Belge yazdırıldı.";
            }));
            
            StatusMessage = "Yazdırma önizlemesi açılıyor...";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Yazdırma hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _closeCallback?.Invoke(false);
        CloseRequested?.Invoke(this, false);
    }

    /// <summary>
    /// Fatura Kes - Taslak faturayı Onaylandı durumuna getir
    /// </summary>
    [RelayCommand]
    private async Task FaturaKesAsync()
    {
        try
        {
            if (SelectedAlici == null)
            {
                StatusMessage = "⚠️ Alıcı seçmelisiniz!";
                return;
            }

            if (!Kalemler.Any())
            {
                StatusMessage = "⚠️ En az bir kalem eklemelisiniz!";
                return;
            }

            // Kalem kontrolü: Tutar 0 olan satırlar var mı?
            var sifirTutarliKalemler = Kalemler.Where(k => k.Tutar <= 0).ToList();
            if (sifirTutarliKalemler.Any())
            {
                StatusMessage = "⚠️ Tutarı 0 veya negatif olan kalemler var!";
                return;
            }

            // Net kg 0 olan satırlar var mı?
            var sifirKgKalemler = Kalemler.Where(k => k.NetKg <= 0).ToList();
            if (sifirKgKalemler.Any())
            {
                StatusMessage = "⚠️ Net kg değeri 0 veya negatif olan kalemler var!";
                return;
            }

            if (_existingFatura == null)
            {
                // Yeni fatura oluştur ve direkt Onaylandı yap
                var fatura = new SatisFaturasi
                {
                    FaturaNo = FaturaNo,
                    FaturaTarihi = Tarih?.DateTime ?? DateTime.Today,
                    AliciId = SelectedAlici.Id,
                    FaturaTipi = SelectedFaturaTipi,
                    Aciklama = Aciklama,
                    AraToplam = AraToplam,
                    KdvTutari = ToplamKdv,
                    RusumTutari = ToplamRusum,
                    KomisyonTutari = ToplamKomisyon,
                    GenelToplam = GenelToplam,
                    Durum = BelgeDurumu.Onaylandi,
                    Kalemler = Kalemler.ToList()
                };
                await _faturaService.CreateAsync(fatura);
                StatusMessage = "✅ Fatura kesildi!";
            }
            else
            {
                // Mevcut faturayı güncelle ve Onaylandı yap
                _existingFatura.FaturaTarihi = Tarih?.DateTime ?? DateTime.Today;
                _existingFatura.AliciId = SelectedAlici.Id;
                _existingFatura.FaturaTipi = SelectedFaturaTipi;
                _existingFatura.Aciklama = Aciklama;
                _existingFatura.AraToplam = AraToplam;
                _existingFatura.KdvTutari = ToplamKdv;
                _existingFatura.RusumTutari = ToplamRusum;
                _existingFatura.KomisyonTutari = ToplamKomisyon;
                _existingFatura.GenelToplam = GenelToplam;
                _existingFatura.Durum = BelgeDurumu.Onaylandi;
                await _faturaService.UpdateAsync(_existingFatura);
                StatusMessage = "✅ Fatura kesildi!";
            }

            await Task.Delay(500);
            _closeCallback?.Invoke(true);
            CloseRequested?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Fatura kesme hatası: {ex.Message}";
        }
    }
}
