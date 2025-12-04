using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeoHal.Services.Interfaces;

namespace NeoHal.Desktop.ViewModels;

public partial class RaporViewModel : ViewModelBase
{
    private readonly IRaporService _raporService;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string? _raporAdi;
    
    [ObservableProperty]
    private DateTimeOffset _baslangicTarihi = DateTimeOffset.Now.AddMonths(-1);
    
    [ObservableProperty]
    private DateTimeOffset _bitisTarihi = DateTimeOffset.Now;
    
    [ObservableProperty]
    private RaporSonucu? _aktifRapor;
    
    [ObservableProperty]
    private string? _selectedRaporTipi;
    
    [ObservableProperty]
    private Guid? _selectedCariId;
    
    [ObservableProperty]
    private Guid? _selectedUrunId;
    
    [ObservableProperty]
    private string? _exportFormat;
    
    [ObservableProperty]
    private string? _statusMessage;
    
    public ObservableCollection<RaporSatiri> RaporSatirlari { get; } = new();
    
    // Rapor satırlarını düz string listesi olarak tutar (DataGrid için)
    public ObservableCollection<RaporSatirDisplay> RaporSatirDisplaylar { get; } = new();
    
    public ObservableCollection<GrafikVerisi> GrafikVerileri { get; } = new();
    public ObservableCollection<RaporSutunu> RaporSutunlari { get; } = new();
    
    public List<string> RaporTipleri { get; } = new()
    {
        "Satış Raporu",
        "Cari Ekstre",
        "Ürün Satış Raporu",
        "Kasa Stok Durumu",
        "Kasa Raporu",
        "HKS Raporu"
    };
    
    public List<string> ExportFormatlari { get; } = new()
    {
        "PDF",
        "Excel",
        "CSV"
    };
    
    public RaporViewModel(IRaporService raporService)
    {
        _raporService = raporService;
        SelectedRaporTipi = RaporTipleri.FirstOrDefault();
        ExportFormat = ExportFormatlari.FirstOrDefault();
    }
    
    [RelayCommand]
    private async Task RaporOlusturAsync()
    {
        if (string.IsNullOrEmpty(SelectedRaporTipi))
        {
            StatusMessage = "Lütfen bir rapor tipi seçin";
            return;
        }
        
        IsLoading = true;
        StatusMessage = "Rapor oluşturuluyor...";
        
        try
        {
            RaporSonucu? rapor = null;
            
            switch (SelectedRaporTipi)
            {
                case "Satış Raporu":
                    rapor = await _raporService.SatisRaporuAsync(BaslangicTarihi.DateTime, BitisTarihi.DateTime);
                    break;
                    
                case "Cari Ekstre":
                    if (SelectedCariId.HasValue)
                    {
                        rapor = await _raporService.CariExtreRaporuAsync(SelectedCariId.Value, BaslangicTarihi.DateTime, BitisTarihi.DateTime);
                    }
                    else
                    {
                        StatusMessage = "Cari ekstre için bir cari seçmelisiniz";
                        return;
                    }
                    break;
                    
                case "Ürün Satış Raporu":
                    rapor = await _raporService.UrunSatisRaporuAsync(BaslangicTarihi.DateTime, BitisTarihi.DateTime, SelectedUrunId);
                    break;
                    
                case "Kasa Stok Durumu":
                    rapor = await _raporService.StokDurumuRaporuAsync(BitisTarihi.DateTime);
                    break;
                    
                case "Kasa Raporu":
                    rapor = await _raporService.KasaRaporuAsync(BaslangicTarihi.DateTime, BitisTarihi.DateTime);
                    break;
                    
                case "HKS Raporu":
                    rapor = await _raporService.HksBildirimRaporuAsync(BaslangicTarihi.DateTime, BitisTarihi.DateTime);
                    break;
            }
            
            if (rapor != null)
            {
                AktifRapor = rapor;
                RaporAdi = rapor.RaporAdi;
                
                // Sütunları güncelle
                RaporSutunlari.Clear();
                foreach (var sutun in rapor.Sutunlar)
                {
                    RaporSutunlari.Add(sutun);
                }
                
                // Satırları güncelle
                RaporSatirlari.Clear();
                RaporSatirDisplaylar.Clear();
                foreach (var satir in rapor.Satirlar)
                {
                    RaporSatirlari.Add(satir);
                    
                    // Düz görüntüleme için Display objesi oluştur
                    var display = new RaporSatirDisplay();
                    foreach (var kv in satir.Degerler)
                    {
                        display.SetValue(kv.Key, kv.Value);
                    }
                    RaporSatirDisplaylar.Add(display);
                }
                
                // Grafik verilerini güncelle
                GrafikVerileri.Clear();
                foreach (var veri in rapor.GrafikVerileri)
                {
                    GrafikVerileri.Add(veri);
                }
                
                StatusMessage = $"Rapor oluşturuldu: {rapor.Satirlar.Count} kayıt bulundu";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task DışaAktarAsync()
    {
        if (AktifRapor == null)
        {
            StatusMessage = "Dışa aktarılacak rapor yok. Önce rapor oluşturun.";
            return;
        }
        
        if (string.IsNullOrEmpty(ExportFormat))
        {
            StatusMessage = "Lütfen bir format seçin";
            return;
        }
        
        IsLoading = true;
        StatusMessage = $"{ExportFormat} formatında dışa aktarılıyor...";
        
        try
        {
            // Dosya yolunu belirle
            var dosyaAdi = $"{AktifRapor.RaporAdi?.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}";
            var belgelerKlasoru = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var raporlarKlasoru = System.IO.Path.Combine(belgelerKlasoru, "NeoHal", "Raporlar");
            
            if (!System.IO.Directory.Exists(raporlarKlasoru))
            {
                System.IO.Directory.CreateDirectory(raporlarKlasoru);
            }
            
            byte[] dosyaIcerigi;
            string dosyaUzantisi;
            
            switch (ExportFormat)
            {
                case "PDF":
                    dosyaIcerigi = await _raporService.ExportToPdfAsync(AktifRapor);
                    dosyaUzantisi = ".pdf";
                    break;
                    
                case "Excel":
                    dosyaIcerigi = await _raporService.ExportToExcelAsync(AktifRapor);
                    dosyaUzantisi = ".xlsx";
                    break;
                    
                case "CSV":
                    dosyaIcerigi = await _raporService.ExportToCsvAsync(AktifRapor);
                    dosyaUzantisi = ".csv";
                    break;
                    
                default:
                    StatusMessage = "Geçersiz format";
                    return;
            }
            
            var tamDosyaYolu = System.IO.Path.Combine(raporlarKlasoru, dosyaAdi + dosyaUzantisi);
            await System.IO.File.WriteAllBytesAsync(tamDosyaYolu, dosyaIcerigi);
            
            // Dosyayı varsayılan uygulamayla aç
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo(tamDosyaYolu) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "open",
                        Arguments = $"\"{tamDosyaYolu}\"",
                        UseShellExecute = false
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "xdg-open",
                        Arguments = $"\"{tamDosyaYolu}\"",
                        UseShellExecute = false
                    });
                }
            }
            catch (Exception openEx)
            {
                StatusMessage = $"⚠️ Rapor kaydedildi ama açılamadı: {openEx.Message}";
            }
            
            StatusMessage = $"✅ Rapor kaydedildi ve açıldı: {tamDosyaYolu}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Dışa aktarma hatası: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private void Temizle()
    {
        AktifRapor = null;
        RaporAdi = null;
        RaporSatirlari.Clear();
        RaporSutunlari.Clear();
        GrafikVerileri.Clear();
        StatusMessage = "Rapor temizlendi";
    }
    
    [RelayCommand]
    private void BugunkuTarihAyarla()
    {
        BaslangicTarihi = DateTimeOffset.Now.Date;
        BitisTarihi = DateTimeOffset.Now.Date;
    }
    
    [RelayCommand]
    private void BuHaftaAyarla()
    {
        var bugun = DateTimeOffset.Now;
        var haftaninBasi = bugun.AddDays(-(int)bugun.DayOfWeek + 1);
        BaslangicTarihi = haftaninBasi;
        BitisTarihi = bugun;
    }
    
    [RelayCommand]
    private void BuAyAyarla()
    {
        var bugun = DateTimeOffset.Now;
        BaslangicTarihi = new DateTimeOffset(bugun.Year, bugun.Month, 1, 0, 0, 0, bugun.Offset);
        BitisTarihi = bugun;
    }
    
    [RelayCommand]
    private void BuYilAyarla()
    {
        var bugun = DateTimeOffset.Now;
        BaslangicTarihi = new DateTimeOffset(bugun.Year, 1, 1, 0, 0, 0, bugun.Offset);
        BitisTarihi = bugun;
    }
}

/// <summary>
/// Rapor satırı görüntüleme için wrapper class
/// DataGrid düz property'leri daha iyi gösterir
/// </summary>
public class RaporSatirDisplay
{
    // Satış raporu için sabit property'ler
    public string FaturaNo { get; set; } = "";
    public string Tarih { get; set; } = "";
    public string Musteri { get; set; } = "";
    public string Urun { get; set; } = "";
    public string Miktar { get; set; } = "";
    public string BirimFiyat { get; set; } = "";
    public string Tutar { get; set; } = "";
    
    public void SetValue(string key, object? value)
    {
        var strValue = FormatValue(key, value);
        
        switch (key)
        {
            case "FaturaNo": FaturaNo = strValue; break;
            case "Tarih": Tarih = strValue; break;
            case "CariUnvan": Musteri = strValue; break;
            case "UrunAdi": Urun = strValue; break;
            case "Miktar": Miktar = strValue; break;
            case "BirimFiyat": BirimFiyat = strValue; break;
            case "Tutar": Tutar = strValue; break;
        }
    }
    
    private string FormatValue(string key, object? value)
    {
        if (value == null) return "-";
        
        if (value is DateTime dt)
            return dt.ToString("dd.MM.yyyy");
        
        if (value is decimal d)
        {
            if (key == "Miktar")
                return $"{d:N2} kg";
            return $"{d:N2} ₺";
        }
        
        return value.ToString() ?? "-";
    }
}
