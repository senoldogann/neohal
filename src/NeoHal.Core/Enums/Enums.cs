namespace NeoHal.Core.Enums;

/// <summary>
/// Belge durumları
/// </summary>
public enum BelgeDurumu
{
    Taslak = 0,
    Beklemede = 1,
    Onaylandi = 2,
    Iptal = 3,
    HksGonderildi = 4,
    HksOnaylandi = 5,
    HksHatali = 6
}

/// <summary>
/// Ödeme durumları
/// </summary>
public enum OdemeDurumu
{
    Odenmedi = 0,
    KismiOdendi = 1,
    Odendi = 2
}

/// <summary>
/// Kasa hareket tipleri
/// </summary>
public enum KasaHareketTipi
{
    GirisDolu = 1,    // Müstahsilden dolu kasa girişi
    CikisDolu = 2,    // Alıcıya dolu kasa çıkışı
    GirisBos = 3,     // Boş kasa iadesi alındı
    CikisBos = 4,     // Boş kasa iadesi verildi
    RehinAl = 5,      // Rehin bedeli alındı
    RehinIade = 6     // Rehin bedeli iade edildi
}

/// <summary>
/// Cari hareket tipleri
/// </summary>
public enum CariHareketTipi
{
    Borc = 1,
    Alacak = 2,
    Tahsilat = 3,
    Odeme = 4
}

/// <summary>
/// Ödeme türleri
/// </summary>
public enum OdemeTuru
{
    Nakit = 1,
    Havale = 2,
    Cek = 3,
    Senet = 4,
    KrediKarti = 5
}

/// <summary>
/// Sync durumları (offline-first için)
/// </summary>
public enum SyncDurumu
{
    Beklemede = 0,
    Senkronize = 1,
    Cakisma = 2,
    Hata = 3
}

/// <summary>
/// Birim türleri
/// </summary>
public enum BirimTuru
{
    Kilogram = 1,
    Adet = 2,
    Koli = 3
}

/// <summary>
/// Fatura tipi - Satış faturasının türünü belirler
/// Her tip farklı kesinti kuralları uygular
/// </summary>
public enum FaturaTipi
{
    /// <summary>
    /// Perakende Satış
    /// Alıcı: Manav, Market, Son tüketici
    /// Kesintiler: Komisyon, Rüsum, Stopaj uygulanabilir
    /// </summary>
    Perakende = 1,
    
    /// <summary>
    /// Toptan Satış (Komisyoncu → Alıcı/Tüccar)
    /// Alıcı: Tüccar, Toptancı, Zincir Market
    /// Kesintiler: Komisyon, Rüsum, Stopaj uygulanır
    /// </summary>
    Toptan = 2,
    
    /// <summary>
    /// Sevkiyat Satış (Sevkiyatçı → Şube/Müşteri)
    /// Alıcı: Sevkiyatçının kendi şubeleri veya müşterileri
    /// Kesintiler: HİÇBİR KESİNTİ UYGULANMAZ
    /// Sadece alış-satış fiyat farkından kar elde edilir
    /// </summary>
    Sevkiyat = 3,
    
    /// <summary>
    /// İç Transfer (Şubeler Arası)
    /// Alıcı: Aynı firmanın farklı şubeleri
    /// Kesintiler: Kesinti uygulanmaz
    /// Maliyet fiyatı üzerinden transfer edilir
    /// </summary>
    IcTransfer = 4
}

/// <summary>
/// Çek/Senet durumu
/// </summary>
public enum CekSenetDurumu
{
    Beklemede = 0,
    TahsilEdildi = 1,
    Iade = 2,
    KarsilikYok = 3,
    Ciro = 4
}

/// <summary>
/// Cari tipi detay (Sektöre Özel)
/// </summary>
public enum CariTipiDetay
{
    Mustahsil = 1,        // Üretici
    Kabzimal = 2,         // Komisyoncu
    Sevkiyatci = 3,       // Nakliyeci
    Tuccar = 4,           // Toptancı
    MarketZinciri = 5,    // Kurumsal Alıcı
    ManavDukkan = 6,      // Perakendeci
    Ihracatci = 7,        // İhracatçı
    HalIciKomisyoncu = 8  // Hal içi aracı
}

/// <summary>
/// Kesinti tipi
/// </summary>
public enum KesintTipi
{
    Stopaj = 1,       // %11 Gelir Vergisi Stopajı
    Rusum = 2,        // Belediye Rüsumu %1-2
    Komisyon = 3,     // Komisyoncu Payı %8-12
    Hamaliye = 4,     // Yükleme/Boşaltma ücreti
    Navlun = 5,       // Nakliye ücreti
    Bagkur = 6,       // SGK Kesintisi
    DigerKesinti = 99
}

/// <summary>
/// Hesaplama tipi
/// </summary>
public enum HesaplamaTipi
{
    Yuzde = 1,
    SabitBirimBasi = 2,  // Kasa başına sabit
    SabitKiloBasi = 3,   // Kilo başına sabit
    SabitToplam = 4      // Toplam sabit tutar
}

/// <summary>
/// HKS Bildirim durumu
/// </summary>
public enum HksBildirimDurumu
{
    Beklemede = 0,
    Gonderiliyor = 1,
    Gonderildi = 2,
    Basarili = 3,
    Hatali = 4,
    Reddedildi = 5,
    IptalEdildi = 6
}

/// <summary>
/// e-Belge durumu
/// </summary>
public enum EBelgeDurumu
{
    Taslak = 0,
    Olusturuldu = 1,
    Gonderildi = 2,
    Onaylandi = 3,
    Reddedildi = 4,
    IptalEdildi = 5
}

/// <summary>
/// Çek/Senet hareket tipi
/// </summary>
public enum CekSenetHareketTipi
{
    Giris = 1,              // Portföye giriş
    TahsileGonder = 2,      // Bankaya tahsile verildi
    TahsilEdildi = 3,       // Banka tahsil etti
    CiroEt = 4,             // Müstahsile/Başkasına ciro
    CiroDonduruldu = 5,     // Ciro edilen geri döndü
    Karsilikisiz = 6,       // Karşılıksız çıktı
    IadeAlindi = 7,         // İade alındı
    ProtestoEdildi = 8      // Protesto edildi
}

/// <summary>
/// E-Fatura tipi
/// </summary>
public enum EFaturaTipi
{
    Satis = 1,        // Satış faturası
    Iade = 2,         // İade faturası
    Tevkifat = 3,     // Tevkifatlı fatura
    Istisna = 4,      // İstisna faturası
    OzelMatrah = 5,   // Özel matrah faturası
    Ihrac = 6         // İhracat faturası
}

/// <summary>
/// E-Fatura senaryosu
/// </summary>
public enum EFaturaSenaryo
{
    Temel = 1,        // Temel fatura (kabul/red yok)
    Ticari = 2,       // Ticari fatura (kabul/red var)
    Ihracat = 3       // İhracat faturası
}

/// <summary>
/// E-Fatura durumu
/// </summary>
public enum EFaturaDurum
{
    Taslak = 0,           // Henüz gönderilmedi
    Kuyrukta = 1,         // Gönderim kuyruğunda
    Gonderildi = 2,       // Entegratöre gönderildi
    GibOnayinda = 3,      // GİB onayı bekleniyor
    Basarili = 4,         // Başarıyla iletildi
    Reddedildi = 5,       // Alıcı tarafından reddedildi
    Hata = 6,             // Hata oluştu
    IptalEdildi = 7       // İptal edildi
}

/// <summary>
/// E-Fatura entegratörü
/// </summary>
public enum EFaturaEntegrator
{
    Izibiz = 1,
    Uyumsoft = 2,
    Mikrokom = 3,
    EFinans = 4,
    Logo = 5,
    Nes = 6
}
