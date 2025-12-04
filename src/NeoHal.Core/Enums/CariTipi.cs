namespace NeoHal.Core.Enums;

/// <summary>
/// Cari hesap tiplerini tanımlar
/// 
/// Hal sektöründe temel iş akışı:
/// 1. Müstahsil → Komisyoncu'ya mal getirir (GirisIrsaliyesi)
/// 2. Komisyoncu → Sevkiyatçı'ya/Alıcı'ya satar (SatisFaturasi - kesintili)
/// 3. Sevkiyatçı → Şube'ye satar (SatisFaturasi - kesintisiz)
/// </summary>
public enum CariTipi
{
    /// <summary>
    /// Üretici - Mal getiren çiftçi/üretici
    /// Komisyoncuya mal getirir, karşılığında stopaj kesilerek ödeme alır
    /// </summary>
    Mustahsil = 1,
    
    /// <summary>
    /// Komisyoncu - Hal içindeki aracı satıcı
    /// Müstahsilden mal alır, alıcılara/sevkiyatçılara satar
    /// Kesintiler uygular: Stopaj, Rüsum, Komisyon, Hamaliye vs.
    /// </summary>
    Komisyoncu = 2,
    
    /// <summary>
    /// Sevkiyatçı - Dağıtımcı/Toptancı
    /// Komisyoncudan toptan alır, kendi şubelerine/müşterilerine satar
    /// Kesinti UYGULAMAZ - sadece alış ve satış fiyatı farkından kazanır
    /// </summary>
    Sevkiyatci = 3,
    
    /// <summary>
    /// Alıcı - Mal alan dükkan/market/manav
    /// Komisyoncudan veya sevkiyatçıdan mal satın alır
    /// </summary>
    Alici = 4,
    
    /// <summary>
    /// Nakliyeci - Taşımacı firma/şahıs
    /// Navlun (nakliye) ücreti alır
    /// </summary>
    Nakliyeci = 5,
    
    /// <summary>
    /// Şube - Sevkiyatçının kendi dükkanları/noktaları
    /// Sevkiyatçıya bağlı alt nokta, AnaCariId ile ilişkilendirilir
    /// İç transferlerde kullanılır
    /// </summary>
    Sube = 6
}
