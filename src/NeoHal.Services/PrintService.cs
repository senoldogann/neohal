using System;
using System.Text;
using System.Threading.Tasks;
using NeoHal.Core.Entities;
using NeoHal.Services.Interfaces;

namespace NeoHal.Services;

/// <summary>
/// Yazdırma servisi - HTML tabanlı önizleme ve yazdırma
/// </summary>
public class PrintService : IPrintService
{
    public Task<string> GenerateFaturaPreviewHtmlAsync(SatisFaturasi fatura)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head>");
        sb.AppendLine("<meta charset='UTF-8'>");
        sb.AppendLine("<title>Satış Faturası</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(@"
            body { font-family: 'Segoe UI', Arial, sans-serif; margin: 20px; background: #f5f5f5; }
            .fatura { background: white; padding: 30px; max-width: 800px; margin: 0 auto; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
            .header { border-bottom: 2px solid #1e88e5; padding-bottom: 20px; margin-bottom: 20px; }
            .header h1 { color: #1e88e5; margin: 0; font-size: 24px; }
            .header .fatura-no { color: #666; font-size: 14px; }
            .info-row { display: flex; justify-content: space-between; margin-bottom: 20px; }
            .info-box { flex: 1; padding: 10px; }
            .info-box h3 { margin: 0 0 10px 0; color: #333; font-size: 14px; border-bottom: 1px solid #ddd; padding-bottom: 5px; }
            .info-box p { margin: 5px 0; color: #666; font-size: 12px; }
            table { width: 100%; border-collapse: collapse; margin: 20px 0; }
            th { background: #1e88e5; color: white; padding: 10px; text-align: left; font-size: 12px; }
            td { padding: 10px; border-bottom: 1px solid #eee; font-size: 12px; }
            tr:hover { background: #f9f9f9; }
            .text-right { text-align: right; }
            .text-center { text-align: center; }
            .totals { margin-top: 20px; display: flex; justify-content: flex-end; }
            .totals-box { background: #f9f9f9; padding: 15px; min-width: 250px; }
            .totals-row { display: flex; justify-content: space-between; margin: 5px 0; font-size: 12px; }
            .totals-row.grand { font-size: 16px; font-weight: bold; color: #1e88e5; border-top: 2px solid #1e88e5; padding-top: 10px; margin-top: 10px; }
            .footer { margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; text-align: center; color: #999; font-size: 10px; }
            @media print { body { margin: 0; background: white; } .fatura { box-shadow: none; } }
        ");
        sb.AppendLine("</style></head><body>");
        
        sb.AppendLine("<div class='fatura'>");
        
        // Header
        sb.AppendLine("<div class='header'>");
        sb.AppendLine("<h1>SATIŞ FATURASI</h1>");
        sb.AppendLine($"<div class='fatura-no'>Fatura No: {fatura.FaturaNo} | Tarih: {fatura.FaturaTarihi:dd.MM.yyyy}</div>");
        sb.AppendLine("</div>");
        
        // Bilgiler
        sb.AppendLine("<div class='info-row'>");
        sb.AppendLine("<div class='info-box'>");
        sb.AppendLine("<h3>ALICI BİLGİLERİ</h3>");
        sb.AppendLine($"<p><strong>{fatura.Alici?.Unvan ?? "-"}</strong></p>");
        sb.AppendLine($"<p>Kod: {fatura.Alici?.Kod ?? "-"}</p>");
        sb.AppendLine($"<p>Vergi No: {fatura.Alici?.VergiNo ?? fatura.Alici?.TcKimlikNo ?? "-"}</p>");
        sb.AppendLine($"<p>Telefon: {fatura.Alici?.Telefon ?? "-"}</p>");
        sb.AppendLine("</div>");
        
        sb.AppendLine("<div class='info-box'>");
        sb.AppendLine("<h3>MÜSTAHSİL BİLGİLERİ</h3>");
        if (fatura.Mustahsil != null)
        {
            sb.AppendLine($"<p><strong>{fatura.Mustahsil.Unvan}</strong></p>");
            sb.AppendLine($"<p>Kod: {fatura.Mustahsil.Kod}</p>");
        }
        else
        {
            sb.AppendLine("<p>-</p>");
        }
        sb.AppendLine("</div>");
        sb.AppendLine("</div>");
        
        // Kalemler
        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th>Ürün</th><th>Kap</th><th class='text-center'>Adet</th><th class='text-right'>Net (kg)</th><th class='text-right'>Fiyat</th><th class='text-right'>Tutar</th></tr>");
        
        if (fatura.Kalemler != null)
        {
            foreach (var kalem in fatura.Kalemler)
            {
                sb.AppendLine($"<tr>");
                sb.AppendLine($"<td>{kalem.Urun?.Ad ?? "-"}</td>");
                sb.AppendLine($"<td>{kalem.KapTipi?.Ad ?? "-"}</td>");
                sb.AppendLine($"<td class='text-center'>{kalem.KapAdet}</td>");
                sb.AppendLine($"<td class='text-right'>{kalem.NetKg:N2}</td>");
                sb.AppendLine($"<td class='text-right'>{kalem.BirimFiyat:N2} ₺</td>");
                sb.AppendLine($"<td class='text-right'>{kalem.Tutar:N2} ₺</td>");
                sb.AppendLine($"</tr>");
            }
        }
        sb.AppendLine("</table>");
        
        // Toplamlar
        sb.AppendLine("<div class='totals'>");
        sb.AppendLine("<div class='totals-box'>");
        sb.AppendLine($"<div class='totals-row'><span>Ara Toplam:</span><span>{fatura.AraToplam:N2} ₺</span></div>");
        sb.AppendLine($"<div class='totals-row'><span>Rüsum:</span><span>{fatura.RusumTutari:N2} ₺</span></div>");
        sb.AppendLine($"<div class='totals-row'><span>Komisyon:</span><span>{fatura.KomisyonTutari:N2} ₺</span></div>");
        sb.AppendLine($"<div class='totals-row'><span>Stopaj:</span><span>{fatura.StopajTutari:N2} ₺</span></div>");
        sb.AppendLine($"<div class='totals-row'><span>KDV:</span><span>{fatura.KdvTutari:N2} ₺</span></div>");
        sb.AppendLine($"<div class='totals-row grand'><span>GENEL TOPLAM:</span><span>{fatura.GenelToplam:N2} ₺</span></div>");
        sb.AppendLine("</div></div>");
        
        // Footer
        sb.AppendLine("<div class='footer'>");
        sb.AppendLine($"<p>Bu belge {DateTime.Now:dd.MM.yyyy HH:mm} tarihinde NeoHal sistemi tarafından oluşturulmuştur.</p>");
        sb.AppendLine("</div>");
        
        sb.AppendLine("</div></body></html>");
        
        return Task.FromResult(sb.ToString());
    }

    public Task<string> GenerateIrsaliyePreviewHtmlAsync(GirisIrsaliyesi irsaliye)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head>");
        sb.AppendLine("<meta charset='UTF-8'>");
        sb.AppendLine("<title>Giriş İrsaliyesi</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(@"
            body { font-family: 'Segoe UI', Arial, sans-serif; margin: 20px; background: #f5f5f5; }
            .irsaliye { background: white; padding: 30px; max-width: 800px; margin: 0 auto; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
            .header { border-bottom: 2px solid #4CAF50; padding-bottom: 20px; margin-bottom: 20px; }
            .header h1 { color: #4CAF50; margin: 0; font-size: 24px; }
            .header .irsaliye-no { color: #666; font-size: 14px; }
            .info-row { display: flex; justify-content: space-between; margin-bottom: 20px; }
            .info-box { flex: 1; padding: 10px; }
            .info-box h3 { margin: 0 0 10px 0; color: #333; font-size: 14px; border-bottom: 1px solid #ddd; padding-bottom: 5px; }
            .info-box p { margin: 5px 0; color: #666; font-size: 12px; }
            table { width: 100%; border-collapse: collapse; margin: 20px 0; }
            th { background: #4CAF50; color: white; padding: 10px; text-align: left; font-size: 12px; }
            td { padding: 10px; border-bottom: 1px solid #eee; font-size: 12px; }
            .text-right { text-align: right; }
            .text-center { text-align: center; }
            .totals { margin-top: 20px; background: #e8f5e9; padding: 15px; }
            .totals-row { display: flex; justify-content: space-between; margin: 5px 0; }
            .totals-row.grand { font-weight: bold; color: #4CAF50; }
            .footer { margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; text-align: center; color: #999; font-size: 10px; }
            @media print { body { margin: 0; background: white; } .irsaliye { box-shadow: none; } }
        ");
        sb.AppendLine("</style></head><body>");
        
        sb.AppendLine("<div class='irsaliye'>");
        
        // Header
        sb.AppendLine("<div class='header'>");
        sb.AppendLine("<h1>GİRİŞ İRSALİYESİ</h1>");
        sb.AppendLine($"<div class='irsaliye-no'>İrsaliye No: {irsaliye.IrsaliyeNo} | Tarih: {irsaliye.Tarih:dd.MM.yyyy}</div>");
        sb.AppendLine("</div>");
        
        // Müstahsil bilgisi
        sb.AppendLine("<div class='info-row'>");
        sb.AppendLine("<div class='info-box'>");
        sb.AppendLine("<h3>MÜSTAHSİL BİLGİLERİ</h3>");
        sb.AppendLine($"<p><strong>{irsaliye.Mustahsil?.Unvan ?? "-"}</strong></p>");
        sb.AppendLine($"<p>Kod: {irsaliye.Mustahsil?.Kod ?? "-"}</p>");
        sb.AppendLine($"<p>TC/Vergi No: {irsaliye.Mustahsil?.TcKimlikNo ?? irsaliye.Mustahsil?.VergiNo ?? "-"}</p>");
        sb.AppendLine("</div>");
        
        sb.AppendLine("<div class='info-box'>");
        sb.AppendLine("<h3>SEVKİYAT BİLGİLERİ</h3>");
        sb.AppendLine($"<p>Plaka: {irsaliye.Plaka ?? "-"}</p>");
        sb.AppendLine($"<p>Künye No: {irsaliye.KunyeNo ?? "-"}</p>");
        sb.AppendLine("</div>");
        sb.AppendLine("</div>");
        
        // Kalemler
        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th>Ürün</th><th>Kap Tipi</th><th class='text-center'>Adet</th><th class='text-right'>Brüt</th><th class='text-right'>Dara</th><th class='text-right'>Net</th></tr>");
        
        if (irsaliye.Kalemler != null)
        {
            foreach (var kalem in irsaliye.Kalemler)
            {
                sb.AppendLine($"<tr>");
                sb.AppendLine($"<td>{kalem.Urun?.Ad ?? "-"}</td>");
                sb.AppendLine($"<td>{kalem.KapTipi?.Ad ?? "-"}</td>");
                sb.AppendLine($"<td class='text-center'>{kalem.KapAdet}</td>");
                sb.AppendLine($"<td class='text-right'>{kalem.BrutKg:N2} kg</td>");
                sb.AppendLine($"<td class='text-right'>{kalem.DaraKg:N2} kg</td>");
                sb.AppendLine($"<td class='text-right'>{kalem.NetKg:N2} kg</td>");
                sb.AppendLine($"</tr>");
            }
        }
        sb.AppendLine("</table>");
        
        // Toplamlar
        sb.AppendLine("<div class='totals'>");
        sb.AppendLine($"<div class='totals-row'><span>Toplam Kap:</span><span>{irsaliye.ToplamKapAdet}</span></div>");
        sb.AppendLine($"<div class='totals-row'><span>Toplam Brüt:</span><span>{irsaliye.ToplamBrut:N2} kg</span></div>");
        sb.AppendLine($"<div class='totals-row'><span>Toplam Dara:</span><span>{irsaliye.ToplamDara:N2} kg</span></div>");
        sb.AppendLine($"<div class='totals-row grand'><span>Toplam Net:</span><span>{irsaliye.ToplamNet:N2} kg</span></div>");
        sb.AppendLine("</div>");
        
        // Footer
        sb.AppendLine("<div class='footer'>");
        sb.AppendLine($"<p>Bu belge {DateTime.Now:dd.MM.yyyy HH:mm} tarihinde NeoHal sistemi tarafından oluşturulmuştur.</p>");
        sb.AppendLine("</div>");
        
        sb.AppendLine("</div></body></html>");
        
        return Task.FromResult(sb.ToString());
    }

    public Task<string> GenerateMustahsilMakbuzuPreviewHtmlAsync(MustahsilMakbuzu makbuz)
    {
        var sb = new StringBuilder();
        
        // Basit bir makbuz şablonu
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head>");
        sb.AppendLine("<meta charset='UTF-8'>");
        sb.AppendLine("<title>Müstahsil Makbuzu</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(@"
            body { font-family: 'Segoe UI', Arial, sans-serif; margin: 20px; }
            .makbuz { background: white; padding: 30px; max-width: 600px; margin: 0 auto; border: 2px solid #333; }
            h1 { text-align: center; color: #333; }
            .info { margin: 20px 0; }
            .row { display: flex; justify-content: space-between; padding: 5px 0; border-bottom: 1px dotted #ccc; }
            .total { font-weight: bold; font-size: 18px; border-top: 2px solid #333; margin-top: 10px; padding-top: 10px; }
        ");
        sb.AppendLine("</style></head><body>");
        
        sb.AppendLine("<div class='makbuz'>");
        sb.AppendLine("<h1>MÜSTAHSİL MAKBUZU</h1>");
        sb.AppendLine($"<p style='text-align:center'>Makbuz No: {makbuz.MakbuzNo} | Tarih: {makbuz.Tarih:dd.MM.yyyy}</p>");
        
        sb.AppendLine("<div class='info'>");
        sb.AppendLine($"<div class='row'><span>Müstahsil:</span><span>{makbuz.Mustahsil?.Unvan ?? "-"}</span></div>");
        sb.AppendLine($"<div class='row'><span>Ürün:</span><span>{makbuz.Urun?.Ad ?? "-"}</span></div>");
        sb.AppendLine($"<div class='row'><span>Miktar:</span><span>{makbuz.Miktar:N2} kg</span></div>");
        sb.AppendLine($"<div class='row'><span>Birim Fiyat:</span><span>{makbuz.BirimFiyat:N2} ₺</span></div>");
        sb.AppendLine($"<div class='row'><span>Brüt Tutar:</span><span>{makbuz.BrutTutar:N2} ₺</span></div>");
        sb.AppendLine($"<div class='row'><span>Stopaj:</span><span>-{makbuz.StopajTutari:N2} ₺</span></div>");
        sb.AppendLine($"<div class='row'><span>Rüsum:</span><span>-{makbuz.RusumTutari:N2} ₺</span></div>");
        sb.AppendLine($"<div class='row'><span>Komisyon:</span><span>-{makbuz.KomisyonTutari:N2} ₺</span></div>");
        sb.AppendLine($"<div class='row total'><span>NET ÖDEME:</span><span>{makbuz.NetOdeme:N2} ₺</span></div>");
        sb.AppendLine("</div>");
        
        sb.AppendLine("</div></body></html>");
        
        return Task.FromResult(sb.ToString());
    }

    public Task PrintDocumentAsync(string htmlContent, string documentTitle)
    {
        // Bu metod platform-specific olacak
        // Şimdilik sadece HTML içeriğini döndürüyoruz
        // Avalonia'da yazdırma için ayrı bir window açılacak
        return Task.CompletedTask;
    }
}
