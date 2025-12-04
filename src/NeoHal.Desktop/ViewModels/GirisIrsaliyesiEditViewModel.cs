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

public partial class GirisIrsaliyesiEditViewModel : ViewModelBase
{
    private readonly IGirisIrsaliyesiService _irsaliyeService;
    private readonly ICariHesapService _cariHesapService;
    private readonly IUrunService _urunService;
    private readonly IKapTipiService _kapTipiService;
    private readonly Action<bool>? _closeCallback;
    
    private GirisIrsaliyesi? _existingIrsaliye;

    // Event for closing window
    public event EventHandler<bool>? CloseRequested;

    [ObservableProperty]
    private string _windowTitle = "Yeni Giriş İrsaliyesi";

    [ObservableProperty]
    private string _irsaliyeNo = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _tarih = DateTimeOffset.Now;

    [ObservableProperty]
    private CariHesap? _selectedMustahsil;

    [ObservableProperty]
    private string _aciklama = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // Listeler
    [ObservableProperty]
    private ObservableCollection<CariHesap> _mustahsiller = new();

    [ObservableProperty]
    private ObservableCollection<CariHesap> _komisyoncular = new();

    [ObservableProperty]
    private ObservableCollection<Urun> _urunler = new();

    [ObservableProperty]
    private ObservableCollection<KapTipi> _kapTipleri = new();

    [ObservableProperty]
    private ObservableCollection<GirisKalem> _kalemler = new();

    [ObservableProperty]
    private GirisKalem? _selectedKalem;

    // Hesaplanan
    public decimal ToplamKapAdet => Kalemler.Sum(k => k.KapAdet);
    public decimal ToplamDaraliKg => Kalemler.Sum(k => k.DaraliKg);
    public decimal ToplamDara => Kalemler.Sum(k => k.DaraKg);
    public decimal ToplamNet => Kalemler.Sum(k => k.NetKg);
    public decimal ToplamTutar => Kalemler.Sum(k => k.Tutar);

    public GirisIrsaliyesiEditViewModel(
        IGirisIrsaliyesiService irsaliyeService,
        ICariHesapService cariHesapService,
        IUrunService urunService,
        IKapTipiService kapTipiService,
        Action<bool>? closeCallback = null,
        GirisIrsaliyesi? existingIrsaliye = null)
    {
        _irsaliyeService = irsaliyeService;
        _cariHesapService = cariHesapService;
        _urunService = urunService;
        _kapTipiService = kapTipiService;
        _closeCallback = closeCallback;
        _existingIrsaliye = existingIrsaliye;

        if (existingIrsaliye != null)
        {
            WindowTitle = "İrsaliye Düzenle";
            IrsaliyeNo = existingIrsaliye.IrsaliyeNo;
            Tarih = new DateTimeOffset(existingIrsaliye.Tarih);
            Aciklama = existingIrsaliye.Aciklama ?? string.Empty;
            
            // Kalemleri yükle
            foreach (var kalem in existingIrsaliye.Kalemler)
            {
                Kalemler.Add(GirisKalem.FromEntity(kalem));
            }
        }
        else
        {
            // Yeni kayıt için boş satır ekle
            Kalemler.Add(new GirisKalem());
        }

        // Collection değişikliklerini dinle
        Kalemler.CollectionChanged += (s, e) => RefreshTotals();

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            // Müstahsilleri yükle
            var mustahsiller = await _cariHesapService.GetByCariTipiAsync(CariTipi.Mustahsil);
            Mustahsiller = new ObservableCollection<CariHesap>(mustahsiller);

            // Komisyoncuları yükle (Hal Kayıt için)
            var tumCariler = await _cariHesapService.GetAllAsync();
            Komisyoncular = new ObservableCollection<CariHesap>(
                tumCariler.Where(c => c.CariTipi == CariTipi.Komisyoncu ||
                                      c.CariTipiDetay == CariTipiDetay.Kabzimal ||
                                      c.CariTipiDetay == CariTipiDetay.HalIciKomisyoncu));

            var urunler = await _urunService.GetActiveAsync();
            Urunler = new ObservableCollection<Urun>(urunler);

            var kapTipleri = await _kapTipiService.GetActiveAsync();
            KapTipleri = new ObservableCollection<KapTipi>(kapTipleri);

            if (_existingIrsaliye != null)
            {
                SelectedMustahsil = Mustahsiller.FirstOrDefault(m => m.Id == _existingIrsaliye.MustahsilId);
                
                // Kalemlerdeki komisyoncuları ata (Include ile yüklenmediyse)
                foreach (var kalem in Kalemler)
                {
                    if (kalem.Komisyoncu == null)
                    {
                        var entityKalem = _existingIrsaliye.Kalemler.FirstOrDefault(k => k.Id == kalem.Id);
                        if (entityKalem?.KomisyoncuId != null)
                        {
                            kalem.Komisyoncu = Komisyoncular.FirstOrDefault(k => k.Id == entityKalem.KomisyoncuId);
                        }
                    }
                }
            }
            else
            {
                IrsaliyeNo = await _irsaliyeService.GenerateNewIrsaliyeNoAsync();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
        }
    }

    private void RefreshTotals()
    {
        OnPropertyChanged(nameof(ToplamKapAdet));
        OnPropertyChanged(nameof(ToplamDaraliKg));
        OnPropertyChanged(nameof(ToplamDara));
        OnPropertyChanged(nameof(ToplamNet));
        OnPropertyChanged(nameof(ToplamTutar));
    }

    [RelayCommand]
    private void YeniSatir()
    {
        Kalemler.Add(new GirisKalem());
        RefreshTotals();
    }

    [RelayCommand]
    private void KopyalaKalem(GirisKalem kalem)
    {
        var index = Kalemler.IndexOf(kalem);
        if (index >= 0)
        {
            Kalemler.Insert(index + 1, kalem.Clone());
            RefreshTotals();
        }
    }

    [RelayCommand]
    private void SilKalem(GirisKalem kalem)
    {
        Kalemler.Remove(kalem);
        RefreshTotals();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            // Boş satırları filtrele
            var gecerliKalemler = Kalemler.Where(k => k.Urun != null && k.DaraliKg > 0).ToList();
            
            if (!gecerliKalemler.Any())
            {
                StatusMessage = "⚠️ En az bir geçerli kalem girmelisiniz!";
                return;
            }

            // İlk komisyoncuyu MustahsilId olarak kullan (Hal Kayıt mantığı)
            var ilkKomisyoncu = gecerliKalemler.FirstOrDefault(k => k.Komisyoncu != null)?.Komisyoncu;
            if (ilkKomisyoncu == null)
            {
                // Komisyoncu yoksa mevcut mustahsili koru veya hata ver
                if (_existingIrsaliye?.MustahsilId == null && SelectedMustahsil == null)
                {
                    StatusMessage = "⚠️ En az bir kalemde komisyoncu seçmelisiniz!";
                    return;
                }
            }

            // Komisyoncu açıklaması
            var komisyoncuAciklama = string.Join(", ", 
                gecerliKalemler.Where(k => k.Komisyoncu != null)
                               .Select(k => k.Komisyoncu!.Unvan)
                               .Distinct());

            if (_existingIrsaliye == null)
            {
                // Yeni kayıt
                var irsaliye = new GirisIrsaliyesi
                {
                    IrsaliyeNo = IrsaliyeNo,
                    Tarih = Tarih?.DateTime ?? DateTime.Today,
                    MustahsilId = ilkKomisyoncu?.Id ?? SelectedMustahsil!.Id,
                    Aciklama = string.IsNullOrEmpty(Aciklama) 
                        ? $"Komisyoncular: {komisyoncuAciklama}"
                        : $"Komisyoncular: {komisyoncuAciklama}. {Aciklama}",
                    ToplamBrut = ToplamDaraliKg,
                    ToplamDara = ToplamDara,
                    ToplamNet = ToplamNet,
                    ToplamKapAdet = (int)ToplamKapAdet,
                    Kalemler = gecerliKalemler.Select(k => k.ToEntity()).ToList()
                };
                await _irsaliyeService.CreateAsync(irsaliye);
                StatusMessage = "✅ İrsaliye kaydedildi!";
            }
            else
            {
                // Güncelleme - mevcut mustahsil koruyoruz veya ilk komisyoncu
                _existingIrsaliye.Tarih = Tarih?.DateTime ?? DateTime.Today;
                if (ilkKomisyoncu != null)
                {
                    _existingIrsaliye.MustahsilId = ilkKomisyoncu.Id;
                }
                _existingIrsaliye.Aciklama = string.IsNullOrEmpty(Aciklama) 
                    ? $"Komisyoncular: {komisyoncuAciklama}"
                    : $"Komisyoncular: {komisyoncuAciklama}. {Aciklama}";
                _existingIrsaliye.ToplamBrut = ToplamDaraliKg;
                _existingIrsaliye.ToplamDara = ToplamDara;
                _existingIrsaliye.ToplamNet = ToplamNet;
                _existingIrsaliye.ToplamKapAdet = (int)ToplamKapAdet;
                
                // Mevcut kalemleri temizle ve yenilerini ekle
                _existingIrsaliye.Kalemler.Clear();
                foreach (var kalem in gecerliKalemler.Select(k => k.ToEntity()))
                {
                    kalem.IrsaliyeId = _existingIrsaliye.Id;
                    _existingIrsaliye.Kalemler.Add(kalem);
                }
                
                await _irsaliyeService.UpdateAsync(_existingIrsaliye);
                StatusMessage = "✅ İrsaliye güncellendi!";
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
    private void Cancel()
    {
        _closeCallback?.Invoke(false);
        CloseRequested?.Invoke(this, false);
    }
}
