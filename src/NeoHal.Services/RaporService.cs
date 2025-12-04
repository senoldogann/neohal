using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NeoHal.Core.Entities;
using NeoHal.Data.Context;
using NeoHal.Services.Interfaces;

namespace NeoHal.Services;

/// <summary>
/// Rapor oluşturma ve dışa aktarma servisi implementasyonu
/// </summary>
public class RaporService : IRaporService
{
    private readonly NeoHalDbContext _context;
    private readonly ILogger<RaporService>? _logger;
    private readonly List<RaporSablonu> _sablonlar = new();
    private readonly List<ZamanlanmisRapor> _zamanlanmisRaporlar = new();

    public RaporService(NeoHalDbContext context, ILogger<RaporService>? logger = null)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<RaporSonucu> GunlukSatisRaporuAsync(DateTime tarih, RaporFiltresi? filtre = null, CancellationToken cancellationToken = default)
    {
        return await SatisRaporuAsync(tarih.Date, tarih.Date.AddDays(1).AddSeconds(-1), filtre, cancellationToken);
    }

    public async Task<RaporSonucu> SatisRaporuAsync(DateTime baslangic, DateTime bitis, RaporFiltresi? filtre = null, CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Satış raporu oluşturuluyor: {Baslangic} - {Bitis}", baslangic, bitis);

        var rapor = new RaporSonucu
        {
            RaporAdi = $"Satış Raporu ({baslangic:dd.MM.yyyy} - {bitis:dd.MM.yyyy})",
            Tur = RaporTuru.DonemselSatis,
            BaslangicTarihi = baslangic,
            BitisTarihi = bitis,
            Filtre = filtre
        };

        // Sütunları tanımla
        rapor.Sutunlar = new List<RaporSutunu>
        {
            new() { Anahtar = "FaturaNo", Baslik = "Fatura No", Tip = SutunTipi.Metin, Genislik = 100 },
            new() { Anahtar = "Tarih", Baslik = "Tarih", Tip = SutunTipi.Tarih, Genislik = 100 },
            new() { Anahtar = "CariUnvan", Baslik = "Müşteri", Tip = SutunTipi.Metin, Genislik = 200 },
            new() { Anahtar = "UrunAdi", Baslik = "Ürün", Tip = SutunTipi.Metin, Genislik = 150 },
            new() { Anahtar = "Miktar", Baslik = "Miktar", Tip = SutunTipi.Sayi, Genislik = 80, Toplanabilir = true },
            new() { Anahtar = "BirimFiyat", Baslik = "Birim Fiyat", Tip = SutunTipi.ParaBirimi, Genislik = 100 },
            new() { Anahtar = "Tutar", Baslik = "Tutar", Tip = SutunTipi.ParaBirimi, Genislik = 120, Toplanabilir = true }
        };

        // Faturaları sorgula
        var query = _context.SatisFaturalari
            .Include(f => f.Alici)
            .Include(f => f.Kalemler)
                .ThenInclude(k => k.Urun)
            .Where(f => f.FaturaTarihi >= baslangic && f.FaturaTarihi <= bitis);

        // Filtre uygula
        if (filtre?.CariHesapId != null)
        {
            query = query.Where(f => f.AliciId == filtre.CariHesapId);
        }

        var faturalar = await query.ToListAsync(cancellationToken);

        decimal toplamTutar = 0;
        decimal toplamKdv = 0;
        decimal toplamMiktar = 0;

        foreach (var fatura in faturalar)
        {
            foreach (var kalem in fatura.Kalemler)
            {
                rapor.Satirlar.Add(new RaporSatiri
                {
                    Degerler = new Dictionary<string, object?>
                    {
                        ["FaturaNo"] = fatura.FaturaNo,
                        ["Tarih"] = fatura.FaturaTarihi,
                        ["CariUnvan"] = fatura.Alici?.Unvan ?? "-",
                        ["UrunAdi"] = kalem.Urun?.Ad ?? "-",
                        ["Miktar"] = kalem.NetKg,
                        ["BirimFiyat"] = kalem.BirimFiyat,
                        ["Tutar"] = kalem.Tutar
                    }
                });

                toplamMiktar += kalem.NetKg;
                toplamTutar += kalem.Tutar;
            }
            toplamKdv += fatura.KdvTutari;
        }

        // Özet bilgiler
        rapor.Ozet = new RaporOzet
        {
            ToplamKayit = rapor.Satirlar.Count,
            ToplamTutar = toplamTutar,
            ToplamKdv = toplamKdv,
            ToplamNet = toplamTutar + toplamKdv,
            ToplamMiktar = toplamMiktar,
            EkBilgiler = new Dictionary<string, object>
            {
                ["FaturaSayisi"] = faturalar.Count
            }
        };

        // Günlük satış grafiği için veri
        var gunlukSatislar = faturalar
            .GroupBy(f => f.FaturaTarihi.Date)
            .Select(g => new GrafikVerisi
            {
                Etiket = g.Key.ToString("dd.MM"),
                Deger = g.Sum(f => f.GenelToplam),
                Renk = "#3B82F6"
            })
            .OrderBy(g => g.Etiket)
            .ToList();

        rapor.GrafikVerileri = gunlukSatislar;

        return rapor;
    }

    public async Task<RaporSonucu> CariExtreRaporuAsync(Guid cariId, DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Cari ekstre raporu oluşturuluyor: CariId={CariId}", cariId);

        var cariHesap = await _context.CariHesaplar.FindAsync(new object[] { cariId }, cancellationToken);
        
        var rapor = new RaporSonucu
        {
            RaporAdi = $"Cari Ekstre - {cariHesap?.Unvan ?? "Bilinmiyor"}",
            Tur = RaporTuru.CariExtre,
            BaslangicTarihi = baslangic,
            BitisTarihi = bitis
        };

        rapor.Sutunlar = new List<RaporSutunu>
        {
            new() { Anahtar = "Tarih", Baslik = "Tarih", Tip = SutunTipi.Tarih, Genislik = 100 },
            new() { Anahtar = "Aciklama", Baslik = "Açıklama", Tip = SutunTipi.Metin, Genislik = 250 },
            new() { Anahtar = "Borc", Baslik = "Borç", Tip = SutunTipi.ParaBirimi, Genislik = 120, Toplanabilir = true },
            new() { Anahtar = "Alacak", Baslik = "Alacak", Tip = SutunTipi.ParaBirimi, Genislik = 120, Toplanabilir = true },
            new() { Anahtar = "Bakiye", Baslik = "Bakiye", Tip = SutunTipi.ParaBirimi, Genislik = 120 }
        };

        var hareketler = await _context.CariHareketler
            .Where(h => h.CariId == cariId && h.Tarih >= baslangic && h.Tarih <= bitis)
            .OrderBy(h => h.Tarih)
            .ToListAsync(cancellationToken);

        decimal bakiye = 0;
        decimal toplamBorc = 0;
        decimal toplamAlacak = 0;

        foreach (var hareket in hareketler)
        {
            // CariHareketTipi'ne göre borç/alacak belirle
            var borc = hareket.HareketTipi == Core.Enums.CariHareketTipi.Borc ? hareket.Tutar : 0;
            var alacak = hareket.HareketTipi == Core.Enums.CariHareketTipi.Alacak ? hareket.Tutar : 0;
            
            bakiye += borc - alacak;
            toplamBorc += borc;
            toplamAlacak += alacak;

            rapor.Satirlar.Add(new RaporSatiri
            {
                Degerler = new Dictionary<string, object?>
                {
                    ["Tarih"] = hareket.Tarih,
                    ["Aciklama"] = hareket.Aciklama,
                    ["Borc"] = borc,
                    ["Alacak"] = alacak,
                    ["Bakiye"] = bakiye
                }
            });
        }

        rapor.Ozet = new RaporOzet
        {
            ToplamKayit = hareketler.Count,
            ToplamTutar = toplamBorc - toplamAlacak,
            EkBilgiler = new Dictionary<string, object>
            {
                ["ToplamBorc"] = toplamBorc,
                ["ToplamAlacak"] = toplamAlacak,
                ["Bakiye"] = bakiye,
                ["CariUnvan"] = cariHesap?.Unvan ?? "-"
            }
        };

        return rapor;
    }

    public async Task<RaporSonucu> UrunSatisRaporuAsync(DateTime baslangic, DateTime bitis, Guid? urunId = null, CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Ürün satış raporu oluşturuluyor");

        var rapor = new RaporSonucu
        {
            RaporAdi = $"Ürün Satış Raporu ({baslangic:dd.MM.yyyy} - {bitis:dd.MM.yyyy})",
            Tur = RaporTuru.UrunSatis,
            BaslangicTarihi = baslangic,
            BitisTarihi = bitis
        };

        rapor.Sutunlar = new List<RaporSutunu>
        {
            new() { Anahtar = "UrunKodu", Baslik = "Kod", Tip = SutunTipi.Metin, Genislik = 80 },
            new() { Anahtar = "UrunAdi", Baslik = "Ürün Adı", Tip = SutunTipi.Metin, Genislik = 200 },
            new() { Anahtar = "ToplamMiktar", Baslik = "Toplam Miktar", Tip = SutunTipi.Sayi, Genislik = 120, Toplanabilir = true },
            new() { Anahtar = "OrtalamaBirimFiyat", Baslik = "Ort. Fiyat", Tip = SutunTipi.ParaBirimi, Genislik = 100 },
            new() { Anahtar = "ToplamTutar", Baslik = "Toplam Tutar", Tip = SutunTipi.ParaBirimi, Genislik = 150, Toplanabilir = true }
        };

        var query = _context.SatisFaturasiKalemleri
            .Include(k => k.Urun)
            .Include(k => k.Fatura)
            .Where(k => k.Fatura != null && k.Fatura.FaturaTarihi >= baslangic && k.Fatura.FaturaTarihi <= bitis);

        if (urunId.HasValue)
        {
            query = query.Where(k => k.UrunId == urunId);
        }

        var urunGruplanmis = await query
            .GroupBy(k => new { k.UrunId, k.Urun!.Kod, k.Urun.Ad })
            .Select(g => new
            {
                g.Key.UrunId,
                g.Key.Kod,
                g.Key.Ad,
                ToplamMiktar = g.Sum(k => k.NetKg),
                ToplamTutar = g.Sum(k => k.Tutar),
                OrtalamaBirimFiyat = g.Average(k => k.BirimFiyat)
            })
            .OrderByDescending(x => x.ToplamTutar)
            .ToListAsync(cancellationToken);

        decimal genelToplamMiktar = 0;
        decimal genelToplamTutar = 0;

        foreach (var urun in urunGruplanmis)
        {
            rapor.Satirlar.Add(new RaporSatiri
            {
                Degerler = new Dictionary<string, object?>
                {
                    ["UrunKodu"] = urun.Kod,
                    ["UrunAdi"] = urun.Ad,
                    ["ToplamMiktar"] = urun.ToplamMiktar,
                    ["OrtalamaBirimFiyat"] = urun.OrtalamaBirimFiyat,
                    ["ToplamTutar"] = urun.ToplamTutar
                }
            });

            genelToplamMiktar += urun.ToplamMiktar;
            genelToplamTutar += urun.ToplamTutar;
        }

        rapor.Ozet = new RaporOzet
        {
            ToplamKayit = urunGruplanmis.Count,
            ToplamTutar = genelToplamTutar,
            ToplamMiktar = genelToplamMiktar
        };

        // En çok satan ürünler grafiği
        rapor.GrafikVerileri = urunGruplanmis
            .Take(10)
            .Select(u => new GrafikVerisi
            {
                Etiket = u.Ad?.Length > 15 ? u.Ad.Substring(0, 15) + "..." : u.Ad ?? "-",
                Deger = u.ToplamTutar,
                Renk = "#10B981"
            })
            .ToList();

        return rapor;
    }

    public async Task<RaporSonucu> StokDurumuRaporuAsync(DateTime? tarih = null, CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Kasa stok durumu raporu oluşturuluyor");

        var rapor = new RaporSonucu
        {
            RaporAdi = $"Kasa Stok Durumu Raporu ({(tarih ?? DateTime.Today):dd.MM.yyyy})",
            Tur = RaporTuru.StokDurumu,
            BaslangicTarihi = tarih ?? DateTime.Today,
            BitisTarihi = tarih ?? DateTime.Today
        };

        rapor.Sutunlar = new List<RaporSutunu>
        {
            new() { Anahtar = "CariAdi", Baslik = "Cari Adı", Tip = SutunTipi.Metin, Genislik = 200 },
            new() { Anahtar = "KapTipi", Baslik = "Kap Tipi", Tip = SutunTipi.Metin, Genislik = 150 },
            new() { Anahtar = "DoluKasa", Baslik = "Dolu Kasa", Tip = SutunTipi.Sayi, Genislik = 100, Toplanabilir = true },
            new() { Anahtar = "BosKasa", Baslik = "Boş Kasa", Tip = SutunTipi.Sayi, Genislik = 100, Toplanabilir = true },
            new() { Anahtar = "Rehin", Baslik = "Rehin Bedeli", Tip = SutunTipi.ParaBirimi, Genislik = 120, Toplanabilir = true }
        };

        var stoklar = await _context.KasaStokDurumlari
            .Include(s => s.Cari)
            .Include(s => s.KapTipi)
            .Where(s => s.DoluKasaAdet > 0 || s.BosKasaAdet > 0)
            .OrderBy(s => s.Cari!.Unvan)
            .ToListAsync(cancellationToken);

        int toplamDolu = 0;
        int toplamBos = 0;
        decimal toplamRehin = 0;

        foreach (var stok in stoklar)
        {
            rapor.Satirlar.Add(new RaporSatiri
            {
                Degerler = new Dictionary<string, object?>
                {
                    ["CariAdi"] = stok.Cari?.Unvan,
                    ["KapTipi"] = stok.KapTipi?.Ad,
                    ["DoluKasa"] = stok.DoluKasaAdet,
                    ["BosKasa"] = stok.BosKasaAdet,
                    ["Rehin"] = stok.RehinToplam
                }
            });

            toplamDolu += stok.DoluKasaAdet;
            toplamBos += stok.BosKasaAdet;
            toplamRehin += stok.RehinToplam;
        }

        rapor.Ozet = new RaporOzet
        {
            ToplamKayit = stoklar.Count,
            ToplamMiktar = toplamDolu + toplamBos,
            ToplamTutar = toplamRehin
        };

        return rapor;
    }

    public async Task<RaporSonucu> KasaRaporuAsync(DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Kasa raporu oluşturuluyor");

        var rapor = new RaporSonucu
        {
            RaporAdi = $"Kasa Raporu ({baslangic:dd.MM.yyyy} - {bitis:dd.MM.yyyy})",
            Tur = RaporTuru.KasaHareket,
            BaslangicTarihi = baslangic,
            BitisTarihi = bitis
        };

        rapor.Sutunlar = new List<RaporSutunu>
        {
            new() { Anahtar = "Tarih", Baslik = "Tarih", Tip = SutunTipi.Tarih, Genislik = 100 },
            new() { Anahtar = "HareketTuru", Baslik = "Hareket Türü", Tip = SutunTipi.Metin, Genislik = 120 },
            new() { Anahtar = "Aciklama", Baslik = "Açıklama", Tip = SutunTipi.Metin, Genislik = 200 },
            new() { Anahtar = "Giris", Baslik = "Giriş", Tip = SutunTipi.ParaBirimi, Genislik = 120, Toplanabilir = true },
            new() { Anahtar = "Cikis", Baslik = "Çıkış", Tip = SutunTipi.ParaBirimi, Genislik = 120, Toplanabilir = true },
            new() { Anahtar = "Bakiye", Baslik = "Bakiye", Tip = SutunTipi.ParaBirimi, Genislik = 120 }
        };

        var hareketler = await _context.KasaHesabi
            .Where(k => k.Tarih >= baslangic && k.Tarih <= bitis)
            .OrderBy(k => k.Tarih)
            .ToListAsync(cancellationToken);

        decimal bakiye = 0;
        decimal toplamGiris = 0;
        decimal toplamCikis = 0;

        foreach (var hareket in hareketler)
        {
            var giris = hareket.GirisHareketi ? hareket.Tutar : 0;
            var cikis = !hareket.GirisHareketi ? hareket.Tutar : 0;
            bakiye += giris - cikis;
            toplamGiris += giris;
            toplamCikis += cikis;

            rapor.Satirlar.Add(new RaporSatiri
            {
                Degerler = new Dictionary<string, object?>
                {
                    ["Tarih"] = hareket.Tarih,
                    ["HareketTuru"] = hareket.GirisHareketi ? "Giriş" : "Çıkış",
                    ["Aciklama"] = hareket.Aciklama,
                    ["Giris"] = giris,
                    ["Cikis"] = cikis,
                    ["Bakiye"] = bakiye
                }
            });
        }

        rapor.Ozet = new RaporOzet
        {
            ToplamKayit = hareketler.Count,
            ToplamTutar = bakiye,
            EkBilgiler = new Dictionary<string, object>
            {
                ["ToplamGiris"] = toplamGiris,
                ["ToplamCikis"] = toplamCikis
            }
        };

        return rapor;
    }

    public async Task<RaporSonucu> MusteriAnalizRaporuAsync(DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Müşteri analiz raporu oluşturuluyor");

        var rapor = new RaporSonucu
        {
            RaporAdi = $"Müşteri Analiz Raporu ({baslangic:dd.MM.yyyy} - {bitis:dd.MM.yyyy})",
            Tur = RaporTuru.MusteriAnaliz,
            BaslangicTarihi = baslangic,
            BitisTarihi = bitis
        };

        rapor.Sutunlar = new List<RaporSutunu>
        {
            new() { Anahtar = "CariUnvan", Baslik = "Müşteri", Tip = SutunTipi.Metin, Genislik = 200 },
            new() { Anahtar = "FaturaSayisi", Baslik = "Fatura Sayısı", Tip = SutunTipi.Sayi, Genislik = 100, Toplanabilir = true },
            new() { Anahtar = "ToplamSatis", Baslik = "Toplam Satış", Tip = SutunTipi.ParaBirimi, Genislik = 150, Toplanabilir = true },
            new() { Anahtar = "OrtalamaFatura", Baslik = "Ort. Fatura", Tip = SutunTipi.ParaBirimi, Genislik = 120 }
        };

        var musteriAnaliz = await _context.SatisFaturalari
            .Include(f => f.Alici)
            .Where(f => f.FaturaTarihi >= baslangic && f.FaturaTarihi <= bitis)
            .GroupBy(f => new { f.AliciId, f.Alici!.Unvan })
            .Select(g => new
            {
                g.Key.AliciId,
                g.Key.Unvan,
                FaturaSayisi = g.Count(),
                ToplamSatis = g.Sum(f => f.GenelToplam)
            })
            .OrderByDescending(x => x.ToplamSatis)
            .ToListAsync(cancellationToken);

        decimal toplamSatis = 0;
        int toplamFatura = 0;

        foreach (var musteri in musteriAnaliz)
        {
            rapor.Satirlar.Add(new RaporSatiri
            {
                Degerler = new Dictionary<string, object?>
                {
                    ["CariUnvan"] = musteri.Unvan,
                    ["FaturaSayisi"] = musteri.FaturaSayisi,
                    ["ToplamSatis"] = musteri.ToplamSatis,
                    ["OrtalamaFatura"] = musteri.FaturaSayisi > 0 ? musteri.ToplamSatis / musteri.FaturaSayisi : 0
                }
            });

            toplamSatis += musteri.ToplamSatis;
            toplamFatura += musteri.FaturaSayisi;
        }

        rapor.Ozet = new RaporOzet
        {
            ToplamKayit = musteriAnaliz.Count,
            ToplamTutar = toplamSatis,
            EkBilgiler = new Dictionary<string, object>
            {
                ["ToplamFaturaSayisi"] = toplamFatura
            }
        };

        // Top 10 müşteri grafiği
        rapor.GrafikVerileri = musteriAnaliz
            .Take(10)
            .Select(m => new GrafikVerisi
            {
                Etiket = m.Unvan?.Length > 15 ? m.Unvan.Substring(0, 15) + "..." : m.Unvan ?? "-",
                Deger = m.ToplamSatis,
                Renk = "#8B5CF6"
            })
            .ToList();

        return rapor;
    }

    public async Task<RaporSonucu> UrunAnalizRaporuAsync(DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default)
    {
        return await UrunSatisRaporuAsync(baslangic, bitis, null, cancellationToken);
    }

    public async Task<RaporSonucu> HksBildirimRaporuAsync(DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("HKS bildirim raporu oluşturuluyor");

        var rapor = new RaporSonucu
        {
            RaporAdi = $"HKS Bildirim Raporu ({baslangic:dd.MM.yyyy} - {bitis:dd.MM.yyyy})",
            Tur = RaporTuru.HksBildirim,
            BaslangicTarihi = baslangic,
            BitisTarihi = bitis
        };

        rapor.Sutunlar = new List<RaporSutunu>
        {
            new() { Anahtar = "Tarih", Baslik = "Tarih", Tip = SutunTipi.Tarih, Genislik = 100 },
            new() { Anahtar = "BildirimTipi", Baslik = "Bildirim Tipi", Tip = SutunTipi.Metin, Genislik = 120 },
            new() { Anahtar = "KunyeNo", Baslik = "Künye No", Tip = SutunTipi.Metin, Genislik = 120 },
            new() { Anahtar = "Durum", Baslik = "Durum", Tip = SutunTipi.Metin, Genislik = 100 }
        };

        var bildirimler = await _context.HksBildirimleri
            .Where(b => b.OlusturmaTarihi >= baslangic && b.OlusturmaTarihi <= bitis)
            .OrderByDescending(b => b.OlusturmaTarihi)
            .ToListAsync(cancellationToken);

        foreach (var bildirim in bildirimler)
        {
            rapor.Satirlar.Add(new RaporSatiri
            {
                Degerler = new Dictionary<string, object?>
                {
                    ["Tarih"] = bildirim.OlusturmaTarihi,
                    ["BildirimTipi"] = bildirim.BildirimTipi,
                    ["KunyeNo"] = bildirim.HksKunyeNo ?? "-",
                    ["Durum"] = bildirim.Durum.ToString()
                }
            });
        }

        rapor.Ozet = new RaporOzet
        {
            ToplamKayit = bildirimler.Count,
            EkBilgiler = new Dictionary<string, object>
            {
                ["BasariliSayisi"] = bildirimler.Count(b => b.Durum == Core.Enums.HksBildirimDurumu.Basarili),
                ["BekleyenSayisi"] = bildirimler.Count(b => b.Durum == Core.Enums.HksBildirimDurumu.Beklemede),
                ["HataliSayisi"] = bildirimler.Count(b => b.Durum == Core.Enums.HksBildirimDurumu.Hatali)
            }
        };

        return rapor;
    }

    public Task<byte[]> ExportToPdfAsync(RaporSonucu rapor, CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("PDF export: {RaporAdi}", rapor.RaporAdi);
        
        // PDF oluşturma - gerçek implementasyon için iTextSharp veya QuestPDF gerekir
        // Şimdilik basit bir text rapor
        var sb = new StringBuilder();
        sb.AppendLine("==================================================");
        sb.AppendLine($"  {rapor.RaporAdi}");
        sb.AppendLine("==================================================");
        sb.AppendLine($"  Oluşturma Tarihi: {rapor.OlusturmaTarihi:dd.MM.yyyy HH:mm}");
        sb.AppendLine($"  Dönem: {rapor.BaslangicTarihi:dd.MM.yyyy} - {rapor.BitisTarihi:dd.MM.yyyy}");
        sb.AppendLine($"  Toplam Kayıt: {rapor.Ozet.ToplamKayit}");
        sb.AppendLine($"  Toplam Tutar: {rapor.Ozet.ToplamTutar:N2} TL");
        sb.AppendLine("==================================================");
        sb.AppendLine();
        
        // Başlıklar
        sb.AppendLine(string.Join("\t", rapor.Sutunlar.Select(s => s.Baslik)));
        sb.AppendLine(new string('-', 100));
        
        // Veriler
        foreach (var satir in rapor.Satirlar)
        {
            var degerler = rapor.Sutunlar.Select(s => 
                satir.Degerler.TryGetValue(s.Anahtar, out var d) 
                    ? FormatValue(d, s.Tip) 
                    : "-");
            sb.AppendLine(string.Join("\t", degerler));
        }
        
        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    public Task<byte[]> ExportToExcelAsync(RaporSonucu rapor, CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Excel export: {RaporAdi}", rapor.RaporAdi);
        
        // Excel oluşturma - gerçek implementasyon için EPPlus veya ClosedXML gerekir
        // Şimdilik TSV (Tab-Separated Values) formatında
        var sb = new StringBuilder();
        
        // Başlık satırı
        sb.AppendLine(string.Join("\t", rapor.Sutunlar.Select(s => s.Baslik)));
        
        // Veri satırları
        foreach (var satir in rapor.Satirlar)
        {
            var degerler = rapor.Sutunlar.Select(s => 
                satir.Degerler.TryGetValue(s.Anahtar, out var d) 
                    ? FormatValue(d, s.Tip) 
                    : "");
            sb.AppendLine(string.Join("\t", degerler));
        }
        
        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    public Task<byte[]> ExportToCsvAsync(RaporSonucu rapor, CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("CSV export: {RaporAdi}", rapor.RaporAdi);
        
        var sb = new StringBuilder();
        
        // BOM for UTF-8
        sb.Append('\uFEFF');
        
        // Başlık satırı
        sb.AppendLine(string.Join(";", rapor.Sutunlar.Select(s => EscapeCsvField(s.Baslik))));
        
        // Veri satırları
        foreach (var satir in rapor.Satirlar)
        {
            var degerler = rapor.Sutunlar.Select(s => 
            {
                satir.Degerler.TryGetValue(s.Anahtar, out var d);
                return EscapeCsvField(FormatValue(d, s.Tip));
            });
            sb.AppendLine(string.Join(";", degerler));
        }
        
        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    public Task<IEnumerable<RaporSablonu>> GetRaporSablonlariAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<RaporSablonu>>(_sablonlar);
    }

    public Task SaveRaporSablonuAsync(RaporSablonu sablon, CancellationToken cancellationToken = default)
    {
        var existing = _sablonlar.FirstOrDefault(s => s.Id == sablon.Id);
        if (existing != null)
        {
            _sablonlar.Remove(existing);
        }
        _sablonlar.Add(sablon);
        return Task.CompletedTask;
    }

    public Task<ZamanlanmisRapor> CreateZamanlanmisRaporAsync(ZamanlanmisRapor rapor, CancellationToken cancellationToken = default)
    {
        _zamanlanmisRaporlar.Add(rapor);
        return Task.FromResult(rapor);
    }

    public Task<IEnumerable<ZamanlanmisRapor>> GetZamanlanmisRaporlarAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<ZamanlanmisRapor>>(_zamanlanmisRaporlar);
    }

    #region Helper Methods

    private static string FormatValue(object? value, SutunTipi tip)
    {
        if (value == null) return "";
        
        return tip switch
        {
            SutunTipi.Tarih when value is DateTime dt => dt.ToString("dd.MM.yyyy"),
            SutunTipi.ParaBirimi when value is decimal d => d.ToString("N2", CultureInfo.GetCultureInfo("tr-TR")),
            SutunTipi.ParaBirimi when value is double d => d.ToString("N2", CultureInfo.GetCultureInfo("tr-TR")),
            SutunTipi.Sayi when value is decimal d => d.ToString("N2", CultureInfo.GetCultureInfo("tr-TR")),
            SutunTipi.Sayi when value is int i => i.ToString("N0", CultureInfo.GetCultureInfo("tr-TR")),
            SutunTipi.Boole when value is bool b => b ? "Evet" : "Hayır",
            _ => value.ToString() ?? ""
        };
    }

    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        
        if (field.Contains('"') || field.Contains(';') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }

    #endregion
}
