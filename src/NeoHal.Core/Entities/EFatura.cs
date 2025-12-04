using NeoHal.Core.Common;
using NeoHal.Core.Enums;

namespace NeoHal.Core.Entities;

/// <summary>
/// E-Fatura kaydı - GİB üzerinden gönderilen faturalar
/// </summary>
public class EFatura : SoftDeleteEntity
{
    /// <summary>
    /// UUID - GİB tarafından verilen benzersiz kimlik
    /// </summary>
    public Guid Uuid { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// E-Fatura numarası (örn: ABC2025000000001)
    /// </summary>
    public string FaturaNo { get; set; } = string.Empty;
    
    /// <summary>
    /// İlişkili satış faturası
    /// </summary>
    public Guid? SatisFaturasiId { get; set; }
    
    /// <summary>
    /// Fatura tipi: SATIS, IADE, TEVKIFAT, ISTISNA, OZELMATRAH, IHRAC
    /// </summary>
    public EFaturaTipi FaturaTipi { get; set; } = EFaturaTipi.Satis;
    
    /// <summary>
    /// Fatura senaryosu: TEMEL, TICARI, IHRACAT
    /// </summary>
    public EFaturaSenaryo Senaryo { get; set; } = EFaturaSenaryo.Temel;
    
    /// <summary>
    /// Fatura tarihi
    /// </summary>
    public DateTime FaturaTarihi { get; set; } = DateTime.Today;
    
    /// <summary>
    /// Gönderici VKN/TCKN
    /// </summary>
    public string GondericiVkn { get; set; } = string.Empty;
    
    /// <summary>
    /// Gönderici unvanı
    /// </summary>
    public string GondericiUnvan { get; set; } = string.Empty;
    
    /// <summary>
    /// Alıcı VKN/TCKN
    /// </summary>
    public string AliciVkn { get; set; } = string.Empty;
    
    /// <summary>
    /// Alıcı unvanı
    /// </summary>
    public string AliciUnvan { get; set; } = string.Empty;
    
    /// <summary>
    /// Alıcı e-posta (e-arşiv için)
    /// </summary>
    public string? AliciEposta { get; set; }
    
    /// <summary>
    /// Mal hizmet toplam tutarı (KDV hariç)
    /// </summary>
    public decimal AraToplam { get; set; }
    
    /// <summary>
    /// KDV toplam tutarı
    /// </summary>
    public decimal KdvToplam { get; set; }
    
    /// <summary>
    /// Genel toplam (KDV dahil)
    /// </summary>
    public decimal GenelToplam { get; set; }
    
    /// <summary>
    /// Para birimi
    /// </summary>
    public string ParaBirimi { get; set; } = "TRY";
    
    /// <summary>
    /// Fatura durumu
    /// </summary>
    public EFaturaDurum Durum { get; set; } = EFaturaDurum.Taslak;
    
    /// <summary>
    /// Entegratör tarafından dönen ID
    /// </summary>
    public string? EntegratorId { get; set; }
    
    /// <summary>
    /// GİB'e gönderilme tarihi
    /// </summary>
    public DateTime? GonderimTarihi { get; set; }
    
    /// <summary>
    /// Yanıt tarihi
    /// </summary>
    public DateTime? YanitTarihi { get; set; }
    
    /// <summary>
    /// Yanıt açıklaması (hata veya onay mesajı)
    /// </summary>
    public string? YanitAciklama { get; set; }
    
    /// <summary>
    /// UBL XML içeriği
    /// </summary>
    public string? UblXml { get; set; }
    
    /// <summary>
    /// PDF içeriği (Base64)
    /// </summary>
    public string? PdfBase64 { get; set; }
    
    /// <summary>
    /// İmzalı XML (XSLT dönüşümlü)
    /// </summary>
    public string? ImzaliXml { get; set; }
    
    // Navigation
    public virtual SatisFaturasi? SatisFaturasi { get; set; }
    public virtual ICollection<EFaturaKalem> Kalemler { get; set; } = new List<EFaturaKalem>();
}

/// <summary>
/// E-Fatura kalemi
/// </summary>
public class EFaturaKalem : BaseEntity
{
    public Guid EFaturaId { get; set; }
    
    /// <summary>
    /// Satır numarası
    /// </summary>
    public int SiraNo { get; set; }
    
    /// <summary>
    /// Mal/Hizmet kodu (GTIP, vs)
    /// </summary>
    public string? MalKodu { get; set; }
    
    /// <summary>
    /// Mal/Hizmet adı
    /// </summary>
    public string MalAdi { get; set; } = string.Empty;
    
    /// <summary>
    /// Miktar
    /// </summary>
    public decimal Miktar { get; set; }
    
    /// <summary>
    /// Birim (KGM, LTR, C62, vb.)
    /// </summary>
    public string Birim { get; set; } = "KGM"; // Kilogram
    
    /// <summary>
    /// Birim fiyat
    /// </summary>
    public decimal BirimFiyat { get; set; }
    
    /// <summary>
    /// KDV oranı
    /// </summary>
    public decimal KdvOrani { get; set; } = 1; // %1 KDV (hal ürünleri)
    
    /// <summary>
    /// KDV tutarı
    /// </summary>
    public decimal KdvTutari { get; set; }
    
    /// <summary>
    /// Satır tutarı (KDV hariç)
    /// </summary>
    public decimal Tutar { get; set; }
    
    /// <summary>
    /// Satır toplam (KDV dahil)
    /// </summary>
    public decimal ToplamTutar { get; set; }
    
    // Navigation
    public virtual EFatura EFatura { get; set; } = null!;
}

/// <summary>
/// E-Fatura ayarları
/// </summary>
public class EFaturaAyarlari : BaseEntity
{
    /// <summary>
    /// Entegratör tipi
    /// </summary>
    public EFaturaEntegrator Entegrator { get; set; } = EFaturaEntegrator.Izibiz;
    
    /// <summary>
    /// Test modu aktif mi?
    /// </summary>
    public bool TestModu { get; set; } = true;
    
    /// <summary>
    /// API kullanıcı adı
    /// </summary>
    public string KullaniciAdi { get; set; } = string.Empty;
    
    /// <summary>
    /// API şifre (şifrelenmiş)
    /// </summary>
    public string Sifre { get; set; } = string.Empty;
    
    /// <summary>
    /// Firma VKN
    /// </summary>
    public string FirmaVkn { get; set; } = string.Empty;
    
    /// <summary>
    /// Firma unvanı
    /// </summary>
    public string FirmaUnvan { get; set; } = string.Empty;
    
    /// <summary>
    /// Firma adresi
    /// </summary>
    public string FirmaAdres { get; set; } = string.Empty;
    
    /// <summary>
    /// Firma il
    /// </summary>
    public string FirmaIl { get; set; } = string.Empty;
    
    /// <summary>
    /// Firma ilçe
    /// </summary>
    public string FirmaIlce { get; set; } = string.Empty;
    
    /// <summary>
    /// Firma vergi dairesi
    /// </summary>
    public string FirmaVergiDairesi { get; set; } = string.Empty;
    
    /// <summary>
    /// Firma telefon
    /// </summary>
    public string? FirmaTelefon { get; set; }
    
    /// <summary>
    /// Firma e-posta
    /// </summary>
    public string? FirmaEposta { get; set; }
    
    /// <summary>
    /// Posta kutusu etiketi (GB)
    /// </summary>
    public string? PostaKutusuEtiketi { get; set; }
    
    /// <summary>
    /// Son seri numarası (otomatik artırım için)
    /// </summary>
    public int SonSeriNo { get; set; } = 0;
    
    /// <summary>
    /// Seri ön eki (örn: ABC)
    /// </summary>
    public string SeriOnEki { get; set; } = "HAL";
}
