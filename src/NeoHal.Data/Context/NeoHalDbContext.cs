using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NeoHal.Core.Entities;

namespace NeoHal.Data.Context;

public class NeoHalDbContext : DbContext
{
    public NeoHalDbContext(DbContextOptions<NeoHalDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    // Cari Hesaplar
    public DbSet<CariHesap> CariHesaplar => Set<CariHesap>();
    public DbSet<Il> Iller => Set<Il>();
    public DbSet<Ilce> Ilceler => Set<Ilce>();

    // Ürün/Stok
    public DbSet<UrunGrubu> UrunGruplari => Set<UrunGrubu>();
    public DbSet<Urun> Urunler => Set<Urun>();
    public DbSet<KapTipi> KapTipleri => Set<KapTipi>();
    public DbSet<UrunKapEslestirme> UrunKapEslestirmeleri => Set<UrunKapEslestirme>();

    // Operasyon
    public DbSet<GirisIrsaliyesi> GirisIrsaliyeleri => Set<GirisIrsaliyesi>();
    public DbSet<GirisIrsaliyesiKalem> GirisIrsaliyesiKalemleri => Set<GirisIrsaliyesiKalem>();
    public DbSet<SatisFaturasi> SatisFaturalari => Set<SatisFaturasi>();
    public DbSet<SatisFaturasiKalem> SatisFaturasiKalemleri => Set<SatisFaturasiKalem>();

    // Kasa Takip
    public DbSet<KasaStokDurumu> KasaStokDurumlari => Set<KasaStokDurumu>();
    public DbSet<KasaHareket> KasaHareketleri => Set<KasaHareket>();
    public DbSet<RehinFisi> RehinFisleri => Set<RehinFisi>();

    // Finans
    public DbSet<CariHareket> CariHareketler => Set<CariHareket>();
    public DbSet<KasaHesabi> KasaHesabi => Set<KasaHesabi>();
    public DbSet<CekSenet> CekSenetler => Set<CekSenet>();

    // Kesinti ve Lot Takip
    public DbSet<KesintTanimi> KesintTanimlari => Set<KesintTanimi>();
    public DbSet<KesintHesaplama> KesintHesaplamalari => Set<KesintHesaplama>();
    public DbSet<LotBilgisi> LotBilgileri => Set<LotBilgisi>();
    public DbSet<LotSatisHareket> LotSatisHareketleri => Set<LotSatisHareket>();

    // HKS Entegrasyon
    public DbSet<HksBildirim> HksBildirimleri => Set<HksBildirim>();
    public DbSet<MustahsilMakbuzu> MustahsilMakbuzlari => Set<MustahsilMakbuzu>();
    
    // E-Fatura
    public DbSet<EFatura> EFaturalar => Set<EFatura>();
    public DbSet<EFaturaKalem> EFaturaKalemleri => Set<EFaturaKalem>();
    public DbSet<EFaturaAyarlari> EFaturaAyarlari => Set<EFaturaAyarlari>();

    // Sistem
    public DbSet<SyncLog> SyncLoglar => Set<SyncLog>();
    public DbSet<Ayar> Ayarlar => Set<Ayar>();
    public DbSet<Kullanici> Kullanicilar => Set<Kullanici>();
    public DbSet<Firma> Firma => Set<Firma>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // === CARİ HESAP ===
        modelBuilder.Entity<CariHesap>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Kod).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Unvan).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TcKimlikNo).HasMaxLength(11);
            entity.Property(e => e.VergiNo).HasMaxLength(11);
            entity.Property(e => e.VergiDairesi).HasMaxLength(100);
            entity.Property(e => e.Telefon).HasMaxLength(20);
            entity.Property(e => e.Telefon2).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HksSicilNo).HasMaxLength(50);
            entity.Property(e => e.Bakiye).HasPrecision(18, 2);
            entity.Property(e => e.KasaBakiye).HasPrecision(18, 2);
            entity.Property(e => e.RiskLimiti).HasPrecision(18, 2);
            entity.Property(e => e.BlokajNedeni).HasMaxLength(200);
            
            entity.HasIndex(e => e.Kod).IsUnique();
            entity.HasIndex(e => e.CariTipi);
            entity.HasIndex(e => e.Aktif);
            
            // Şube - Ana Cari İlişkisi (Self-referencing)
            entity.HasOne(e => e.AnaCari)
                  .WithMany(e => e.Subeler)
                  .HasForeignKey(e => e.AnaCariId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // === İL/İLÇE ===
        modelBuilder.Entity<Il>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Ad).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Ilce>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Ad).HasMaxLength(50).IsRequired();
            entity.HasOne(e => e.Il).WithMany(i => i.Ilceler).HasForeignKey(e => e.IlId);
        });

        // === ÜRÜN ===
        modelBuilder.Entity<UrunGrubu>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Kod).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Ad).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Kod).IsUnique();
        });

        modelBuilder.Entity<Urun>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Kod).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Ad).HasMaxLength(100).IsRequired();
            entity.Property(e => e.HksUrunKodu).HasMaxLength(50);
            entity.Property(e => e.KdvOrani).HasPrecision(5, 2);
            entity.Property(e => e.RusumOrani).HasPrecision(5, 2);
            entity.Property(e => e.StopajOrani).HasPrecision(5, 2);
            
            entity.HasIndex(e => e.Kod).IsUnique();
            entity.HasOne(e => e.Grup).WithMany(g => g.Urunler).HasForeignKey(e => e.GrupId);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<KapTipi>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Kod).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Ad).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DaraAgirlik).HasPrecision(10, 3);
            entity.Property(e => e.RehinBedeli).HasPrecision(18, 2);
            entity.HasIndex(e => e.Kod).IsUnique();
        });

        modelBuilder.Entity<UrunKapEslestirme>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrtalamaAgirlik).HasPrecision(10, 3);
            entity.HasOne(e => e.Urun).WithMany(u => u.KapEslestirmeleri).HasForeignKey(e => e.UrunId);
            entity.HasOne(e => e.KapTipi).WithMany(k => k.UrunEslestirmeleri).HasForeignKey(e => e.KapTipiId);
            entity.HasIndex(e => new { e.UrunId, e.KapTipiId }).IsUnique();
        });

        // === GİRİŞ İRSALİYESİ ===
        modelBuilder.Entity<GirisIrsaliyesi>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IrsaliyeNo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Plaka).HasMaxLength(20);
            entity.Property(e => e.KunyeNo).HasMaxLength(50);
            entity.Property(e => e.HksBildirimNo).HasMaxLength(50);
            entity.Property(e => e.ToplamBrut).HasPrecision(18, 3);
            entity.Property(e => e.ToplamDara).HasPrecision(18, 3);
            entity.Property(e => e.ToplamNet).HasPrecision(18, 3);
            
            entity.HasIndex(e => e.IrsaliyeNo).IsUnique();
            entity.HasIndex(e => e.Tarih);
            entity.HasIndex(e => e.MustahsilId);
            
            entity.HasOne(e => e.Mustahsil).WithMany(c => c.GirisIrsaliyeleri).HasForeignKey(e => e.MustahsilId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Sevkiyatci).WithMany().HasForeignKey(e => e.SevkiyatciId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Nakliyeci).WithMany().HasForeignKey(e => e.NakliyeciId).OnDelete(DeleteBehavior.Restrict);
            
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<GirisIrsaliyesiKalem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BrutKg).HasPrecision(18, 3);
            entity.Property(e => e.DaraKg).HasPrecision(18, 3);
            entity.Property(e => e.NetKg).HasPrecision(18, 3);
            entity.Property(e => e.BirimFiyat).HasPrecision(18, 2);
            entity.Property(e => e.KalanKg).HasPrecision(18, 3);
            
            entity.HasOne(e => e.Irsaliye).WithMany(i => i.Kalemler).HasForeignKey(e => e.IrsaliyeId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Urun).WithMany().HasForeignKey(e => e.UrunId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.KapTipi).WithMany().HasForeignKey(e => e.KapTipiId).OnDelete(DeleteBehavior.Restrict);
            
            // Hal Kayıt için: Bu satırdaki malı aldığımız komisyoncu
            entity.HasOne(e => e.Komisyoncu).WithMany().HasForeignKey(e => e.KomisyoncuId).OnDelete(DeleteBehavior.Restrict);
        });

        // === SATIŞ FATURASI ===
        modelBuilder.Entity<SatisFaturasi>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FaturaNo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.EFaturaUuid).HasMaxLength(100);
            entity.Property(e => e.EFaturaDurum).HasMaxLength(50);
            entity.Property(e => e.HksBildirimNo).HasMaxLength(50);
            entity.Property(e => e.AraToplam).HasPrecision(18, 2);
            entity.Property(e => e.RusumTutari).HasPrecision(18, 2);
            entity.Property(e => e.KomisyonTutari).HasPrecision(18, 2);
            entity.Property(e => e.StopajTutari).HasPrecision(18, 2);
            entity.Property(e => e.EkMasrafTutari).HasPrecision(18, 2);
            entity.Property(e => e.EkMasrafAciklama).HasMaxLength(500);
            entity.Property(e => e.KdvTutari).HasPrecision(18, 2);
            entity.Property(e => e.GenelToplam).HasPrecision(18, 2);
            entity.Property(e => e.OdenenTutar).HasPrecision(18, 2);
            
            entity.HasIndex(e => e.FaturaNo).IsUnique();
            entity.HasIndex(e => e.FaturaTarihi);
            entity.HasIndex(e => e.AliciId);
            entity.HasIndex(e => e.FaturaTipi);
            
            entity.HasOne(e => e.Alici).WithMany(c => c.SatisFaturalari).HasForeignKey(e => e.AliciId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Mustahsil).WithMany().HasForeignKey(e => e.MustahsilId).OnDelete(DeleteBehavior.Restrict);
            
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<SatisFaturasiKalem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BrutKg).HasPrecision(18, 3);
            entity.Property(e => e.DaraKg).HasPrecision(18, 3);
            entity.Property(e => e.NetKg).HasPrecision(18, 3);
            entity.Property(e => e.AlisFiyati).HasPrecision(18, 2);
            entity.Property(e => e.BirimFiyat).HasPrecision(18, 2);
            entity.Property(e => e.Tutar).HasPrecision(18, 2);
            entity.Property(e => e.RusumOrani).HasPrecision(5, 2);
            entity.Property(e => e.RusumTutari).HasPrecision(18, 2);
            entity.Property(e => e.KomisyonOrani).HasPrecision(5, 2);
            entity.Property(e => e.KomisyonTutari).HasPrecision(18, 2);
            entity.Property(e => e.StopajOrani).HasPrecision(5, 2);
            entity.Property(e => e.StopajTutari).HasPrecision(18, 2);
            
            entity.HasOne(e => e.Fatura).WithMany(f => f.Kalemler).HasForeignKey(e => e.FaturaId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.GirisKalem).WithMany(g => g.SatisKalemleri).HasForeignKey(e => e.GirisKalemId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Urun).WithMany().HasForeignKey(e => e.UrunId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.KapTipi).WithMany().HasForeignKey(e => e.KapTipiId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Komisyoncu).WithMany().HasForeignKey(e => e.KomisyoncuId).OnDelete(DeleteBehavior.Restrict);
        });

        // === KASA TAKİP ===
        modelBuilder.Entity<KasaStokDurumu>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RehinToplam).HasPrecision(18, 2);
            
            entity.HasIndex(e => new { e.CariId, e.KapTipiId }).IsUnique();
            entity.HasOne(e => e.Cari).WithMany(c => c.KasaStokDurumlari).HasForeignKey(e => e.CariId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.KapTipi).WithMany().HasForeignKey(e => e.KapTipiId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<KasaHareket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReferansBelgeTipi).HasMaxLength(50);
            entity.HasIndex(e => e.Tarih);
            entity.HasOne(e => e.KasaStok).WithMany(k => k.Hareketler).HasForeignKey(e => e.KasaStokId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RehinFisi>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FisNo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.BirimBedel).HasPrecision(18, 2);
            entity.Property(e => e.ToplamTutar).HasPrecision(18, 2);
            
            entity.HasIndex(e => e.FisNo).IsUnique();
            entity.HasIndex(e => e.Tarih);
            entity.HasOne(e => e.Cari).WithMany().HasForeignKey(e => e.CariId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.KapTipi).WithMany().HasForeignKey(e => e.KapTipiId).OnDelete(DeleteBehavior.Restrict);
        });

        // === FİNANS ===
        modelBuilder.Entity<CariHareket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tutar).HasPrecision(18, 2);
            entity.Property(e => e.ReferansBelgeTipi).HasMaxLength(50);
            
            entity.HasIndex(e => e.Tarih);
            entity.HasIndex(e => e.CariId);
            entity.HasOne(e => e.Cari).WithMany(c => c.CariHareketler).HasForeignKey(e => e.CariId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<KasaHesabi>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tutar).HasPrecision(18, 2);
            entity.Property(e => e.ReferansBelgeTipi).HasMaxLength(50);
            
            entity.HasIndex(e => e.Tarih);
            entity.HasOne(e => e.Cari).WithMany().HasForeignKey(e => e.CariId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CekSenet>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BelgeNo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Tutar).HasPrecision(18, 2);
            entity.Property(e => e.BankaAdi).HasMaxLength(100);
            entity.Property(e => e.SubeAdi).HasMaxLength(100);
            entity.Property(e => e.HesapNo).HasMaxLength(50);
            
            entity.HasIndex(e => e.VadeTarihi);
            entity.HasIndex(e => e.Durum);
            entity.HasOne(e => e.Cari).WithMany().HasForeignKey(e => e.CariId).OnDelete(DeleteBehavior.Restrict);
        });

        // === SİSTEM ===
        modelBuilder.Entity<SyncLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TabloAdi).HasMaxLength(100).IsRequired();
            entity.Property(e => e.IslemTipi).HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.Durum);
            entity.HasIndex(e => e.YerelTarih);
        });

        modelBuilder.Entity<Ayar>(entity =>
        {
            entity.HasKey(e => e.Key);
            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Tip).HasMaxLength(20);
        });

        modelBuilder.Entity<Kullanici>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.KullaniciAdi).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SifreHash).HasMaxLength(256).IsRequired();
            entity.Property(e => e.AdSoyad).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Rol).HasMaxLength(20).IsRequired();
            
            entity.HasIndex(e => e.KullaniciAdi).IsUnique();
        });

        modelBuilder.Entity<Firma>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Unvan).HasMaxLength(200).IsRequired();
            entity.Property(e => e.VergiNo).HasMaxLength(11);
            entity.Property(e => e.VergiDairesi).HasMaxLength(100);
            entity.Property(e => e.Telefon).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HalAdi).HasMaxLength(100);
            entity.Property(e => e.HksKullaniciAdi).HasMaxLength(100);
            entity.Property(e => e.HksSifre).HasMaxLength(256);
            entity.Property(e => e.VarsayilanRusumOrani).HasPrecision(5, 2);
            entity.Property(e => e.VarsayilanKomisyonOrani).HasPrecision(5, 2);
            entity.Property(e => e.VarsayilanStopajOrani).HasPrecision(5, 2);
        });

        // === KESİNTİ TANIMLARI ===
        modelBuilder.Entity<KesintTanimi>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Kod).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Ad).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Oran).HasPrecision(5, 2);
            entity.Property(e => e.SabitTutar).HasPrecision(18, 2);
            entity.Property(e => e.MinTutar).HasPrecision(18, 2);
            entity.Property(e => e.MaxTutar).HasPrecision(18, 2);
            entity.Property(e => e.MuhasebeHesapKodu).HasMaxLength(50);
            entity.HasIndex(e => e.Kod).IsUnique();
        });

        modelBuilder.Entity<KesintHesaplama>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReferansBelgeTipi).HasMaxLength(50);
            entity.Property(e => e.Matrah).HasPrecision(18, 2);
            entity.Property(e => e.Oran).HasPrecision(5, 2);
            entity.Property(e => e.Tutar).HasPrecision(18, 2);
            entity.HasOne(e => e.KesintTanimi).WithMany().HasForeignKey(e => e.KesintTanimiId);
        });

        // === LOT TAKİP ===
        modelBuilder.Entity<LotBilgisi>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LotNo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.UretimBolgesi).HasMaxLength(100);
            entity.Property(e => e.HksKunyeNo).HasMaxLength(50);
            entity.Property(e => e.BrutKg).HasPrecision(18, 3);
            entity.Property(e => e.NetKg).HasPrecision(18, 3);
            entity.Property(e => e.SatilanKg).HasPrecision(18, 3);
            entity.HasIndex(e => e.LotNo).IsUnique();
            entity.HasOne(e => e.GirisIrsaliyesi).WithMany().HasForeignKey(e => e.GirisIrsaliyesiId);
            entity.HasOne(e => e.Mustahsil).WithMany().HasForeignKey(e => e.MustahsilId);
            entity.HasOne(e => e.Urun).WithMany().HasForeignKey(e => e.UrunId);
            entity.HasOne(e => e.KapTipi).WithMany().HasForeignKey(e => e.KapTipiId);
        });

        modelBuilder.Entity<LotSatisHareket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SatilanKg).HasPrecision(18, 3);
            entity.HasOne(e => e.Lot).WithMany(l => l.SatisHareketleri).HasForeignKey(e => e.LotId);
            entity.HasOne(e => e.SatisFaturasi).WithMany().HasForeignKey(e => e.SatisFaturasiId);
        });

        // === HKS ENTEGRASYON ===
        modelBuilder.Entity<HksBildirim>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BildirimTipi).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ReferansBelgeTipi).HasMaxLength(50);
            entity.Property(e => e.HksKunyeNo).HasMaxLength(50);
            entity.Property(e => e.HksYanitKodu).HasMaxLength(50);
            entity.Property(e => e.HksYanitMesaji).HasMaxLength(500);
            entity.Property(e => e.SonHataMesaji).HasMaxLength(500);
            entity.HasIndex(e => e.Durum);
            entity.HasIndex(e => e.ReferansBelgeId);
        });

        modelBuilder.Entity<MustahsilMakbuzu>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MakbuzNo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Miktar).HasPrecision(18, 3);
            entity.Property(e => e.BirimFiyat).HasPrecision(18, 2);
            entity.Property(e => e.BrutTutar).HasPrecision(18, 2);
            entity.Property(e => e.StopajTutari).HasPrecision(18, 2);
            entity.Property(e => e.RusumTutari).HasPrecision(18, 2);
            entity.Property(e => e.KomisyonTutari).HasPrecision(18, 2);
            entity.Property(e => e.NavlunTutari).HasPrecision(18, 2);
            entity.Property(e => e.HamaliyeTutari).HasPrecision(18, 2);
            entity.Property(e => e.BagkurTutari).HasPrecision(18, 2);
            entity.Property(e => e.DigerKesintiler).HasPrecision(18, 2);
            entity.Property(e => e.EmmUuid).HasMaxLength(50);
            entity.HasIndex(e => e.MakbuzNo).IsUnique();
            entity.HasOne(e => e.Mustahsil).WithMany().HasForeignKey(e => e.MustahsilId);
            entity.HasOne(e => e.GirisIrsaliyesi).WithMany().HasForeignKey(e => e.GirisIrsaliyesiId);
            entity.HasOne(e => e.Urun).WithMany().HasForeignKey(e => e.UrunId);
        });

        // Seed Data - Temel veriler
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // İller (Örnek)
        modelBuilder.Entity<Il>().HasData(
            new Il { Id = 1, PlakaKodu = 1, Ad = "Adana" },
            new Il { Id = 6, PlakaKodu = 6, Ad = "Ankara" },
            new Il { Id = 7, PlakaKodu = 7, Ad = "Antalya" },
            new Il { Id = 33, PlakaKodu = 33, Ad = "Mersin" },
            new Il { Id = 34, PlakaKodu = 34, Ad = "İstanbul" },
            new Il { Id = 35, PlakaKodu = 35, Ad = "İzmir" }
        );
        
        // Örnek Komisyoncular (Halden mal aldığımız yerler)
        modelBuilder.Entity<CariHesap>().HasData(
            new CariHesap 
            { 
                Id = Guid.Parse("c0000001-0000-0000-0000-000000000001"),
                Kod = "KOM001",
                Unvan = "MEHMET ALİ KOMİSYON",
                CariTipi = Core.Enums.CariTipi.Komisyoncu,
                CariTipiDetay = Core.Enums.CariTipiDetay.Kabzimal,
                Telefon = "0532 111 2233",
                IlId = 33
            },
            new CariHesap 
            { 
                Id = Guid.Parse("c0000001-0000-0000-0000-000000000002"),
                Kod = "KOM002",
                Unvan = "ERKAN CENGİZ KOMİSYON",
                CariTipi = Core.Enums.CariTipi.Komisyoncu,
                CariTipiDetay = Core.Enums.CariTipiDetay.Kabzimal,
                Telefon = "0533 222 3344",
                IlId = 33
            },
            new CariHesap 
            { 
                Id = Guid.Parse("c0000001-0000-0000-0000-000000000003"),
                Kod = "KOM003",
                Unvan = "HALİL KARDEŞLER",
                CariTipi = Core.Enums.CariTipi.Komisyoncu,
                CariTipiDetay = Core.Enums.CariTipiDetay.Kabzimal,
                Telefon = "0534 333 4455",
                IlId = 33
            },
            new CariHesap 
            { 
                Id = Guid.Parse("c0000001-0000-0000-0000-000000000004"),
                Kod = "KOM004",
                Unvan = "DOĞAN TİCARET",
                CariTipi = Core.Enums.CariTipi.Komisyoncu,
                CariTipiDetay = Core.Enums.CariTipiDetay.HalIciKomisyoncu,
                Telefon = "0535 444 5566",
                IlId = 33
            },
            // Örnek Şubeler (Mal gönderdiğimiz yerler)
            new CariHesap 
            { 
                Id = Guid.Parse("a0000001-0000-0000-0000-000000000001"),
                Kod = "SUB001",
                Unvan = "ANKARA ŞUBESİ",
                CariTipi = Core.Enums.CariTipi.Alici,
                CariTipiDetay = Core.Enums.CariTipiDetay.Tuccar,
                Telefon = "0312 111 2233",
                IlId = 6
            },
            new CariHesap 
            { 
                Id = Guid.Parse("a0000001-0000-0000-0000-000000000002"),
                Kod = "SUB002",
                Unvan = "İSTANBUL ŞUBESİ",
                CariTipi = Core.Enums.CariTipi.Alici,
                CariTipiDetay = Core.Enums.CariTipiDetay.Tuccar,
                Telefon = "0212 222 3344",
                IlId = 34
            },
            new CariHesap 
            { 
                Id = Guid.Parse("a0000001-0000-0000-0000-000000000003"),
                Kod = "SUB003",
                Unvan = "ANTALYA ŞUBESİ",
                CariTipi = Core.Enums.CariTipi.Alici,
                CariTipiDetay = Core.Enums.CariTipiDetay.Tuccar,
                Telefon = "0242 333 4455",
                IlId = 7
            },
            new CariHesap 
            { 
                Id = Guid.Parse("a0000001-0000-0000-0000-000000000004"),
                Kod = "ALC001",
                Unvan = "NAZMI DOĞAN",
                CariTipi = Core.Enums.CariTipi.Alici,
                CariTipiDetay = Core.Enums.CariTipiDetay.ManavDukkan,
                Telefon = "0536 555 6677",
                IlId = 33
            },
            new CariHesap 
            { 
                Id = Guid.Parse("a0000001-0000-0000-0000-000000000005"),
                Kod = "ALC002",
                Unvan = "ŞENOL DOĞAN",
                CariTipi = Core.Enums.CariTipi.Alici,
                CariTipiDetay = Core.Enums.CariTipiDetay.ManavDukkan,
                Telefon = "0537 666 7788",
                IlId = 33
            }
        );

        // Ürün Grupları
        var sebzeGrupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var meyveGrupId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        modelBuilder.Entity<UrunGrubu>().HasData(
            new UrunGrubu { Id = sebzeGrupId, Kod = "SBZ", Ad = "Sebze" },
            new UrunGrubu { Id = meyveGrupId, Kod = "MYV", Ad = "Meyve" }
        );

        // Kap Tipleri
        var plastikKasaId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var tahtaKasaId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var kartonKoliId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        modelBuilder.Entity<KapTipi>().HasData(
            new KapTipi { Id = plastikKasaId, Kod = "PLS", Ad = "Plastik Kasa", DaraAgirlik = 1.5m, RehinBedeli = 50 },
            new KapTipi { Id = tahtaKasaId, Kod = "TAH", Ad = "Tahta Kasa", DaraAgirlik = 2.0m, RehinBedeli = 30 },
            new KapTipi { Id = kartonKoliId, Kod = "KRT", Ad = "Karton Koli", DaraAgirlik = 0.5m, RehinBedeli = 0 }
        );

        // Örnek Ürünler
        modelBuilder.Entity<Urun>().HasData(
            new Urun { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Kod = "DMT", Ad = "Domates", GrupId = sebzeGrupId },
            new Urun { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Kod = "BBR", Ad = "Biber", GrupId = sebzeGrupId },
            new Urun { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Kod = "PTL", Ad = "Patlıcan", GrupId = sebzeGrupId },
            new Urun { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), Kod = "ELM", Ad = "Elma", GrupId = meyveGrupId },
            new Urun { Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), Kod = "PRT", Ad = "Portakal", GrupId = meyveGrupId }
        );

        // Firma varsayılan kayıt
        modelBuilder.Entity<Firma>().HasData(
            new Firma 
            { 
                Id = 1, 
                Unvan = "NeoHal Komisyonculuk",
                VarsayilanRusumOrani = 8,
                VarsayilanKomisyonOrani = 8,
                VarsayilanStopajOrani = 4
            }
        );

        // Admin kullanıcı (şifre: admin123)
        modelBuilder.Entity<Kullanici>().HasData(
            new Kullanici
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                KullaniciAdi = "admin",
                SifreHash = "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=", // admin123 SHA256
                AdSoyad = "Sistem Yöneticisi",
                Rol = "ADMIN"
            }
        );

        // Kesinti Tanımları (5957 Sayılı Kanun)
        modelBuilder.Entity<KesintTanimi>().HasData(
            new KesintTanimi
            {
                Id = Guid.Parse("f0000001-0000-0000-0000-000000000001"),
                Kod = "STOPAJ",
                Ad = "Gelir Vergisi Stopajı",
                Tip = Core.Enums.KesintTipi.Stopaj,
                HesaplamaTipi = Core.Enums.HesaplamaTipi.Yuzde,
                Oran = 4, // %4 - Çiftçiden alınan stopaj
                MustahsildenKesilir = true,
                AlicidanKesilir = false,
                Aktif = true
            },
            new KesintTanimi
            {
                Id = Guid.Parse("f0000001-0000-0000-0000-000000000002"),
                Kod = "RUSUM",
                Ad = "Belediye Rüsumu",
                Tip = Core.Enums.KesintTipi.Rusum,
                HesaplamaTipi = Core.Enums.HesaplamaTipi.Yuzde,
                Oran = 1, // %1
                MustahsildenKesilir = true,
                AlicidanKesilir = true,
                Aktif = true
            },
            new KesintTanimi
            {
                Id = Guid.Parse("f0000001-0000-0000-0000-000000000003"),
                Kod = "KOMISYON",
                Ad = "Komisyon Bedeli",
                Tip = Core.Enums.KesintTipi.Komisyon,
                HesaplamaTipi = Core.Enums.HesaplamaTipi.Yuzde,
                Oran = 8, // %8
                MustahsildenKesilir = true,
                AlicidanKesilir = false,
                Aktif = true
            },
            new KesintTanimi
            {
                Id = Guid.Parse("f0000001-0000-0000-0000-000000000004"),
                Kod = "HAMALIYE",
                Ad = "Hamaliye Ücreti",
                Tip = Core.Enums.KesintTipi.Hamaliye,
                HesaplamaTipi = Core.Enums.HesaplamaTipi.SabitKiloBasi,
                SabitTutar = 0.10m, // 0.10 TL/kg
                MustahsildenKesilir = true,
                AlicidanKesilir = false,
                Aktif = true
            },
            new KesintTanimi
            {
                Id = Guid.Parse("f0000001-0000-0000-0000-000000000005"),
                Kod = "NAVLUN",
                Ad = "Nakliye Ücreti",
                Tip = Core.Enums.KesintTipi.Navlun,
                HesaplamaTipi = Core.Enums.HesaplamaTipi.SabitKiloBasi,
                SabitTutar = 0.05m, // 0.05 TL/kg
                MustahsildenKesilir = true,
                AlicidanKesilir = false,
                Aktif = true
            },
            new KesintTanimi
            {
                Id = Guid.Parse("f0000001-0000-0000-0000-000000000006"),
                Kod = "BAGKUR",
                Ad = "SGK (Bağkur) Kesintisi",
                Tip = Core.Enums.KesintTipi.Bagkur,
                HesaplamaTipi = Core.Enums.HesaplamaTipi.Yuzde,
                Oran = 1, // %1
                MustahsildenKesilir = true,
                AlicidanKesilir = false,
                Aktif = true
            }
        );
    }
}
