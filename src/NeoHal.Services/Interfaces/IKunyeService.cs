using System;
using System.Threading.Tasks;
using NeoHal.Core.Entities;

namespace NeoHal.Services.Interfaces;

/// <summary>
/// Künye Yönetim Servisi
/// </summary>
public interface IKunyeService
{
    /// <summary>
    /// Giriş irsaliyesi için künye oluşturur ve HKS'ye bildirir
    /// </summary>
    Task<string> GirisIrsaliyesiIcinKunyeOlusturAsync(GirisIrsaliyesi irsaliye);

    /// <summary>
    /// Satış faturası için bildirim yapar
    /// </summary>
    Task SatisBildirimiYapAsync(SatisFaturasi fatura);
}
