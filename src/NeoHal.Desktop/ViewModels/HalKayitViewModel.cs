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
/// Hal Kayıt - Komisyoncudan mal alış kaydı
/// İş Akışı:
/// 1. Komisyoncu seç (halden mal aldığın kişi)
/// 2. O komisyoncudan aldığın malları gir
/// 3. Kaydet → Stoka girer + Komisyoncuya borç yazılır
/// </summary>
public partial class HalKayitViewModel : ViewModelBase
{
    private readonly IGirisIrsaliyesiService _irsaliyeService;
    private readonly ICariHesapService _cariService;
    private readonly IUrunService _urunService;
    private readonly IKapTipiService _kapTipiService;

    // Mevcut irsaliye takibi (düzenleme/taslak için)
    private Guid? _mevcutIrsaliyeId = null;

    [ObservableProperty]
    private ObservableCollection<HalKayitKalem> _kalemler = new();

    [ObservableProperty]
    private HalKayitKalem? _selectedKalem;

    [ObservableProperty]
    private ObservableCollection<CariHesap> _komisyoncular = new();

    [ObservableProperty]
    private ObservableCollection<Urun> _urunler = new();

    [ObservableProperty]
    private ObservableCollection<KapTipi> _kapTipleri = new();

    [ObservableProperty]
    private DateTimeOffset? _kayitTarihi = DateTimeOffset.Now;

    [ObservableProperty]
    private string _irsaliyeNo = string.Empty;

    [ObservableProperty]
    private string _aciklama = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Hazır";

    // Hesaplanan Değerler
    public int ToplamKap => Kalemler.Sum(k => k.KapAdet);
    public decimal ToplamKg => Kalemler.Sum(k => k.NetKg);
    public decimal ToplamTutar => Kalemler.Sum(k => k.Tutar);
    
    // Komisyoncu bazlı özet
    public int ToplamKomisyoncu => Kalemler.Where(k => k.Komisyoncu != null).Select(k => k.Komisyoncu!.Id).Distinct().Count();

    public HalKayitViewModel(
        IGirisIrsaliyesiService irsaliyeService,
        ICariHesapService cariService,
        IUrunService urunService,
        IKapTipiService kapTipiService)
    {
        _irsaliyeService = irsaliyeService;
        _cariService = cariService;
        _urunService = urunService;
        _kapTipiService = kapTipiService;

        AddEmptyRow();
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            StatusMessage = "Veriler yükleniyor...";
            
            var tumCariler = await _cariService.GetAllAsync();
            
            // Komisyoncuları yükle
            Komisyoncular = new ObservableCollection<CariHesap>(
                tumCariler.Where(c => c.CariTipi == CariTipi.Komisyoncu ||
                                      c.CariTipiDetay == CariTipiDetay.Kabzimal ||
                                      c.CariTipiDetay == CariTipiDetay.HalIciKomisyoncu));

            var urunler = await _urunService.GetAllAsync();
            Urunler = new ObservableCollection<Urun>(urunler);

            var kapTipleri = await _kapTipiService.GetAllAsync();
            KapTipleri = new ObservableCollection<KapTipi>(kapTipleri);

            IrsaliyeNo = GenerateNewIrsaliyeNo();
            
            // Taslak irsaliyeleri yükle
            await LoadTaslaklarAsync();
            
            StatusMessage = $"Hazır - {Komisyoncular.Count} komisyoncu, {Urunler.Count} ürün";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Hata: {ex.Message}";
        }
    }

    // Taslak irsaliyeler için
    [ObservableProperty]
    private ObservableCollection<GirisIrsaliyesi> _taslakIrsaliyeler = new();

    [ObservableProperty]
    private bool _taslakPanelAcik = false;

    private async Task LoadTaslaklarAsync()
    {
        try
        {
            var bugunBaslangic = DateTime.Today.AddDays(-7); // Son 7 gün
            var bugün = DateTime.Today.AddDays(1);
            var irsaliyeler = await _irsaliyeService.GetByDateRangeAsync(bugunBaslangic, bugün);
            var taslaklar = irsaliyeler.Where(i => i.Durum == BelgeDurumu.Taslak).ToList();
            TaslakIrsaliyeler = new ObservableCollection<GirisIrsaliyesi>(taslaklar);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Taslak yükleme hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private void TaslakPaneliAcKapat()
    {
        TaslakPanelAcik = !TaslakPanelAcik;
    }

    [RelayCommand]
    private async Task TaslakYukleAsync(GirisIrsaliyesi? irsaliye)
    {
        if (irsaliye == null) return;
        
        try
        {
            StatusMessage = "Taslak yükleniyor...";
            
            // Kalemlerle birlikte yükle
            var irsaliyeDetay = await _irsaliyeService.GetByIdWithKalemlerAsync(irsaliye.Id);
            if (irsaliyeDetay == null)
            {
                StatusMessage = "⚠️ Taslak bulunamadı!";
                return;
            }

            // İrsaliye bilgilerini doldur
            _mevcutIrsaliyeId = irsaliyeDetay.Id;
            IrsaliyeNo = irsaliyeDetay.IrsaliyeNo;
            KayitTarihi = new DateTimeOffset(irsaliyeDetay.Tarih);
            Aciklama = irsaliyeDetay.Aciklama ?? string.Empty;

            // Kalemleri doldur
            Kalemler.Clear();
            foreach (var kalem in irsaliyeDetay.Kalemler)
            {
                var halKalem = new HalKayitKalem
                {
                    Komisyoncu = Komisyoncular.FirstOrDefault(k => k.Id == kalem.KomisyoncuId),
                    Urun = Urunler.FirstOrDefault(u => u.Id == kalem.UrunId),
                    KapTipi = KapTipleri.FirstOrDefault(k => k.Id == kalem.KapTipiId),
                    KapAdet = kalem.KapAdet,
                    DaraliKg = kalem.BrutKg,
                    BirimFiyat = kalem.BirimFiyat ?? 0
                };
                halKalem.PropertyChanged += (s, e) => RefreshTotals();
                Kalemler.Add(halKalem);
            }
            
            // En az bir boş satır ekle
            if (!Kalemler.Any())
            {
                AddEmptyRow();
            }



            TaslakPanelAcik = false;
            RefreshTotals();
            StatusMessage = $"✅ Taslak yüklendi: {IrsaliyeNo} | Onaylamak için F6'ya bas";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Taslak yükleme hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task TaslakSilAsync(GirisIrsaliyesi? irsaliye)
    {
        if (irsaliye == null) return;
        
        try
        {
            await _irsaliyeService.DeleteAsync(irsaliye.Id);
            TaslakIrsaliyeler.Remove(irsaliye);
            StatusMessage = $"🗑 Taslak silindi: {irsaliye.IrsaliyeNo}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Silme hatası: {ex.Message}";
        }
    }

    private string GenerateNewIrsaliyeNo()
    {
        return $"HAL{DateTime.Now:yyyyMMddHHmm}";
    }

    private void AddEmptyRow()
    {
        var yeniKalem = new HalKayitKalem();
        yeniKalem.PropertyChanged += (s, e) =>
        {
            // Herhangi bir değişiklikte toplamları güncelle
            // NetKg ve Tutar artık computed property oldukları için otomatik hesaplanıyor
            RefreshTotals();
        };
        Kalemler.Add(yeniKalem);
    }

    private void RefreshTotals()
    {
        OnPropertyChanged(nameof(ToplamKap));
        OnPropertyChanged(nameof(ToplamKg));
        OnPropertyChanged(nameof(ToplamTutar));
        OnPropertyChanged(nameof(ToplamKomisyoncu));
    }

    [RelayCommand]
    private void Yeni()
    {
        Kalemler.Clear();
        AddEmptyRow();
        Aciklama = string.Empty;
        IrsaliyeNo = GenerateNewIrsaliyeNo();
        _mevcutIrsaliyeId = null;
        StatusMessage = "✨ Yeni kayıt hazır.";
    }

    [RelayCommand]
    private void YeniSatir()
    {
        AddEmptyRow();
        SelectedKalem = Kalemler.Last();
    }

    [RelayCommand]
    private void KopyalaKalem(HalKayitKalem? kalem)
    {
        if (kalem == null) return;
        
        // Satırın kopyasını oluştur - komisyoncu dahil
        var yeniKalem = new HalKayitKalem
        {
            Komisyoncu = kalem.Komisyoncu, // Komisyoncuyu da kopyala
            Urun = kalem.Urun,
            KapTipi = kalem.KapTipi,
            KapAdet = kalem.KapAdet,
            DaraliKg = kalem.DaraliKg,
            BirimFiyat = kalem.BirimFiyat
        };
        
        // Mevcut satırın altına ekle
        var index = Kalemler.IndexOf(kalem);
        if (index >= 0)
        {
            Kalemler.Insert(index + 1, yeniKalem);
        }
        else
        {
            Kalemler.Add(yeniKalem);
        }
        
        SelectedKalem = yeniKalem;
        RefreshTotals();
        StatusMessage = "✅ Satır kopyalandı";
    }

    [RelayCommand]
    private void SilKalem(HalKayitKalem? kalem)
    {
        if (kalem != null)
        {
            // Önce seçimi kaldır (DataGrid senkronizasyon hatası önleme)
            if (SelectedKalem == kalem)
            {
                SelectedKalem = null;
            }
            
            Kalemler.Remove(kalem);
            RefreshTotals();
        }
        
        if (Kalemler.Count == 0)
            AddEmptyRow();
    }

    [RelayCommand]
    private async Task KaydetAsync()
    {
        try
        {
            // Komisyoncusu olan geçerli kalemleri al
            var gecerliKalemler = Kalemler.Where(k => k.Komisyoncu != null && k.Urun != null && k.NetKg > 0).ToList();
            
            if (!gecerliKalemler.Any())
            {
                StatusMessage = "⚠️ En az bir geçerli kalem girmelisiniz! (Komisyoncu + Ürün seçili olmalı)";
                return;
            }

            StatusMessage = "Kaydediliyor...";

            var kayitTarihi = KayitTarihi?.DateTime ?? DateTime.Today;
            
            // Yeni kalemleri oluştur - KOMİSYONCU BİLGİSİ DAHİL
            var yeniKalemler = gecerliKalemler.Select(k => new GirisIrsaliyesiKalem
            {
                Id = Guid.NewGuid(),
                KomisyoncuId = k.Komisyoncu!.Id,
                UrunId = k.Urun!.Id,
                KapTipiId = k.KapTipi?.Id ?? Guid.Empty,
                KapAdet = k.KapAdet,
                BrutKg = k.DaraliKg,
                DaraKg = k.KapAdet * (k.KapTipi?.DaraAgirlik ?? 0),
                NetKg = k.NetKg,
                BirimFiyat = k.BirimFiyat,
                KalanKapAdet = k.KapAdet,
                KalanKg = k.NetKg
            }).ToList();

            // Komisyoncu listesini açıklamaya ekle
            var komisyoncular = gecerliKalemler
                .Select(k => k.Komisyoncu!.Unvan)
                .Distinct()
                .ToList();
            var komisyoncuAciklama = string.Join(", ", komisyoncular);

            if (_mevcutIrsaliyeId.HasValue)
            {
                // TASLAK GÜNCELLEME - mevcut taslağı düzenliyoruz
                var mevcutIrsaliye = await _irsaliyeService.GetByIdWithKalemlerAsync(_mevcutIrsaliyeId.Value);
                if (mevcutIrsaliye != null)
                {
                    var anaKomisyoncu = gecerliKalemler.First().Komisyoncu!;
                    mevcutIrsaliye.Tarih = kayitTarihi;
                    mevcutIrsaliye.MustahsilId = anaKomisyoncu.Id;
                    mevcutIrsaliye.Aciklama = $"Hal alışı. Komisyoncular: {komisyoncuAciklama}. {Aciklama}".Trim();
                    mevcutIrsaliye.ToplamNet = yeniKalemler.Sum(k => k.NetKg);
                    mevcutIrsaliye.ToplamKapAdet = yeniKalemler.Sum(k => k.KapAdet);
                    mevcutIrsaliye.ToplamBrut = yeniKalemler.Sum(k => k.BrutKg);
                    mevcutIrsaliye.ToplamDara = yeniKalemler.Sum(k => k.DaraKg);
                    mevcutIrsaliye.Kalemler = yeniKalemler;
                    
                    await _irsaliyeService.UpdateAsync(mevcutIrsaliye);
                    StatusMessage = $"✅ Taslak güncellendi: {IrsaliyeNo}";
                }
            }
            else
            {
                // Aynı güne ait mevcut irsaliye var mı kontrol et
                var mevcutIrsaliye = await _irsaliyeService.GetByTarihAsync(kayitTarihi);
                
                if (mevcutIrsaliye != null)
                {
                    // MEVCUT İRSALİYEYE EKLE - aynı güne ait tüm alımlar tek irsaliyede
                    await _irsaliyeService.AddKalemlerAsync(mevcutIrsaliye.Id, yeniKalemler);
                    _mevcutIrsaliyeId = mevcutIrsaliye.Id;
                    
                    var toplamKalem = mevcutIrsaliye.Kalemler.Count + yeniKalemler.Count;
                    StatusMessage = $"✅ {yeniKalemler.Count} kalem eklendi! İrsaliye: {mevcutIrsaliye.IrsaliyeNo} - Toplam: {toplamKalem} kalem";
                }
                else
                {
                    // İlk komisyoncuyu ana tedarikçi olarak kullan
                    var anaKomisyoncu = gecerliKalemler.First().Komisyoncu!;

                    // YENİ İRSALİYE OLUŞTUR - günün ilk kaydı
                    var irsaliye = new GirisIrsaliyesi
                    {
                        Id = Guid.NewGuid(),
                        IrsaliyeNo = IrsaliyeNo,
                        Tarih = kayitTarihi,
                        OlusturmaTarihi = DateTime.Now,
                        MustahsilId = anaKomisyoncu.Id,
                        Aciklama = $"Hal alışı. Komisyoncular: {komisyoncuAciklama}. {Aciklama}".Trim(),
                        Durum = BelgeDurumu.Taslak,
                        ToplamNet = yeniKalemler.Sum(k => k.NetKg),
                        ToplamKapAdet = yeniKalemler.Sum(k => k.KapAdet),
                        ToplamBrut = yeniKalemler.Sum(k => k.BrutKg),
                        ToplamDara = yeniKalemler.Sum(k => k.DaraKg),
                        Kalemler = yeniKalemler
                    };

                    await _irsaliyeService.CreateAsync(irsaliye);
                    _mevcutIrsaliyeId = irsaliye.Id;
                    
                    StatusMessage = $"✅ Taslak kaydedildi! {gecerliKalemler.Count} kalem, {ToplamKg:N2} kg - {ToplamTutar:N2} ₺ | Onaylamak için F6'ya bas";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Kayıt hatası: {ex.Message}";
        }
    }

    /// <summary>
    /// İrsaliyeyi onayla - Durum = Onaylandi
    /// </summary>
    [RelayCommand]
    private async Task OnaytaAsync()
    {
        try
        {
            if (!_mevcutIrsaliyeId.HasValue)
            {
                StatusMessage = "⚠️ Onaylanacak taslak bulunamadı! Önce F5 ile kaydet.";
                return;
            }

            // Kalemler kontrol et
            var gecerliKalemler = Kalemler.Where(k => k.Komisyoncu != null && k.Urun != null && k.NetKg > 0).ToList();
            if (!gecerliKalemler.Any())
            {
                StatusMessage = "⚠️ En az bir geçerli kalem olmalı! (Komisyoncu + Ürün seçili, Net Kg > 0)";
                return;
            }

            StatusMessage = "Onaylanıyor...";

            // Mevcut irsaliyeyi al ve onayla
            var irsaliye = await _irsaliyeService.GetByIdWithKalemlerAsync(_mevcutIrsaliyeId.Value);
            if (irsaliye == null)
            {
                StatusMessage = "❌ İrsaliye bulunamadı!";
                return;
            }

            // Durum = Onaylandi
            irsaliye.Durum = BelgeDurumu.Onaylandi;
            await _irsaliyeService.UpdateAsync(irsaliye);

            StatusMessage = $"✅ İrsaliye ONAYLANDI! 🎉 {irsaliye.IrsaliyeNo}";
            
            // Taslaklar listesini yenile
            await LoadTaslaklarAsync();
            
            // Yeni bir taslak için temizle - Yeni() komutunu çalıştır
            Yeni();
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Onaylama hatası: {ex.Message}";
        }
    }
}

/// <summary>
/// Hal kayıt kalemi - komisyoncudan alınan mal
/// </summary>
public partial class HalKayitKalem : ObservableObject
{
    [ObservableProperty]
    private CariHesap? _komisyoncu;

    public string KomisyoncuAdi => Komisyoncu?.Unvan ?? string.Empty;
    public string KomisyoncuKodu => Komisyoncu?.Kod ?? string.Empty;

    [ObservableProperty]
    private Urun? _urun;

    public string UrunAdi => Urun?.Ad ?? string.Empty;

    [ObservableProperty]
    private KapTipi? _kapTipi;

    public string KapTipiAdi => KapTipi?.Ad ?? string.Empty;

    [ObservableProperty]
    private int _kapAdet = 1;

    [ObservableProperty]
    private decimal _daraliKg; // Kasayla birlikte tartılan toplam ağırlık

    // Net Kg = Daralı Kg - (Kap Adedi × Kap Ağırlığı)
    public decimal NetKg => DaraliKg - (KapAdet * (KapTipi?.DaraAgirlik ?? 0));

    [ObservableProperty]
    private decimal _birimFiyat;

    // Tutar = Net Kg × Birim Fiyat
    public decimal Tutar => NetKg * BirimFiyat;
    
    partial void OnKomisyoncuChanged(CariHesap? value)
    {
        OnPropertyChanged(nameof(KomisyoncuAdi));
        OnPropertyChanged(nameof(KomisyoncuKodu));
    }
    partial void OnUrunChanged(Urun? value) => OnPropertyChanged(nameof(UrunAdi));
    partial void OnKapTipiChanged(KapTipi? value)
    {
        OnPropertyChanged(nameof(KapTipiAdi));
        OnPropertyChanged(nameof(NetKg));
        OnPropertyChanged(nameof(Tutar));
    }
    partial void OnKapAdetChanged(int value)
    {
        OnPropertyChanged(nameof(NetKg));
        OnPropertyChanged(nameof(Tutar));
    }
    partial void OnDaraliKgChanged(decimal value)
    {
        OnPropertyChanged(nameof(NetKg));
        OnPropertyChanged(nameof(Tutar));
    }
    partial void OnBirimFiyatChanged(decimal value) => OnPropertyChanged(nameof(Tutar));
}
