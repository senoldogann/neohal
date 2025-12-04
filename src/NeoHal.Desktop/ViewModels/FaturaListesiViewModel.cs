using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeoHal.Core.Entities;
using NeoHal.Core.Enums;
using NeoHal.Desktop.Views;
using NeoHal.Services.Interfaces;

namespace NeoHal.Desktop.ViewModels;

public partial class FaturaListesiViewModel : ViewModelBase
{
    private readonly ISatisFaturasiService _faturaService;
    private readonly ICariHesapService _cariService;
    private readonly IGirisIrsaliyesiService _irsaliyeService;
    private readonly IUrunService _urunService;
    private readonly IKapTipiService _kapTipiService;

    [ObservableProperty]
    private ObservableCollection<SatisFaturasi> _faturalar = new();

    [ObservableProperty]
    private ObservableCollection<CariHesap> _musteriler = new();

    [ObservableProperty]
    private SatisFaturasi? _seciliFatura;

    [ObservableProperty]
    private CariHesap? _seciliMusteri;

    [ObservableProperty]
    private DateTimeOffset? _baslangicTarihi = DateTimeOffset.Now.AddDays(-30);

    [ObservableProperty]
    private DateTimeOffset? _bitisTarihi = DateTimeOffset.Now;

    [ObservableProperty]
    private string _aramaMetni = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Hazır";

    // Özet bilgiler
    public int ToplamFaturaSayisi => Faturalar.Count;
    public int ToplamKapOzet => Faturalar.Sum(f => f.Kalemler?.Sum(k => k.KapAdet) ?? 0);
    public decimal ToplamKgOzet => Faturalar.Sum(f => f.Kalemler?.Sum(k => k.NetKg) ?? 0);
    public decimal ToplamTutarOzet => Faturalar.Sum(f => f.GenelToplam);

    public FaturaListesiViewModel(
        ISatisFaturasiService faturaService,
        ICariHesapService cariService,
        IGirisIrsaliyesiService irsaliyeService,
        IUrunService urunService,
        IKapTipiService kapTipiService)
    {
        _faturaService = faturaService;
        _cariService = cariService;
        _irsaliyeService = irsaliyeService;
        _urunService = urunService;
        _kapTipiService = kapTipiService;
        
        Task.Run(LoadDataAsync);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            StatusMessage = "Veriler yükleniyor...";

            var musteriler = await _cariService.GetAllAsync();
            // Alıcı tipindeki carileri filtrele
            Musteriler = new ObservableCollection<CariHesap>(
                musteriler.Where(m => m.CariTipi == CariTipi.Alici));

            await AraAsync();

            StatusMessage = "Hazır";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AraAsync()
    {
        try
        {
            StatusMessage = "Faturalar aranıyor...";

            var tumFaturalar = await _faturaService.GetAllAsync();
            
            var filtrelenmis = tumFaturalar.AsEnumerable();

            // Tarih filtresi
            if (BaslangicTarihi.HasValue)
            {
                filtrelenmis = filtrelenmis.Where(f => f.FaturaTarihi >= BaslangicTarihi.Value.Date);
            }
            if (BitisTarihi.HasValue)
            {
                filtrelenmis = filtrelenmis.Where(f => f.FaturaTarihi <= BitisTarihi.Value.Date.AddDays(1));
            }

            // Müşteri filtresi
            if (SeciliMusteri != null)
            {
                filtrelenmis = filtrelenmis.Where(f => f.AliciId == SeciliMusteri.Id);
            }

            // Metin araması
            if (!string.IsNullOrWhiteSpace(AramaMetni))
            {
                var aranan = AramaMetni.ToLower();
                filtrelenmis = filtrelenmis.Where(f =>
                    (f.FaturaNo?.ToLower().Contains(aranan) ?? false) ||
                    (f.Aciklama?.ToLower().Contains(aranan) ?? false) ||
                    (f.Alici?.Unvan?.ToLower().Contains(aranan) ?? false));
            }

            // Tarihe göre sırala (yeniden eskiye)
            var sonuc = filtrelenmis.OrderByDescending(f => f.FaturaTarihi).ToList();

            Faturalar = new ObservableCollection<SatisFaturasi>(sonuc);
            
            RefreshSummary();
            StatusMessage = $"{Faturalar.Count} fatura bulundu";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Arama hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task YenileAsync()
    {
        SeciliMusteri = null;
        AramaMetni = string.Empty;
        BaslangicTarihi = DateTimeOffset.Now.AddDays(-30);
        BitisTarihi = DateTimeOffset.Now;
        await AraAsync();
    }

    // Seçili fatura için komutlar
    [RelayCommand]
    private async Task DuzenleSeciliAsync()
    {
        if (SeciliFatura == null) return;
        await DuzenleAsync(SeciliFatura);
    }

    [RelayCommand]
    private async Task YazdirSeciliAsync()
    {
        if (SeciliFatura == null) return;
        await YazdirAsync(SeciliFatura);
    }

    [RelayCommand]
    private async Task PdfSeciliAsync()
    {
        if (SeciliFatura == null) return;
        await PdfAsync(SeciliFatura);
    }

    [RelayCommand]
    private async Task DuzenleAsync(SatisFaturasi? fatura)
    {
        if (fatura == null) return;
        
        // Faturayı kalemlerle birlikte yükle
        var faturaDetay = await _faturaService.GetByIdWithKalemlerAsync(fatura.Id);
        if (faturaDetay == null) return;

        var vm = new SatisFaturasiEditViewModel(
            _faturaService,
            _cariService,
            _irsaliyeService,
            _urunService,
            _kapTipiService,
            async (saved) =>
            {
                if (saved)
                {
                    await AraAsync();
                }
            },
            faturaDetay
        );

        var window = new SatisFaturasiWindow
        {
            DataContext = vm
        };

        if (App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            await window.ShowDialog(desktop.MainWindow!);
        }
    }

    [RelayCommand]
    private async Task YazdirAsync(SatisFaturasi? fatura)
    {
        if (fatura == null) return;
        
        try
        {
            StatusMessage = $"Fatura yazdırılıyor: {fatura.FaturaNo}";
            
            // PDF oluştur ve yazdır
            var pdfPath = await GeneratePdfAsync(fatura);
            
            // Yazdırma dialog'u aç
            var psi = new ProcessStartInfo
            {
                FileName = pdfPath,
                UseShellExecute = true,
                Verb = "print"
            };
            Process.Start(psi);
            
            StatusMessage = $"Fatura yazdırıldı: {fatura.FaturaNo}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Yazdırma hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task PdfAsync(SatisFaturasi? fatura)
    {
        if (fatura == null) return;
        
        try
        {
            StatusMessage = $"PDF oluşturuluyor: {fatura.FaturaNo}";
            
            var pdfPath = await GeneratePdfAsync(fatura);
            
            // PDF'i varsayılan uygulama ile aç (önizleme)
            var psi = new ProcessStartInfo
            {
                FileName = pdfPath,
                UseShellExecute = true
            };
            Process.Start(psi);
            
            StatusMessage = $"PDF açıldı: {fatura.FaturaNo}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"PDF hatası: {ex.Message}";
        }
    }

    private async Task<string> GeneratePdfAsync(SatisFaturasi fatura)
    {
        // Basit HTML -> PDF dönüşümü için temp dosya oluştur
        var tempDir = Path.Combine(Path.GetTempPath(), "NeoHal");
        Directory.CreateDirectory(tempDir);
        
        var htmlPath = Path.Combine(tempDir, $"Fatura_{fatura.FaturaNo}.html");
        var html = GenerateFaturaHtml(fatura);
        await File.WriteAllTextAsync(htmlPath, html);
        
        // HTML'i döndür (gerçek PDF için QuestPDF veya benzeri kütüphane eklenebilir)
        return htmlPath;
    }

    private string GenerateFaturaHtml(SatisFaturasi fatura)
    {
        var kalemlerHtml = string.Join("\n", fatura.Kalemler?.Select(k => $@"
            <tr>
                <td>{k.Urun?.Ad ?? "-"}</td>
                <td>{k.KapTipi?.Ad ?? "-"}</td>
                <td style='text-align:center'>{k.KapAdet}</td>
                <td style='text-align:right'>{k.NetKg:N2}</td>
                <td style='text-align:right'>{k.BirimFiyat:N2}</td>
                <td style='text-align:right'><strong>{k.Tutar:N2}</strong></td>
            </tr>") ?? Array.Empty<string>());

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Fatura - {fatura.FaturaNo}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        h1 {{ color: #1565C0; border-bottom: 2px solid #1565C0; padding-bottom: 10px; }}
        .header {{ display: flex; justify-content: space-between; margin-bottom: 20px; }}
        .info {{ background: #f5f5f5; padding: 15px; border-radius: 5px; margin-bottom: 20px; }}
        table {{ width: 100%; border-collapse: collapse; margin-bottom: 20px; }}
        th, td {{ border: 1px solid #ddd; padding: 10px; }}
        th {{ background: #1565C0; color: white; }}
        .total {{ text-align: right; font-size: 18px; }}
        .footer {{ margin-top: 30px; text-align: center; color: #888; font-size: 12px; }}
    </style>
</head>
<body>
    <h1>🧾 SATIŞ FATURASI</h1>
    
    <div class='info'>
        <div class='header'>
            <div>
                <strong>Fatura No:</strong> {fatura.FaturaNo}<br>
                <strong>Tarih:</strong> {fatura.FaturaTarihi:dd.MM.yyyy}
            </div>
            <div>
                <strong>Müşteri:</strong> {fatura.Alici?.Unvan ?? "-"}<br>
                <strong>Adres:</strong> {fatura.Alici?.Adres ?? "-"}
            </div>
        </div>
    </div>
    
    <table>
        <thead>
            <tr>
                <th>Ürün</th>
                <th>Kap Tipi</th>
                <th>Kap Ad.</th>
                <th>Net Kg</th>
                <th>Fiyat (₺)</th>
                <th>Tutar (₺)</th>
            </tr>
        </thead>
        <tbody>
            {kalemlerHtml}
        </tbody>
    </table>
    
    <div class='total'>
        <p>Ara Toplam: <strong>{fatura.AraToplam:N2} ₺</strong></p>
        <p>Masraflar: <strong>{(fatura.GenelToplam - fatura.AraToplam):N2} ₺</strong></p>
        <p style='font-size:24px; color:#4CAF50'>GENEL TOPLAM: <strong>{fatura.GenelToplam:N2} ₺</strong></p>
    </div>
    
    <div class='footer'>
        <p>NeoHal Sistem - {DateTime.Now:dd.MM.yyyy HH:mm}</p>
    </div>
</body>
</html>";
    }

    private void RefreshSummary()
    {
        OnPropertyChanged(nameof(ToplamFaturaSayisi));
        OnPropertyChanged(nameof(ToplamKapOzet));
        OnPropertyChanged(nameof(ToplamKgOzet));
        OnPropertyChanged(nameof(ToplamTutarOzet));
    }
}
