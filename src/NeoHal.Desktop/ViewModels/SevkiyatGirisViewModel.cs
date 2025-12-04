using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeoHal.Core.Entities;
using NeoHal.Core.Enums;
using NeoHal.Services.Interfaces;

namespace NeoHal.Desktop.ViewModels;

/// <summary>
/// Sevkiyatçı için Excel tarzı hızlı mal giriş/fatura oluşturma
/// İş Akışı:
/// 1. Sabah halden mal alınır (farklı komisyonculardan)
/// 2. Dükkana gelinir, işçiler tartar, kağıda yazar
/// 3. Gün sonunda katip tüm verileri girer
/// 4. Şubeye fatura kesilir (masraflar eklenir)
/// KESİNTİ UYGULANMAZ - Komisyoncudan fatura alınıyor
/// </summary>
public partial class SevkiyatGirisViewModel : ViewModelBase
{
    private readonly ISatisFaturasiService _faturaService;
    private readonly ICariHesapService _cariService;
    private readonly IUrunService _urunService;
    private readonly IKapTipiService _kapTipiService;

    // Mevcut fatura takibi (düzenleme/taslak için)
    private Guid? _mevcutFaturaId = null;
    private bool _faturaKaydedildi = false;

    [ObservableProperty]
    private ObservableCollection<SevkiyatKalem> _kalemler = new();

    [ObservableProperty]
    private SevkiyatKalem? _selectedKalem;

    [ObservableProperty]
    private ObservableCollection<CariHesap> _subeler = new(); // Şubeler (alıcılar)

    [ObservableProperty]
    private ObservableCollection<CariHesap> _komisyoncular = new(); // Halden mal aldığımız yerler

    [ObservableProperty]
    private ObservableCollection<Urun> _urunler = new();

    [ObservableProperty]
    private ObservableCollection<KapTipi> _kapTipleri = new();

    [ObservableProperty]
    private CariHesap? _selectedSube;

    [ObservableProperty]
    private DateTimeOffset? _faturaTarihi = DateTimeOffset.Now;

    [ObservableProperty]
    private string _faturaNo = string.Empty;

    [ObservableProperty]
    private decimal _ekMasraflar = 0;

    [ObservableProperty]
    private string _masrafAciklamasi = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Hazır";

    // Şube seçildi mi kontrolü
    public bool SubeSecildi => SelectedSube != null;
    public string SubeDurumMesaji => SelectedSube != null 
        ? $"✅ {SelectedSube.Unvan}" 
        : "⚠️ Lütfen şube seçin!";
    public string SubeDurumRenk => SelectedSube != null ? "#4CAF50" : "#F44336";

    partial void OnSelectedSubeChanged(CariHesap? value)
    {
        OnPropertyChanged(nameof(SubeSecildi));
        OnPropertyChanged(nameof(SubeDurumMesaji));
        OnPropertyChanged(nameof(SubeDurumRenk));
        
        if (value != null)
        {
            StatusMessage = $"Şube: {value.Unvan} seçildi";
        }
    }

    // Hesaplanan Değerler - KESİNTİ YOK!
    public int ToplamKap => Kalemler.Sum(k => k.KapAdet);
    public decimal ToplamKg => Kalemler.Sum(k => k.NetKg);
    public decimal AraToplam => Kalemler.Sum(k => k.Tutar);
    public decimal GenelToplam => AraToplam + EkMasraflar;

    public SevkiyatGirisViewModel(
        ISatisFaturasiService faturaService,
        ICariHesapService cariService,
        IUrunService urunService,
        IKapTipiService kapTipiService)
    {
        _faturaService = faturaService;
        _cariService = cariService;
        _urunService = urunService;
        _kapTipiService = kapTipiService;

        // Başlangıçta boş bir satır ekle
        AddEmptyRow();

        _ = LoadDataAsync();
    }

    partial void OnEkMasraflarChanged(decimal value)
    {
        OnPropertyChanged(nameof(GenelToplam));
    }

    private async Task LoadDataAsync()
    {
        try
        {
            StatusMessage = "Veriler yükleniyor...";
            
            // Tüm carileri yükle
            var tumCariler = await _cariService.GetAllAsync();
            
            // Şubeleri yükle (Sube tipi + Sevkiyatçı + Alıcı + Tüccar - mal gönderdiğimiz yerler)
            Subeler = new ObservableCollection<CariHesap>(
                tumCariler.Where(c => c.CariTipi == CariTipi.Sube ||           // Kendi şubelerimiz (iç transfer)
                                      c.CariTipi == CariTipi.Alici ||           // Alıcılar (dış satış)
                                      c.CariTipiDetay == CariTipiDetay.Tuccar ||
                                      c.CariTipiDetay == CariTipiDetay.MarketZinciri ||
                                      c.CariTipiDetay == CariTipiDetay.ManavDukkan));
            
            // Komisyoncuları yükle (Halden mal aldığımız yerler)
            Komisyoncular = new ObservableCollection<CariHesap>(
                tumCariler.Where(c => c.CariTipi == CariTipi.Komisyoncu ||
                                      c.CariTipiDetay == CariTipiDetay.Kabzimal ||
                                      c.CariTipiDetay == CariTipiDetay.HalIciKomisyoncu));

            // Ürünleri yükle
            var urunler = await _urunService.GetAllAsync();
            Urunler = new ObservableCollection<Urun>(urunler);

            // Kap tiplerini yükle
            var kapTipleri = await _kapTipiService.GetAllAsync();
            KapTipleri = new ObservableCollection<KapTipi>(kapTipleri);

            // Yeni fatura no oluştur
            FaturaNo = GenerateNewFaturaNo();
            
            // Taslak faturaları yükle
            await LoadTaslaklarAsync();
            
            StatusMessage = $"Hazır - {Subeler.Count} şube/alıcı, {Komisyoncular.Count} komisyoncu, {Urunler.Count} ürün";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Yükleme hatası: {ex.Message}";
        }
    }

    // Taslak faturalar için
    [ObservableProperty]
    private ObservableCollection<SatisFaturasi> _taslakFaturalar = new();

    [ObservableProperty]
    private bool _taslakPanelAcik = false;

    private async Task LoadTaslaklarAsync()
    {
        try
        {
            var tumFaturalar = await _faturaService.GetByFaturaTipiAsync(FaturaTipi.Sevkiyat);
            var taslaklar = tumFaturalar.Where(f => f.Durum == BelgeDurumu.Taslak).ToList();
            TaslakFaturalar = new ObservableCollection<SatisFaturasi>(taslaklar);
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
    private async Task TaslakYukleAsync(SatisFaturasi? fatura)
    {
        if (fatura == null) return;
        
        try
        {
            StatusMessage = "Taslak yükleniyor...";
            
            // Kalemlerle birlikte yükle
            var faturaDetay = await _faturaService.GetByIdWithKalemlerAsync(fatura.Id);
            if (faturaDetay == null)
            {
                StatusMessage = "⚠️ Taslak bulunamadı!";
                return;
            }

            // Fatura bilgilerini doldur
            _mevcutFaturaId = faturaDetay.Id;
            FaturaNo = faturaDetay.FaturaNo;
            FaturaTarihi = new DateTimeOffset(faturaDetay.FaturaTarihi);
            SelectedSube = Subeler.FirstOrDefault(s => s.Id == faturaDetay.AliciId);
            EkMasraflar = faturaDetay.EkMasrafTutari;
            MasrafAciklamasi = faturaDetay.EkMasrafAciklama ?? string.Empty;

            // Kalemleri doldur
            Kalemler.Clear();
            foreach (var kalem in faturaDetay.Kalemler)
            {
                var sevkiyatKalem = new SevkiyatKalem
                {
                    Urun = Urunler.FirstOrDefault(u => u.Id == kalem.UrunId),
                    KapTipi = KapTipleri.FirstOrDefault(k => k.Id == kalem.KapTipiId),
                    Komisyoncu = Komisyoncular.FirstOrDefault(c => c.Id == kalem.KomisyoncuId),
                    KapAdet = kalem.KapAdet,
                    DaraliKg = kalem.BrutKg,
                    BirimFiyat = kalem.BirimFiyat,
                    AlisFiyati = kalem.AlisFiyati
                };
                sevkiyatKalem.PropertyChanged += (s, e) => RefreshTotals();
                Kalemler.Add(sevkiyatKalem);
            }
            
            // En az bir boş satır ekle
            if (!Kalemler.Any())
            {
                AddEmptyRow();
            }

            _faturaKaydedildi = true;
            TaslakPanelAcik = false;
            RefreshTotals();
            StatusMessage = $"✅ Taslak yüklendi: {FaturaNo}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Taslak yükleme hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task TaslakSilAsync(SatisFaturasi? fatura)
    {
        if (fatura == null) return;
        
        try
        {
            await _faturaService.DeleteAsync(fatura.Id);
            TaslakFaturalar.Remove(fatura);
            StatusMessage = $"🗑 Taslak silindi: {fatura.FaturaNo}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Silme hatası: {ex.Message}";
        }
    }

    private string GenerateNewFaturaNo()
    {
        // SEV + Yıl + Ay + Gün + Saat + Dakika
        return $"SEV{DateTime.Now:yyyyMMddHHmm}";
    }

    private void AddEmptyRow()
    {
        var yeniKalem = new SevkiyatKalem();
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
        OnPropertyChanged(nameof(AraToplam));
        OnPropertyChanged(nameof(GenelToplam));
    }

    [RelayCommand]
    private void Yeni()
    {
        Kalemler.Clear();
        AddEmptyRow();
        EkMasraflar = 0;
        MasrafAciklamasi = string.Empty;
        SelectedSube = null;
        FaturaNo = GenerateNewFaturaNo();
        _mevcutFaturaId = null;
        _faturaKaydedildi = false;
        StatusMessage = "✨ Yeni fatura hazır.";
    }

    [RelayCommand]
    private void YeniSatir()
    {
        AddEmptyRow();
        SelectedKalem = Kalemler.Last();
    }

    [RelayCommand]
    private void KopyalaKalem(SevkiyatKalem? kalem)
    {
        if (kalem == null) return;
        
        // Satırın kopyasını oluştur
        var yeniKalem = new SevkiyatKalem
        {
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
    private void SilKalem(SevkiyatKalem? kalem)
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
        
        // En az 1 satır kalsın
        if (Kalemler.Count == 0)
            AddEmptyRow();
    }

    [RelayCommand]
    private async Task KaydetAsync()
    {
        try
        {
            if (SelectedSube == null)
            {
                StatusMessage = "⚠️ Lütfen şube seçin!";
                return;
            }

            var gecerliKalemler = Kalemler.Where(k => k.Urun != null && k.NetKg > 0).ToList();
            if (!gecerliKalemler.Any())
            {
                StatusMessage = "⚠️ En az bir geçerli kalem girmelisiniz!";
                return;
            }

            StatusMessage = "Kaydediliyor...";

            var faturaKalemleri = gecerliKalemler.Select(k => new SatisFaturasiKalem
            {
                Id = Guid.NewGuid(),
                UrunId = k.Urun!.Id,
                KapTipiId = k.KapTipi?.Id ?? Guid.Empty,
                KomisyoncuId = k.Komisyoncu?.Id,
                AlisFiyati = k.AlisFiyati,
                KapAdet = k.KapAdet,
                BrutKg = k.DaraliKg,
                DaraKg = k.KapAdet * (k.KapTipi?.DaraAgirlik ?? 0),
                NetKg = k.NetKg,
                BirimFiyat = k.BirimFiyat,
                Tutar = k.Tutar,
                RusumOrani = 0,
                KomisyonOrani = 0,
                StopajOrani = 0,
                RusumTutari = 0,
                KomisyonTutari = 0,
                StopajTutari = 0
            }).ToList();

            if (_mevcutFaturaId.HasValue)
            {
                // GÜNCELLEME - Mevcut faturayı güncelle
                var mevcutFatura = await _faturaService.GetByIdAsync(_mevcutFaturaId.Value);
                if (mevcutFatura != null)
                {
                    mevcutFatura.FaturaTarihi = FaturaTarihi?.DateTime ?? DateTime.Today;
                    mevcutFatura.AliciId = SelectedSube.Id;
                    mevcutFatura.AraToplam = AraToplam;
                    mevcutFatura.EkMasrafTutari = EkMasraflar;
                    mevcutFatura.EkMasrafAciklama = MasrafAciklamasi;
                    mevcutFatura.GenelToplam = GenelToplam;
                    mevcutFatura.Aciklama = BuildAciklama();
                    mevcutFatura.Kalemler = faturaKalemleri;
                    
                    await _faturaService.UpdateAsync(mevcutFatura);
                    StatusMessage = $"✅ Fatura {FaturaNo} güncellendi! Toplam: {GenelToplam:N2} ₺";
                }
            }
            else
            {
                // YENİ KAYIT
                var fatura = new SatisFaturasi
                {
                    Id = Guid.NewGuid(),
                    FaturaNo = FaturaNo,
                    FaturaTarihi = FaturaTarihi?.DateTime ?? DateTime.Today,
                    AliciId = SelectedSube.Id,
                    FaturaTipi = FaturaTipi.Sevkiyat,
                    AraToplam = AraToplam,
                    KdvTutari = 0,
                    RusumTutari = 0,
                    KomisyonTutari = 0,
                    StopajTutari = 0,
                    EkMasrafTutari = EkMasraflar,
                    EkMasrafAciklama = MasrafAciklamasi,
                    GenelToplam = GenelToplam,
                    Aciklama = BuildAciklama(),
                    Durum = BelgeDurumu.Taslak, // Taslak olarak kaydet
                    Kalemler = faturaKalemleri
                };

                await _faturaService.CreateAsync(fatura);
                _mevcutFaturaId = fatura.Id; // ID'yi sakla
                StatusMessage = $"✅ Fatura {FaturaNo} taslak olarak kaydedildi! Toplam: {GenelToplam:N2} ₺";
            }
            
            _faturaKaydedildi = true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Kayıt hatası: {ex.Message}";
        }
    }
    
    // Direkt kaydetme - dialog olmadan, form temizleme ile
    [RelayCommand]
    private async Task KaydetVeTemizleAsync()
    {
        await KaydetAsync();
        await Task.Delay(1500);
        Yeni();
    }

    private string BuildAciklama()
    {
        var sb = new StringBuilder();
        
        // Komisyoncuları listele
        var komisyoncuGruplari = Kalemler
            .Where(k => k.Komisyoncu != null)
            .GroupBy(k => k.Komisyoncu!.Unvan)
            .Select(g => g.Key)
            .ToList();
        
        if (komisyoncuGruplari.Any())
        {
            sb.AppendLine($"Komisyoncular: {string.Join(", ", komisyoncuGruplari)}");
        }
        
        if (EkMasraflar > 0)
        {
            sb.AppendLine($"Masraf ({MasrafAciklamasi}): {EkMasraflar:N2} ₺");
        }
        
        return sb.ToString().Trim();
    }

    // Fatura kesme işlemi - kaydedip yazdırma seçeneği sunar
    [ObservableProperty]
    private bool _faturaKesDialogAcik = false;
    
    [ObservableProperty]
    private string _sonKesilenFaturaNo = string.Empty;

    [RelayCommand]
    private async Task FaturaKesAsync()
    {
        try
        {
            // Önce doğrulama
            if (SelectedSube == null)
            {
                StatusMessage = "⚠️ Lütfen şube seçin!";
                return;
            }

            var gecerliKalemler = Kalemler.Where(k => k.Urun != null && k.NetKg > 0).ToList();
            if (!gecerliKalemler.Any())
            {
                StatusMessage = "⚠️ En az bir geçerli kalem girmelisiniz!";
                return;
            }

            // Risk limiti kontrolü
            var toplamTutar = gecerliKalemler.Sum(k => k.Tutar);
            var riskDurumu = await _cariService.CheckRiskLimitiAsync(SelectedSube.Id, toplamTutar);
            
            if (riskDurumu.LimitAsildi)
            {
                StatusMessage = $"🚫 RİSK LİMİTİ AŞILDI! Mevcut bakiye: {riskDurumu.MevcutBakiye:N2} ₺, Limit: {riskDurumu.RiskLimiti:N2} ₺, Bu işlem: {toplamTutar:N2} ₺";
                return;
            }

            // Eğer henüz kaydedilmemişse veya değişiklik yapılmışsa kaydet
            if (!_faturaKaydedildi)
            {
                await KaydetAsync();
            }

            // Fatura durumunu "Onaylandı" yap
            if (_mevcutFaturaId.HasValue)
            {
                var fatura = await _faturaService.GetByIdAsync(_mevcutFaturaId.Value);
                if (fatura != null && fatura.Durum == BelgeDurumu.Taslak)
                {
                    fatura.Durum = BelgeDurumu.Onaylandi;
                    await _faturaService.UpdateAsync(fatura);
                }
            }
            
            // Dialog'u aç - kullanıcı yazdırma seçeneği seçsin
            SonKesilenFaturaNo = FaturaNo;
            FaturaKesDialogAcik = true;
            
            StatusMessage = $"✅ Fatura kesildi: {SonKesilenFaturaNo} - Yazdırmak için seçim yapın";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Fatura kesme hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task FaturaYazdirAsync()
    {
        FaturaKesDialogAcik = false;
        await YazdirVePrintAsync();
        Yeni(); // Yazdırdıktan sonra temizle
    }

    [RelayCommand]
    private async Task FaturaPdfOnizleAsync()
    {
        FaturaKesDialogAcik = false;
        await YazdirAsync(); // PDF önizleme (tarayıcıda açar)
        Yeni(); // Önizlemeden sonra temizle
    }

    [RelayCommand]
    private void FaturaDialogKapat()
    {
        FaturaKesDialogAcik = false;
        Yeni(); // Yeni fatura için hazırla
    }

    private async Task YazdirVePrintAsync()
    {
        try
        {
            StatusMessage = "Fatura yazıcıya gönderiliyor...";
            
            var html = GenerateFaturaHtml();
            var fileName = $"Sevkiyat_{SonKesilenFaturaNo}_{DateTime.Now:yyyyMMddHHmmss}.html";
            var tempPath = Path.Combine(Path.GetTempPath(), fileName);
            await File.WriteAllTextAsync(tempPath, html);
            
            // Print dialog ile yazdır
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo 
                { 
                    FileName = tempPath, 
                    UseShellExecute = true,
                    Verb = "print"
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // macOS'ta lpr komutu ile yazdır
                Process.Start("lpr", tempPath);
            }
            else
            {
                Process.Start("lpr", tempPath);
            }
            
            StatusMessage = $"🖨️ Fatura yazıcıya gönderildi: {SonKesilenFaturaNo}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Yazdırma hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task YazdirAsync()
    {
        try
        {
            if (SelectedSube == null || !Kalemler.Any(k => k.Urun != null))
            {
                StatusMessage = "⚠️ Yazdırmak için şube seçin ve en az bir kalem girin!";
                return;
            }
            
            StatusMessage = "Fatura hazırlanıyor...";
            
            var html = GenerateFaturaHtml();
            
            // Geçici dosyaya kaydet ve aç
            var fileName = $"Sevkiyat_{FaturaNo}_{DateTime.Now:yyyyMMddHHmmss}.html";
            var tempPath = Path.Combine(Path.GetTempPath(), fileName);
            await File.WriteAllTextAsync(tempPath, html);
            
            // Platformla göre tarayıcıda aç
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo { FileName = tempPath, UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", tempPath);
            }
            else
            {
                Process.Start("xdg-open", tempPath);
            }
            
            StatusMessage = "🖨️ Fatura tarayıcıda açıldı - Ctrl+P ile yazdırabilirsiniz.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Yazdırma hatası: {ex.Message}";
        }
    }

    private string GenerateFaturaHtml()
    {
        var sb = new StringBuilder();
        var gecerliKalemler = Kalemler.Where(k => k.Urun != null && k.NetKg > 0).ToList();
        
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head>");
        sb.AppendLine("<meta charset='UTF-8'>");
        sb.AppendLine($"<title>Sevkiyat Faturası - {FaturaNo}</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(@"
            body { font-family: 'Segoe UI', Arial, sans-serif; margin: 20px; background: #f5f5f5; }
            .fatura { background: white; padding: 30px; max-width: 900px; margin: 0 auto; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
            .header { border-bottom: 3px solid #0d47a1; padding-bottom: 20px; margin-bottom: 20px; }
            .header h1 { color: #0d47a1; margin: 0; font-size: 28px; }
            .header .subtitle { color: #666; font-size: 14px; margin-top: 5px; }
            .info-row { display: flex; justify-content: space-between; margin-bottom: 20px; }
            .info-box { flex: 1; padding: 15px; background: #f8f9fa; border-radius: 8px; margin: 0 5px; }
            .info-box:first-child { margin-left: 0; }
            .info-box:last-child { margin-right: 0; }
            .info-box h3 { margin: 0 0 10px 0; color: #333; font-size: 14px; border-bottom: 1px solid #ddd; padding-bottom: 5px; }
            .info-box p { margin: 5px 0; color: #666; font-size: 13px; }
            .info-box .value { color: #333; font-weight: 500; }
            table { width: 100%; border-collapse: collapse; margin: 20px 0; }
            th { background: #0d47a1; color: white; padding: 12px 10px; text-align: left; font-size: 13px; }
            td { padding: 10px; border-bottom: 1px solid #eee; font-size: 13px; }
            tr:hover { background: #f9f9f9; }
            .text-right { text-align: right; }
            .text-center { text-align: center; }
            .totals { margin-top: 20px; display: flex; justify-content: flex-end; }
            .totals-box { background: #f8f9fa; padding: 20px; min-width: 280px; border-radius: 8px; }
            .totals-row { display: flex; justify-content: space-between; margin: 8px 0; font-size: 14px; }
            .totals-row.grand { font-size: 20px; font-weight: bold; color: #0d47a1; border-top: 2px solid #0d47a1; padding-top: 15px; margin-top: 15px; }
            .footer { margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; text-align: center; color: #999; font-size: 11px; }
            .komisyoncu-tag { display: inline-block; background: #e3f2fd; color: #1565c0; padding: 2px 8px; border-radius: 4px; font-size: 11px; }
            @media print { 
                body { margin: 0; background: white; } 
                .fatura { box-shadow: none; max-width: none; }
            }
        ");
        sb.AppendLine("</style></head><body>");
        
        sb.AppendLine("<div class='fatura'>");
        
        // Header
        sb.AppendLine("<div class='header'>");
        sb.AppendLine("<h1>🚛 SEVKİYAT FATURASI</h1>");
        sb.AppendLine($"<div class='subtitle'>Fatura No: {FaturaNo} | Tarih: {FaturaTarihi?.DateTime.ToString("dd.MM.yyyy") ?? DateTime.Today.ToString("dd.MM.yyyy")}</div>");
        sb.AppendLine("</div>");
        
        // Info boxes
        sb.AppendLine("<div class='info-row'>");
        sb.AppendLine("<div class='info-box'>");
        sb.AppendLine("<h3>📍 ŞUBE BİLGİLERİ</h3>");
        sb.AppendLine($"<p><span class='value'>{SelectedSube?.Unvan ?? "-"}</span></p>");
        sb.AppendLine($"<p>Kod: {SelectedSube?.Kod ?? "-"}</p>");
        sb.AppendLine($"<p>Vergi No: {SelectedSube?.VergiNo ?? "-"}</p>");
        sb.AppendLine($"<p>Telefon: {SelectedSube?.Telefon ?? "-"}</p>");
        sb.AppendLine("</div>");
        
        // Komisyoncular
        var komisyoncuList = gecerliKalemler
            .Where(k => k.Komisyoncu != null)
            .Select(k => k.Komisyoncu!.Unvan)
            .Distinct()
            .ToList();
        
        sb.AppendLine("<div class='info-box'>");
        sb.AppendLine("<h3>🏪 KOMİSYONCULAR (HAL)</h3>");
        if (komisyoncuList.Any())
        {
            foreach (var kom in komisyoncuList)
            {
                sb.AppendLine($"<p><span class='komisyoncu-tag'>{kom}</span></p>");
            }
        }
        else
        {
            sb.AppendLine("<p>-</p>");
        }
        sb.AppendLine("</div>");
        sb.AppendLine("</div>");
        
        // Tablo
        sb.AppendLine("<table>");
        sb.AppendLine("<tr>");
        sb.AppendLine("<th>Ürün</th>");
        sb.AppendLine("<th>Kap Tipi</th>");
        sb.AppendLine("<th class='text-center'>Adet</th>");
        sb.AppendLine("<th class='text-right'>Net (kg)</th>");
        sb.AppendLine("<th>Komisyoncu</th>");
        sb.AppendLine("<th class='text-right'>Fiyat</th>");
        sb.AppendLine("<th class='text-right'>Tutar</th>");
        sb.AppendLine("</tr>");
        
        foreach (var kalem in gecerliKalemler)
        {
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>{kalem.UrunAdi}</td>");
            sb.AppendLine($"<td>{kalem.KapTipiAdi}</td>");
            sb.AppendLine($"<td class='text-center'>{kalem.KapAdet}</td>");
            sb.AppendLine($"<td class='text-right'>{kalem.NetKg:N2} kg</td>");
            sb.AppendLine($"<td><span class='komisyoncu-tag'>{kalem.KomisyoncuAdi}</span></td>");
            sb.AppendLine($"<td class='text-right'>{kalem.BirimFiyat:N2} ₺</td>");
            sb.AppendLine($"<td class='text-right'><strong>{kalem.Tutar:N2} ₺</strong></td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</table>");
        
        // Toplamlar
        sb.AppendLine("<div class='totals'>");
        sb.AppendLine("<div class='totals-box'>");
        sb.AppendLine($"<div class='totals-row'><span>Toplam Kap:</span><span>{ToplamKap} adet</span></div>");
        sb.AppendLine($"<div class='totals-row'><span>Toplam Ağırlık:</span><span>{ToplamKg:N2} kg</span></div>");
        sb.AppendLine($"<div class='totals-row'><span>Mal Tutarı:</span><span>{AraToplam:N2} ₺</span></div>");
        
        if (EkMasraflar > 0)
        {
            var masrafText = string.IsNullOrEmpty(MasrafAciklamasi) ? "Ek Masraf" : MasrafAciklamasi;
            sb.AppendLine($"<div class='totals-row'><span>{masrafText}:</span><span>+{EkMasraflar:N2} ₺</span></div>");
        }
        
        sb.AppendLine($"<div class='totals-row grand'><span>GENEL TOPLAM:</span><span>{GenelToplam:N2} ₺</span></div>");
        sb.AppendLine("</div>");
        sb.AppendLine("</div>");
        
        // Footer
        sb.AppendLine("<div class='footer'>");
        sb.AppendLine($"<p>Bu belge {DateTime.Now:dd.MM.yyyy HH:mm} tarihinde NeoHal sistemi tarafından oluşturulmuştur.</p>");
        sb.AppendLine("<p>Kesinti uygulanmamıştır - Komisyoncu faturaları mevcuttur.</p>");
        sb.AppendLine("</div>");
        
        sb.AppendLine("</div></body></html>");
        
        return sb.ToString();
    }
}

/// <summary>
/// Sevkiyat kalemi - Excel satırı gibi
/// Sevkiyatçı için: AlisFiyati = komisyoncudan alış, BirimFiyat = şubeye satış
/// Kar = BirimFiyat - AlisFiyati
/// </summary>
public partial class SevkiyatKalem : ObservableObject
{
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
    private CariHesap? _komisyoncu; // Malı aldığımız komisyoncu

    public string KomisyoncuAdi => Komisyoncu?.Unvan ?? string.Empty;
    
    /// <summary>
    /// Komisyoncudan alış fiyatı (maliyet)
    /// </summary>
    [ObservableProperty]
    private decimal _alisFiyati;

    [ObservableProperty]
    private decimal _birimFiyat; // Satış fiyatı

    // Tutar = Net Kg × Birim Fiyat (Satış)
    public decimal Tutar => NetKg * BirimFiyat;
    
    // Maliyet = Net Kg × Alış Fiyatı
    public decimal Maliyet => NetKg * AlisFiyati;
    
    // Kar = Tutar - Maliyet
    public decimal Kar => Tutar - Maliyet;
    
    // Otomatik hesaplama için property changed tetikleme
    partial void OnUrunChanged(Urun? value) => OnPropertyChanged(nameof(UrunAdi));
    partial void OnKapTipiChanged(KapTipi? value)
    {
        OnPropertyChanged(nameof(KapTipiAdi));
        OnPropertyChanged(nameof(NetKg));
        OnPropertyChanged(nameof(Tutar));
        OnPropertyChanged(nameof(Maliyet));
        OnPropertyChanged(nameof(Kar));
    }
    partial void OnKapAdetChanged(int value)
    {
        OnPropertyChanged(nameof(NetKg));
        OnPropertyChanged(nameof(Tutar));
        OnPropertyChanged(nameof(Maliyet));
        OnPropertyChanged(nameof(Kar));
    }
    partial void OnDaraliKgChanged(decimal value)
    {
        OnPropertyChanged(nameof(NetKg));
        OnPropertyChanged(nameof(Tutar));
        OnPropertyChanged(nameof(Maliyet));
        OnPropertyChanged(nameof(Kar));
    }
    partial void OnAlisFiyatiChanged(decimal value)
    {
        OnPropertyChanged(nameof(Maliyet));
        OnPropertyChanged(nameof(Kar));
    }
    partial void OnBirimFiyatChanged(decimal value)
    {
        OnPropertyChanged(nameof(Tutar));
        OnPropertyChanged(nameof(Kar));
    }
    partial void OnKomisyoncuChanged(CariHesap? value) => OnPropertyChanged(nameof(KomisyoncuAdi));
}
