using System;
using System.Threading.Tasks;
using NeoHal.Core.Entities;

namespace NeoHal.Services.Interfaces;

/// <summary>
/// HKS (Hal Kayıt Sistemi) Entegrasyon Servisi
/// </summary>
public interface IHksService
{
    /// <summary>
    /// HKS'ye bildirim gönderir (Giriş/Satış)
    /// </summary>
    Task<HksBildirimSonuc> BildirimYapAsync(HksBildirim bildirim);

    /// <summary>
    /// Ürün için yeni künye numarası alır
    /// </summary>
    Task<string> KunyeAlAsync(Guid urunId, decimal miktar, string plaka);

    /// <summary>
    /// HKS'den stok sorgular
    /// </summary>
    Task<bool> StokSorgulaAsync(string kunyeNo);
}

public class HksBildirimSonuc
{
    public bool Basarili { get; set; }
    public string? HataMesaji { get; set; }
    public string? KunyeNo { get; set; }
    public string? ReferansNo { get; set; }
}
