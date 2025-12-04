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
/// Şube Tahsilat Ekranı - Şubelerden gelen ödemeleri kaydetme
/// - Nakit, Çek, Havale/EFT tahsilatları
/// - Borç/Alacak takibi
/// - Tahsilat geçmişi
/// </summary>
public partial class SubeTahsilatViewModel : ViewModelBase
{
    private readonly ICariHesapService _cariService;
    private readonly ICariHareketService _cariHareketService;
    private readonly IKasaHesabiService _kasaService;
    private readonly ISatisFaturasiService _faturaService;

    [ObservableProperty]
    private ObservableCollection<CariHesap> _subeler = new();

    [ObservableProperty]
    private CariHesap? _selectedSube;

    [ObservableProperty]
    private ObservableCollection<CariHareket> _tahsilatGecmisi = new();

    [ObservableProperty]
    private ObservableCollection<SatisFaturasi> _odenmemisFaturalar = new();

    [ObservableProperty]
    private string _statusMessage = "Şube seçerek tahsilat yapabilirsiniz";

    // Bakiye bilgileri
    [ObservableProperty]
    private decimal _toplamBorc;

    [ObservableProperty]
    private decimal _toplamAlacak;

    [ObservableProperty]
    private decimal _bakiye; // Pozitif = Borçlu, Negatif = Alacaklı

    // Yeni tahsilat formu
    [ObservableProperty]
    private bool _tahsilatDialogAcik;

    [ObservableProperty]
    private decimal _tahsilatTutari;

    [ObservableProperty]
    private OdemeTuru _selectedOdemeTuru = OdemeTuru.Nakit;

    [ObservableProperty]
    private string _tahsilatAciklama = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _tahsilatTarihi = DateTimeOffset.Now;

    // Çek bilgileri
    [ObservableProperty]
    private string _cekNo = string.Empty;

    [ObservableProperty]
    private string _cekBanka = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _cekVadeTarihi;

    // Filtre
    [ObservableProperty]
    private DateTimeOffset? _filtreBaslangic = DateTimeOffset.Now.AddMonths(-3);

    [ObservableProperty]
    private DateTimeOffset? _filtreBitis = DateTimeOffset.Now;

    public ObservableCollection<OdemeTuru> OdemeTurleri { get; } = new(Enum.GetValues<OdemeTuru>());

    public SubeTahsilatViewModel(
        ICariHesapService cariService,
        ICariHareketService cariHareketService,
        IKasaHesabiService kasaService,
        ISatisFaturasiService faturaService)
    {
        _cariService = cariService;
        _cariHareketService = cariHareketService;
        _kasaService = kasaService;
        _faturaService = faturaService;

        _ = LoadSubelerAsync();
    }

    private async Task LoadSubelerAsync()
    {
        try
        {
            // Alıcı tipindeki carileri getir (Şubeler)
            var subeler = await _cariService.GetByCariTipiAsync(CariTipi.Alici);
            Subeler = new ObservableCollection<CariHesap>(subeler.OrderBy(s => s.Unvan));
            StatusMessage = $"{Subeler.Count} şube listelendi";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Yükleme hatası: {ex.Message}";
        }
    }

    partial void OnSelectedSubeChanged(CariHesap? value)
    {
        if (value != null)
        {
            _ = LoadSubeDetayAsync();
        }
        else
        {
            TahsilatGecmisi.Clear();
            OdenmemisFaturalar.Clear();
            ToplamBorc = 0;
            ToplamAlacak = 0;
            Bakiye = 0;
        }
    }

    private async Task LoadSubeDetayAsync()
    {
        if (SelectedSube == null) return;

        try
        {
            StatusMessage = "Şube detayları yükleniyor...";

            // Bakiye hesapla
            Bakiye = await _cariService.GetBakiyeAsync(SelectedSube.Id);

            // Cari hareketleri getir
            var hareketler = await _cariHareketService.GetByCariIdAsync(SelectedSube.Id);
            
            var baslangic = FiltreBaslangic?.DateTime ?? DateTime.Today.AddMonths(-3);
            var bitis = FiltreBitis?.DateTime ?? DateTime.Today.AddDays(1);
            
            var filtrelenmis = hareketler
                .Where(h => h.Tarih >= baslangic && h.Tarih <= bitis)
                .OrderByDescending(h => h.Tarih)
                .ToList();

            // Listeyi temizle ve yeniden doldur - UI güncellemesi için
            TahsilatGecmisi.Clear();
            foreach (var hareket in filtrelenmis)
            {
                TahsilatGecmisi.Add(hareket);
            }

            ToplamBorc = hareketler.Where(h => h.HareketTipi == CariHareketTipi.Borc).Sum(h => h.Tutar);
            ToplamAlacak = hareketler.Where(h => h.HareketTipi == CariHareketTipi.Alacak).Sum(h => h.Tutar);

            // Ödenmemiş faturaları getir
            var tumFaturalar = await _faturaService.GetAllAsync();
            var subeFaturalari = tumFaturalar
                .Where(f => f.AliciId == SelectedSube.Id && f.Durum != BelgeDurumu.Iptal)
                .OrderByDescending(f => f.FaturaTarihi)
                .ToList();

            OdenmemisFaturalar.Clear();
            foreach (var fatura in subeFaturalari)
            {
                OdenmemisFaturalar.Add(fatura);
            }

            StatusMessage = $"✅ {SelectedSube.Unvan} - Bakiye: {Bakiye:N2} ₺";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Detay yükleme hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task YenileAsync()
    {
        await LoadSubeDetayAsync();
    }

    [RelayCommand]
    private void YeniTahsilat()
    {
        if (SelectedSube == null)
        {
            StatusMessage = "⚠️ Önce şube seçin!";
            return;
        }

        TahsilatTutari = 0;
        SelectedOdemeTuru = OdemeTuru.Nakit;
        TahsilatAciklama = string.Empty;
        TahsilatTarihi = DateTimeOffset.Now;
        CekNo = string.Empty;
        CekBanka = string.Empty;
        CekVadeTarihi = null;

        TahsilatDialogAcik = true;
        StatusMessage = $"💰 {SelectedSube.Unvan} için tahsilat girişi";
    }

    [RelayCommand]
    private async Task TahsilatKaydetAsync()
    {
        try
        {
            if (SelectedSube == null)
            {
                StatusMessage = "⚠️ Şube seçili değil!";
                return;
            }

            if (TahsilatTutari <= 0)
            {
                StatusMessage = "⚠️ Tutar 0'dan büyük olmalı!";
                return;
            }

            StatusMessage = "Tahsilat kaydediliyor...";

            var tarih = TahsilatTarihi?.DateTime ?? DateTime.Now;
            var aciklama = string.IsNullOrWhiteSpace(TahsilatAciklama) 
                ? $"{SelectedOdemeTuru} tahsilatı" 
                : TahsilatAciklama;

            // Çek ise ek bilgi ekle
            if (SelectedOdemeTuru == OdemeTuru.Cek && !string.IsNullOrWhiteSpace(CekNo))
            {
                aciklama += $" - Çek No: {CekNo}";
                if (!string.IsNullOrWhiteSpace(CekBanka))
                    aciklama += $", Banka: {CekBanka}";
                if (CekVadeTarihi.HasValue)
                    aciklama += $", Vade: {CekVadeTarihi.Value:dd.MM.yyyy}";
            }

            // Cari hareket oluştur (Alacak = şubeden tahsilat)
            var cariHareket = new CariHareket
            {
                CariId = SelectedSube.Id,
                HareketTipi = CariHareketTipi.Alacak, // Bizim alacağımız
                Tutar = TahsilatTutari,
                Tarih = tarih,
                Aciklama = aciklama,
                ReferansBelgeTipi = "Tahsilat"
            };

            await _cariHareketService.CreateAsync(cariHareket);

            // Kasa hareketi oluştur (Giriş)
            var kasaHareketi = new KasaHesabi
            {
                Tarih = tarih,
                GirisHareketi = true,
                Tutar = TahsilatTutari,
                OdemeTuru = SelectedOdemeTuru,
                CariId = SelectedSube.Id,
                ReferansBelgeTipi = "Tahsilat",
                Aciklama = $"{SelectedSube.Unvan} - {aciklama}"
            };

            await _kasaService.CreateAsync(kasaHareketi);

            TahsilatDialogAcik = false;
            await LoadSubeDetayAsync();

            StatusMessage = $"✅ Tahsilat kaydedildi: {TahsilatTutari:N2} ₺ ({SelectedOdemeTuru})";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Tahsilat hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private void TahsilatIptal()
    {
        TahsilatDialogAcik = false;
        StatusMessage = "Tahsilat iptal edildi.";
    }

    [RelayCommand]
    private void HizliNakitTahsilat()
    {
        if (SelectedSube == null)
        {
            StatusMessage = "⚠️ Önce şube seçin!";
            return;
        }

        SelectedOdemeTuru = OdemeTuru.Nakit;
        TahsilatTutari = Bakiye > 0 ? Bakiye : 0; // Borç kadar
        TahsilatAciklama = "Nakit tahsilat";
        TahsilatTarihi = DateTimeOffset.Now;
        TahsilatDialogAcik = true;
    }

    [RelayCommand]
    private void HizliCekTahsilat()
    {
        if (SelectedSube == null)
        {
            StatusMessage = "⚠️ Önce şube seçin!";
            return;
        }

        SelectedOdemeTuru = OdemeTuru.Cek;
        TahsilatTutari = 0;
        TahsilatAciklama = "Çek tahsilatı";
        TahsilatTarihi = DateTimeOffset.Now;
        CekVadeTarihi = DateTimeOffset.Now.AddDays(30);
        TahsilatDialogAcik = true;
    }

    [RelayCommand]
    private void HizliHavaleTahsilat()
    {
        if (SelectedSube == null)
        {
            StatusMessage = "⚠️ Önce şube seçin!";
            return;
        }

        SelectedOdemeTuru = OdemeTuru.Havale;
        TahsilatTutari = 0;
        TahsilatAciklama = "Havale/EFT";
        TahsilatTarihi = DateTimeOffset.Now;
        TahsilatDialogAcik = true;
    }
}
