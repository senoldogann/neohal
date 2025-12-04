using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NeoHal.Desktop.ViewModels;

public partial class PrintPreviewViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _documentTitle = "Belge";
    
    [ObservableProperty]
    private string _documentDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
    
    [ObservableProperty]
    private string _htmlContent = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<KeyValuePair<string, string>> _summaryItems = new();
    
    public event EventHandler? CloseRequested;
    
    public PrintPreviewViewModel()
    {
    }
    
    public void SetFaturaData(string faturaNo, string aliciUnvan, decimal genelToplam, int kalemSayisi)
    {
        DocumentTitle = $"Satış Faturası - {faturaNo}";
        SummaryItems = new ObservableCollection<KeyValuePair<string, string>>
        {
            new("Fatura No", faturaNo),
            new("Alıcı", aliciUnvan),
            new("Kalem Sayısı", kalemSayisi.ToString()),
            new("Genel Toplam", $"{genelToplam:N2} ₺")
        };
    }
    
    public void SetIrsaliyeData(string irsaliyeNo, string mustahsilUnvan, decimal toplamNet, int kalemSayisi)
    {
        DocumentTitle = $"Giriş İrsaliyesi - {irsaliyeNo}";
        SummaryItems = new ObservableCollection<KeyValuePair<string, string>>
        {
            new("İrsaliye No", irsaliyeNo),
            new("Müstahsil", mustahsilUnvan),
            new("Kalem Sayısı", kalemSayisi.ToString()),
            new("Toplam Net", $"{toplamNet:N2} kg")
        };
    }
    
    [RelayCommand]
    private async Task PrintAsync()
    {
        try
        {
            // HTML içeriğini geçici dosyaya kaydet
            var tempFile = Path.Combine(Path.GetTempPath(), $"NeoHal_Print_{Guid.NewGuid():N}.html");
            await File.WriteAllTextAsync(tempFile, HtmlContent);
            
            // Platforma göre yazdırma işlemi
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows: Varsayılan tarayıcıda aç (kullanıcı Ctrl+P yapabilir)
                Process.Start(new ProcessStartInfo
                {
                    FileName = tempFile,
                    UseShellExecute = true
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // macOS: open komutu ile aç
                Process.Start("open", tempFile);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Linux: xdg-open ile aç
                Process.Start("xdg-open", tempFile);
            }
            
            // Pencereyi kapat
            await Task.Delay(500);
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            // Hata durumunda mesaj göster
            Debug.WriteLine($"Yazdırma hatası: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
