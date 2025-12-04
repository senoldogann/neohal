using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace NeoHal.Desktop.Views;

public partial class PrintPreviewWindow : Window
{
    private string? _htmlContent;
    private string? _documentTitle;
    
    public PrintPreviewWindow()
    {
        InitializeComponent();
    }
    
    public void SetContent(string title, string htmlContent)
    {
        _documentTitle = title;
        _htmlContent = htmlContent;
        Title = $"Yazdırma Önizleme - {title}";
    }
    
    private async void OnPrintClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_htmlContent))
        {
            return;
        }
        
        try
        {
            // HTML'i geçici dosyaya kaydet
            var fileName = $"NeoHal_{_documentTitle?.Replace(" ", "_") ?? "Belge"}_{DateTime.Now:yyyyMMddHHmmss}.html";
            var tempPath = Path.Combine(Path.GetTempPath(), fileName);
            await File.WriteAllTextAsync(tempPath, _htmlContent);
            
            // Platforma göre tarayıcıda aç
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", tempPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", tempPath);
            }
            
            Close();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Yazdırma hatası: {ex.Message}");
        }
    }
    
    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
