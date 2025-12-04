using System;
using System.Threading.Tasks;
using NeoHal.Core.Entities;

namespace NeoHal.Services.Interfaces;

/// <summary>
/// Yazdırma servisi - Fatura, İrsaliye, Müstahsil Makbuzu vb.
/// </summary>
public interface IPrintService
{
    /// <summary>
    /// Satış faturası önizleme HTML'i oluşturur
    /// </summary>
    Task<string> GenerateFaturaPreviewHtmlAsync(SatisFaturasi fatura);
    
    /// <summary>
    /// Giriş irsaliyesi önizleme HTML'i oluşturur
    /// </summary>
    Task<string> GenerateIrsaliyePreviewHtmlAsync(GirisIrsaliyesi irsaliye);
    
    /// <summary>
    /// Müstahsil makbuzu önizleme HTML'i oluşturur
    /// </summary>
    Task<string> GenerateMustahsilMakbuzuPreviewHtmlAsync(MustahsilMakbuzu makbuz);
    
    /// <summary>
    /// Belgeyi yazdırır
    /// </summary>
    Task PrintDocumentAsync(string htmlContent, string documentTitle);
}
