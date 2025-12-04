using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NeoHal.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ayarlar",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    Tip = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ayarlar", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Firma",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Unvan = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    VergiNo = table.Column<string>(type: "TEXT", maxLength: 11, nullable: true),
                    VergiDairesi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Adres = table.Column<string>(type: "TEXT", nullable: true),
                    Telefon = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    WebSite = table.Column<string>(type: "TEXT", nullable: true),
                    HalAdi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    HksKullaniciAdi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    HksSifre = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    VarsayilanRusumOrani = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    VarsayilanKomisyonOrani = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    VarsayilanStopajOrani = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Firma", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HksBildirimleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BildirimTipi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ReferansBelgeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReferansBelgeTipi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    BildirimJson = table.Column<string>(type: "TEXT", nullable: false),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    GonderimTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OnayTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HksKunyeNo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    HksYanitKodu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    HksYanitMesaji = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DenemeSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    SonHataMesaji = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HksBildirimleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Iller",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlakaKodu = table.Column<int>(type: "INTEGER", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Iller", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KapTipleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Kod = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Ad = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DaraAgirlik = table.Column<decimal>(type: "TEXT", precision: 10, scale: 3, nullable: false),
                    RehinBedeli = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KapTipleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KesintTanimlari",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Kod = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Ad = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Tip = table.Column<int>(type: "INTEGER", nullable: false),
                    HesaplamaTipi = table.Column<int>(type: "INTEGER", nullable: false),
                    Oran = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    SabitTutar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    MinTutar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    MaxTutar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    MustahsildenKesilir = table.Column<bool>(type: "INTEGER", nullable: false),
                    AlicidanKesilir = table.Column<bool>(type: "INTEGER", nullable: false),
                    MuhasebeHesapKodu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KesintTanimlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    KullaniciAdi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SifreHash = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    AdSoyad = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Rol = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Yetkiler = table.Column<string>(type: "TEXT", nullable: true),
                    SonGirisTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncLoglar",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TabloAdi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    KayitId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IslemTipi = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    YerelTarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SyncTarih = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    HataMesaji = table.Column<string>(type: "TEXT", nullable: true),
                    JsonData = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncLoglar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UrunGruplari",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Kod = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Ad = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunGruplari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ilceler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IlId = table.Column<int>(type: "INTEGER", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ilceler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ilceler_Iller_IlId",
                        column: x => x.IlId,
                        principalTable: "Iller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KesintHesaplamalari",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReferansBelgeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReferansBelgeTipi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    KesintTanimiId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Matrah = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Oran = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    Tutar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KesintHesaplamalari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KesintHesaplamalari_KesintTanimlari_KesintTanimiId",
                        column: x => x.KesintTanimiId,
                        principalTable: "KesintTanimlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Urunler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Kod = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Ad = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    GrupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    HksUrunKodu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Birim = table.Column<int>(type: "INTEGER", nullable: false),
                    KdvOrani = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    RusumOrani = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    StopajOrani = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Urunler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Urunler_UrunGruplari_GrupId",
                        column: x => x.GrupId,
                        principalTable: "UrunGruplari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CariHesaplar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Kod = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Unvan = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CariTipi = table.Column<int>(type: "INTEGER", nullable: false),
                    CariTipiDetay = table.Column<int>(type: "INTEGER", nullable: true),
                    TcKimlikNo = table.Column<string>(type: "TEXT", maxLength: 11, nullable: true),
                    VergiNo = table.Column<string>(type: "TEXT", maxLength: 11, nullable: true),
                    VergiDairesi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Telefon = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Telefon2 = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Adres = table.Column<string>(type: "TEXT", nullable: true),
                    IlId = table.Column<int>(type: "INTEGER", nullable: true),
                    IlceId = table.Column<int>(type: "INTEGER", nullable: true),
                    HksSicilNo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Bakiye = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    KasaBakiye = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    RiskLimiti = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    RehinLimiti = table.Column<int>(type: "INTEGER", nullable: false),
                    MevcutRehinKasa = table.Column<int>(type: "INTEGER", nullable: false),
                    VarsayilanVadeGun = table.Column<int>(type: "INTEGER", nullable: false),
                    CekKabulEder = table.Column<bool>(type: "INTEGER", nullable: false),
                    SenetKabulEder = table.Column<bool>(type: "INTEGER", nullable: false),
                    SatisBlokajli = table.Column<bool>(type: "INTEGER", nullable: false),
                    BlokajNedeni = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CariHesaplar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CariHesaplar_Ilceler_IlceId",
                        column: x => x.IlceId,
                        principalTable: "Ilceler",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CariHesaplar_Iller_IlId",
                        column: x => x.IlId,
                        principalTable: "Iller",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UrunKapEslestirmeleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UrunId = table.Column<Guid>(type: "TEXT", nullable: false),
                    KapTipiId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Varsayilan = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrtalamaAgirlik = table.Column<decimal>(type: "TEXT", precision: 10, scale: 3, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunKapEslestirmeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrunKapEslestirmeleri_KapTipleri_KapTipiId",
                        column: x => x.KapTipiId,
                        principalTable: "KapTipleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UrunKapEslestirmeleri_Urunler_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urunler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CariHareketler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CariId = table.Column<Guid>(type: "TEXT", nullable: false),
                    HareketTipi = table.Column<int>(type: "INTEGER", nullable: false),
                    Tutar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReferansBelgeTipi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ReferansBelgeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CariHareketler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CariHareketler_CariHesaplar_CariId",
                        column: x => x.CariId,
                        principalTable: "CariHesaplar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CekSenetler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CekMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    BelgeNo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    VadeTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Tutar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CariId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BankaAdi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SubeAdi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    HesapNo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    TahsilTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CekSenetler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CekSenetler_CariHesaplar_CariId",
                        column: x => x.CariId,
                        principalTable: "CariHesaplar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GirisIrsaliyeleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IrsaliyeNo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MustahsilId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SevkiyatciId = table.Column<Guid>(type: "TEXT", nullable: true),
                    NakliyeciId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Plaka = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    KunyeNo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    HksBildirimNo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ToplamBrut = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    ToplamDara = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    ToplamNet = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    ToplamKapAdet = table.Column<int>(type: "INTEGER", nullable: false),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GirisIrsaliyeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GirisIrsaliyeleri_CariHesaplar_MustahsilId",
                        column: x => x.MustahsilId,
                        principalTable: "CariHesaplar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GirisIrsaliyeleri_CariHesaplar_NakliyeciId",
                        column: x => x.NakliyeciId,
                        principalTable: "CariHesaplar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GirisIrsaliyeleri_CariHesaplar_SevkiyatciId",
                        column: x => x.SevkiyatciId,
                        principalTable: "CariHesaplar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KasaHesabi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GirisHareketi = table.Column<bool>(type: "INTEGER", nullable: false),
                    Tutar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    OdemeTuru = table.Column<int>(type: "INTEGER", nullable: false),
                    CariId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ReferansBelgeTipi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ReferansBelgeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KasaHesabi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KasaHesabi_CariHesaplar_CariId",
                        column: x => x.CariId,
                        principalTable: "CariHesaplar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KasaStokDurumlari",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CariId = table.Column<Guid>(type: "TEXT", nullable: false),
                    KapTipiId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DoluKasaAdet = table.Column<int>(type: "INTEGER", nullable: false),
                    BosKasaAdet = table.Column<int>(type: "INTEGER", nullable: false),
                    RehinToplam = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KasaStokDurumlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KasaStokDurumlari_CariHesaplar_CariId",
                        column: x => x.CariId,
                        principalTable: "CariHesaplar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KasaStokDurumlari_KapTipleri_KapTipiId",
                        column: x => x.KapTipiId,
                        principalTable: "KapTipleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RehinFisleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FisNo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CariId = table.Column<Guid>(type: "TEXT", nullable: false),
                    KapTipiId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IslemTipiAl = table.Column<bool>(type: "INTEGER", nullable: false),
                    KasaAdet = table.Column<int>(type: "INTEGER", nullable: false),
                    BirimBedel = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ToplamTutar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Odendi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OdemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RehinFisleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RehinFisleri_CariHesaplar_CariId",
                        column: x => x.CariId,
                        principalTable: "CariHesaplar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RehinFisleri_KapTipleri_KapTipiId",
                        column: x => x.KapTipiId,
                        principalTable: "KapTipleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SatisFaturalari",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FaturaNo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FaturaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FaturaTipi = table.Column<int>(type: "INTEGER", nullable: false),
                    AliciId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MustahsilId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AraToplam = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    RusumTutari = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    KomisyonTutari = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    StopajTutari = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    KdvTutari = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    GenelToplam = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    OdemeDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    OdenenTutar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    EFaturaUuid = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    EFaturaDurum = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    HksBildirimNo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatisFaturalari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SatisFaturalari_CariHesaplar_AliciId",
                        column: x => x.AliciId,
                        principalTable: "CariHesaplar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SatisFaturalari_CariHesaplar_MustahsilId",
                        column: x => x.MustahsilId,
                        principalTable: "CariHesaplar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GirisIrsaliyesiKalemleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IrsaliyeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UrunId = table.Column<Guid>(type: "TEXT", nullable: false),
                    KapTipiId = table.Column<Guid>(type: "TEXT", nullable: false),
                    KapAdet = table.Column<int>(type: "INTEGER", nullable: false),
                    BrutKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    DaraKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    NetKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    KalanKapAdet = table.Column<int>(type: "INTEGER", nullable: false),
                    KalanKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GirisIrsaliyesiKalemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GirisIrsaliyesiKalemleri_GirisIrsaliyeleri_IrsaliyeId",
                        column: x => x.IrsaliyeId,
                        principalTable: "GirisIrsaliyeleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GirisIrsaliyesiKalemleri_KapTipleri_KapTipiId",
                        column: x => x.KapTipiId,
                        principalTable: "KapTipleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GirisIrsaliyesiKalemleri_Urunler_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urunler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MustahsilMakbuzlari",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MakbuzNo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MustahsilId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GirisIrsaliyesiId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UrunId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Miktar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    BrutTutar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    StopajTutari = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    RusumTutari = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    KomisyonTutari = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    NavlunTutari = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    HamaliyeTutari = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    BagkurTutari = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DigerKesintiler = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    EmmDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    EmmUuid = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    EmmTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Odendi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OdemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OdemeTuru = table.Column<int>(type: "INTEGER", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MustahsilMakbuzlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MustahsilMakbuzlari_CariHesaplar_MustahsilId",
                        column: x => x.MustahsilId,
                        principalTable: "CariHesaplar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MustahsilMakbuzlari_GirisIrsaliyeleri_GirisIrsaliyesiId",
                        column: x => x.GirisIrsaliyesiId,
                        principalTable: "GirisIrsaliyeleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MustahsilMakbuzlari_Urunler_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urunler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KasaHareketleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    KasaStokId = table.Column<Guid>(type: "TEXT", nullable: false),
                    HareketTipi = table.Column<int>(type: "INTEGER", nullable: false),
                    Adet = table.Column<int>(type: "INTEGER", nullable: false),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReferansBelgeTipi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ReferansBelgeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KasaHareketleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KasaHareketleri_KasaStokDurumlari_KasaStokId",
                        column: x => x.KasaStokId,
                        principalTable: "KasaStokDurumlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LotBilgileri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LotNo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    GirisIrsaliyesiId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GirisKalemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MustahsilId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UrunId = table.Column<Guid>(type: "TEXT", nullable: false),
                    KapTipiId = table.Column<Guid>(type: "TEXT", nullable: false),
                    KapAdet = table.Column<int>(type: "INTEGER", nullable: false),
                    BrutKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    NetKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    UretimBolgesi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    HasatTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HksKunyeNo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    HksBildirimTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SatilanKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotBilgileri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LotBilgileri_CariHesaplar_MustahsilId",
                        column: x => x.MustahsilId,
                        principalTable: "CariHesaplar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LotBilgileri_GirisIrsaliyeleri_GirisIrsaliyesiId",
                        column: x => x.GirisIrsaliyesiId,
                        principalTable: "GirisIrsaliyeleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LotBilgileri_GirisIrsaliyesiKalemleri_GirisKalemId",
                        column: x => x.GirisKalemId,
                        principalTable: "GirisIrsaliyesiKalemleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LotBilgileri_KapTipleri_KapTipiId",
                        column: x => x.KapTipiId,
                        principalTable: "KapTipleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LotBilgileri_Urunler_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urunler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SatisFaturasiKalemleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FaturaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GirisKalemId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UrunId = table.Column<Guid>(type: "TEXT", nullable: false),
                    KapTipiId = table.Column<Guid>(type: "TEXT", nullable: false),
                    KapAdet = table.Column<int>(type: "INTEGER", nullable: false),
                    BrutKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    DaraKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    NetKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Tutar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    RusumOrani = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    RusumTutari = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    KomisyonOrani = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    KomisyonTutari = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    StopajOrani = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    StopajTutari = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatisFaturasiKalemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SatisFaturasiKalemleri_GirisIrsaliyesiKalemleri_GirisKalemId",
                        column: x => x.GirisKalemId,
                        principalTable: "GirisIrsaliyesiKalemleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SatisFaturasiKalemleri_KapTipleri_KapTipiId",
                        column: x => x.KapTipiId,
                        principalTable: "KapTipleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SatisFaturasiKalemleri_SatisFaturalari_FaturaId",
                        column: x => x.FaturaId,
                        principalTable: "SatisFaturalari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SatisFaturasiKalemleri_Urunler_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urunler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LotSatisHareketleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SatisFaturasiId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SatisKalemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SatilanKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    SatilanKap = table.Column<int>(type: "INTEGER", nullable: false),
                    SatisTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotSatisHareketleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LotSatisHareketleri_LotBilgileri_LotId",
                        column: x => x.LotId,
                        principalTable: "LotBilgileri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LotSatisHareketleri_SatisFaturalari_SatisFaturasiId",
                        column: x => x.SatisFaturasiId,
                        principalTable: "SatisFaturalari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LotSatisHareketleri_SatisFaturasiKalemleri_SatisKalemId",
                        column: x => x.SatisKalemId,
                        principalTable: "SatisFaturasiKalemleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Firma",
                columns: new[] { "Id", "Adres", "Email", "HalAdi", "HksKullaniciAdi", "HksSifre", "Telefon", "Unvan", "VarsayilanKomisyonOrani", "VarsayilanRusumOrani", "VarsayilanStopajOrani", "VergiDairesi", "VergiNo", "WebSite" },
                values: new object[] { 1, null, null, null, null, null, null, "NeoHal Komisyonculuk", 8m, 8m, 4m, null, null, null });

            migrationBuilder.InsertData(
                table: "Iller",
                columns: new[] { "Id", "Ad", "PlakaKodu" },
                values: new object[,]
                {
                    { 1, "Adana", 1 },
                    { 6, "Ankara", 6 },
                    { 7, "Antalya", 7 },
                    { 34, "İstanbul", 34 },
                    { 35, "İzmir", 35 }
                });

            migrationBuilder.InsertData(
                table: "KapTipleri",
                columns: new[] { "Id", "Ad", "Aktif", "DaraAgirlik", "GuncellemeTarihi", "Kod", "OlusturmaTarihi", "RehinBedeli", "SyncDurumu" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Plastik Kasa", true, 1.5m, null, "PLS", new DateTime(2025, 12, 3, 16, 54, 53, 145, DateTimeKind.Local).AddTicks(7150), 50m, 0 },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Tahta Kasa", true, 2.0m, null, "TAH", new DateTime(2025, 12, 3, 16, 54, 53, 145, DateTimeKind.Local).AddTicks(7570), 30m, 0 },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "Karton Koli", true, 0.5m, null, "KRT", new DateTime(2025, 12, 3, 16, 54, 53, 145, DateTimeKind.Local).AddTicks(7580), 0m, 0 }
                });

            migrationBuilder.InsertData(
                table: "KesintTanimlari",
                columns: new[] { "Id", "Ad", "Aktif", "AlicidanKesilir", "GuncellemeTarihi", "HesaplamaTipi", "Kod", "MaxTutar", "MinTutar", "MuhasebeHesapKodu", "MustahsildenKesilir", "OlusturmaTarihi", "Oran", "SabitTutar", "SyncDurumu", "Tip" },
                values: new object[,]
                {
                    { new Guid("f0000001-0000-0000-0000-000000000001"), "Gelir Vergisi Stopajı", true, false, null, 1, "STOPAJ", 79228162514264337593543950335m, 0m, null, true, new DateTime(2025, 12, 3, 16, 54, 53, 146, DateTimeKind.Local).AddTicks(40), 4m, 0m, 0, 1 },
                    { new Guid("f0000001-0000-0000-0000-000000000002"), "Belediye Rüsumu", true, true, null, 1, "RUSUM", 79228162514264337593543950335m, 0m, null, true, new DateTime(2025, 12, 3, 16, 54, 53, 146, DateTimeKind.Local).AddTicks(650), 1m, 0m, 0, 2 },
                    { new Guid("f0000001-0000-0000-0000-000000000003"), "Komisyon Bedeli", true, false, null, 1, "KOMISYON", 79228162514264337593543950335m, 0m, null, true, new DateTime(2025, 12, 3, 16, 54, 53, 146, DateTimeKind.Local).AddTicks(660), 8m, 0m, 0, 3 },
                    { new Guid("f0000001-0000-0000-0000-000000000004"), "Hamaliye Ücreti", true, false, null, 3, "HAMALIYE", 79228162514264337593543950335m, 0m, null, true, new DateTime(2025, 12, 3, 16, 54, 53, 146, DateTimeKind.Local).AddTicks(660), 0m, 0.10m, 0, 4 },
                    { new Guid("f0000001-0000-0000-0000-000000000005"), "Nakliye Ücreti", true, false, null, 3, "NAVLUN", 79228162514264337593543950335m, 0m, null, true, new DateTime(2025, 12, 3, 16, 54, 53, 146, DateTimeKind.Local).AddTicks(750), 0m, 0.05m, 0, 5 },
                    { new Guid("f0000001-0000-0000-0000-000000000006"), "SGK (Bağkur) Kesintisi", true, false, null, 1, "BAGKUR", 79228162514264337593543950335m, 0m, null, true, new DateTime(2025, 12, 3, 16, 54, 53, 146, DateTimeKind.Local).AddTicks(750), 1m, 0m, 0, 6 }
                });

            migrationBuilder.InsertData(
                table: "Kullanicilar",
                columns: new[] { "Id", "AdSoyad", "Aktif", "GuncellemeTarihi", "KullaniciAdi", "OlusturmaTarihi", "Rol", "SifreHash", "SonGirisTarihi", "SyncDurumu", "Yetkiler" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), "Sistem Yöneticisi", true, null, "admin", new DateTime(2025, 12, 3, 16, 54, 53, 145, DateTimeKind.Local).AddTicks(9440), "ADMIN", "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=", null, 0, null });

            migrationBuilder.InsertData(
                table: "UrunGruplari",
                columns: new[] { "Id", "Ad", "Aktif", "GuncellemeTarihi", "Kod", "OlusturmaTarihi", "SyncDurumu" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Sebze", true, null, "SBZ", new DateTime(2025, 12, 3, 16, 54, 53, 139, DateTimeKind.Local).AddTicks(8630), 0 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Meyve", true, null, "MYV", new DateTime(2025, 12, 3, 16, 54, 53, 145, DateTimeKind.Local).AddTicks(6640), 0 }
                });

            migrationBuilder.InsertData(
                table: "Urunler",
                columns: new[] { "Id", "Ad", "Aktif", "Birim", "DeletedAt", "GrupId", "GuncellemeTarihi", "HksUrunKodu", "IsDeleted", "KdvOrani", "Kod", "OlusturmaTarihi", "RusumOrani", "StopajOrani", "SyncDurumu" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Domates", true, 1, null, new Guid("11111111-1111-1111-1111-111111111111"), null, null, false, 1m, "DMT", new DateTime(2025, 12, 3, 16, 54, 53, 145, DateTimeKind.Local).AddTicks(8200), 8m, 4m, 0 },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Biber", true, 1, null, new Guid("11111111-1111-1111-1111-111111111111"), null, null, false, 1m, "BBR", new DateTime(2025, 12, 3, 16, 54, 53, 145, DateTimeKind.Local).AddTicks(8540), 8m, 4m, 0 },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Patlıcan", true, 1, null, new Guid("11111111-1111-1111-1111-111111111111"), null, null, false, 1m, "PTL", new DateTime(2025, 12, 3, 16, 54, 53, 145, DateTimeKind.Local).AddTicks(8550), 8m, 4m, 0 },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Elma", true, 1, null, new Guid("22222222-2222-2222-2222-222222222222"), null, null, false, 1m, "ELM", new DateTime(2025, 12, 3, 16, 54, 53, 145, DateTimeKind.Local).AddTicks(8550), 8m, 4m, 0 },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Portakal", true, 1, null, new Guid("22222222-2222-2222-2222-222222222222"), null, null, false, 1m, "PRT", new DateTime(2025, 12, 3, 16, 54, 53, 145, DateTimeKind.Local).AddTicks(8560), 8m, 4m, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CariHareketler_CariId",
                table: "CariHareketler",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_CariHareketler_Tarih",
                table: "CariHareketler",
                column: "Tarih");

            migrationBuilder.CreateIndex(
                name: "IX_CariHesaplar_Aktif",
                table: "CariHesaplar",
                column: "Aktif");

            migrationBuilder.CreateIndex(
                name: "IX_CariHesaplar_CariTipi",
                table: "CariHesaplar",
                column: "CariTipi");

            migrationBuilder.CreateIndex(
                name: "IX_CariHesaplar_IlceId",
                table: "CariHesaplar",
                column: "IlceId");

            migrationBuilder.CreateIndex(
                name: "IX_CariHesaplar_IlId",
                table: "CariHesaplar",
                column: "IlId");

            migrationBuilder.CreateIndex(
                name: "IX_CariHesaplar_Kod",
                table: "CariHesaplar",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CekSenetler_CariId",
                table: "CekSenetler",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_CekSenetler_Durum",
                table: "CekSenetler",
                column: "Durum");

            migrationBuilder.CreateIndex(
                name: "IX_CekSenetler_VadeTarihi",
                table: "CekSenetler",
                column: "VadeTarihi");

            migrationBuilder.CreateIndex(
                name: "IX_GirisIrsaliyeleri_IrsaliyeNo",
                table: "GirisIrsaliyeleri",
                column: "IrsaliyeNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GirisIrsaliyeleri_MustahsilId",
                table: "GirisIrsaliyeleri",
                column: "MustahsilId");

            migrationBuilder.CreateIndex(
                name: "IX_GirisIrsaliyeleri_NakliyeciId",
                table: "GirisIrsaliyeleri",
                column: "NakliyeciId");

            migrationBuilder.CreateIndex(
                name: "IX_GirisIrsaliyeleri_SevkiyatciId",
                table: "GirisIrsaliyeleri",
                column: "SevkiyatciId");

            migrationBuilder.CreateIndex(
                name: "IX_GirisIrsaliyeleri_Tarih",
                table: "GirisIrsaliyeleri",
                column: "Tarih");

            migrationBuilder.CreateIndex(
                name: "IX_GirisIrsaliyesiKalemleri_IrsaliyeId",
                table: "GirisIrsaliyesiKalemleri",
                column: "IrsaliyeId");

            migrationBuilder.CreateIndex(
                name: "IX_GirisIrsaliyesiKalemleri_KapTipiId",
                table: "GirisIrsaliyesiKalemleri",
                column: "KapTipiId");

            migrationBuilder.CreateIndex(
                name: "IX_GirisIrsaliyesiKalemleri_UrunId",
                table: "GirisIrsaliyesiKalemleri",
                column: "UrunId");

            migrationBuilder.CreateIndex(
                name: "IX_HksBildirimleri_Durum",
                table: "HksBildirimleri",
                column: "Durum");

            migrationBuilder.CreateIndex(
                name: "IX_HksBildirimleri_ReferansBelgeId",
                table: "HksBildirimleri",
                column: "ReferansBelgeId");

            migrationBuilder.CreateIndex(
                name: "IX_Ilceler_IlId",
                table: "Ilceler",
                column: "IlId");

            migrationBuilder.CreateIndex(
                name: "IX_KapTipleri_Kod",
                table: "KapTipleri",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KasaHareketleri_KasaStokId",
                table: "KasaHareketleri",
                column: "KasaStokId");

            migrationBuilder.CreateIndex(
                name: "IX_KasaHareketleri_Tarih",
                table: "KasaHareketleri",
                column: "Tarih");

            migrationBuilder.CreateIndex(
                name: "IX_KasaHesabi_CariId",
                table: "KasaHesabi",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_KasaHesabi_Tarih",
                table: "KasaHesabi",
                column: "Tarih");

            migrationBuilder.CreateIndex(
                name: "IX_KasaStokDurumlari_CariId_KapTipiId",
                table: "KasaStokDurumlari",
                columns: new[] { "CariId", "KapTipiId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KasaStokDurumlari_KapTipiId",
                table: "KasaStokDurumlari",
                column: "KapTipiId");

            migrationBuilder.CreateIndex(
                name: "IX_KesintHesaplamalari_KesintTanimiId",
                table: "KesintHesaplamalari",
                column: "KesintTanimiId");

            migrationBuilder.CreateIndex(
                name: "IX_KesintTanimlari_Kod",
                table: "KesintTanimlari",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_KullaniciAdi",
                table: "Kullanicilar",
                column: "KullaniciAdi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LotBilgileri_GirisIrsaliyesiId",
                table: "LotBilgileri",
                column: "GirisIrsaliyesiId");

            migrationBuilder.CreateIndex(
                name: "IX_LotBilgileri_GirisKalemId",
                table: "LotBilgileri",
                column: "GirisKalemId");

            migrationBuilder.CreateIndex(
                name: "IX_LotBilgileri_KapTipiId",
                table: "LotBilgileri",
                column: "KapTipiId");

            migrationBuilder.CreateIndex(
                name: "IX_LotBilgileri_LotNo",
                table: "LotBilgileri",
                column: "LotNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LotBilgileri_MustahsilId",
                table: "LotBilgileri",
                column: "MustahsilId");

            migrationBuilder.CreateIndex(
                name: "IX_LotBilgileri_UrunId",
                table: "LotBilgileri",
                column: "UrunId");

            migrationBuilder.CreateIndex(
                name: "IX_LotSatisHareketleri_LotId",
                table: "LotSatisHareketleri",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_LotSatisHareketleri_SatisFaturasiId",
                table: "LotSatisHareketleri",
                column: "SatisFaturasiId");

            migrationBuilder.CreateIndex(
                name: "IX_LotSatisHareketleri_SatisKalemId",
                table: "LotSatisHareketleri",
                column: "SatisKalemId");

            migrationBuilder.CreateIndex(
                name: "IX_MustahsilMakbuzlari_GirisIrsaliyesiId",
                table: "MustahsilMakbuzlari",
                column: "GirisIrsaliyesiId");

            migrationBuilder.CreateIndex(
                name: "IX_MustahsilMakbuzlari_MakbuzNo",
                table: "MustahsilMakbuzlari",
                column: "MakbuzNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MustahsilMakbuzlari_MustahsilId",
                table: "MustahsilMakbuzlari",
                column: "MustahsilId");

            migrationBuilder.CreateIndex(
                name: "IX_MustahsilMakbuzlari_UrunId",
                table: "MustahsilMakbuzlari",
                column: "UrunId");

            migrationBuilder.CreateIndex(
                name: "IX_RehinFisleri_CariId",
                table: "RehinFisleri",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_RehinFisleri_FisNo",
                table: "RehinFisleri",
                column: "FisNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RehinFisleri_KapTipiId",
                table: "RehinFisleri",
                column: "KapTipiId");

            migrationBuilder.CreateIndex(
                name: "IX_RehinFisleri_Tarih",
                table: "RehinFisleri",
                column: "Tarih");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturalari_AliciId",
                table: "SatisFaturalari",
                column: "AliciId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturalari_FaturaNo",
                table: "SatisFaturalari",
                column: "FaturaNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturalari_FaturaTarihi",
                table: "SatisFaturalari",
                column: "FaturaTarihi");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturalari_MustahsilId",
                table: "SatisFaturalari",
                column: "MustahsilId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturasiKalemleri_FaturaId",
                table: "SatisFaturasiKalemleri",
                column: "FaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturasiKalemleri_GirisKalemId",
                table: "SatisFaturasiKalemleri",
                column: "GirisKalemId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturasiKalemleri_KapTipiId",
                table: "SatisFaturasiKalemleri",
                column: "KapTipiId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturasiKalemleri_UrunId",
                table: "SatisFaturasiKalemleri",
                column: "UrunId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncLoglar_Durum",
                table: "SyncLoglar",
                column: "Durum");

            migrationBuilder.CreateIndex(
                name: "IX_SyncLoglar_YerelTarih",
                table: "SyncLoglar",
                column: "YerelTarih");

            migrationBuilder.CreateIndex(
                name: "IX_UrunGruplari_Kod",
                table: "UrunGruplari",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UrunKapEslestirmeleri_KapTipiId",
                table: "UrunKapEslestirmeleri",
                column: "KapTipiId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunKapEslestirmeleri_UrunId_KapTipiId",
                table: "UrunKapEslestirmeleri",
                columns: new[] { "UrunId", "KapTipiId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Urunler_GrupId",
                table: "Urunler",
                column: "GrupId");

            migrationBuilder.CreateIndex(
                name: "IX_Urunler_Kod",
                table: "Urunler",
                column: "Kod",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ayarlar");

            migrationBuilder.DropTable(
                name: "CariHareketler");

            migrationBuilder.DropTable(
                name: "CekSenetler");

            migrationBuilder.DropTable(
                name: "Firma");

            migrationBuilder.DropTable(
                name: "HksBildirimleri");

            migrationBuilder.DropTable(
                name: "KasaHareketleri");

            migrationBuilder.DropTable(
                name: "KasaHesabi");

            migrationBuilder.DropTable(
                name: "KesintHesaplamalari");

            migrationBuilder.DropTable(
                name: "Kullanicilar");

            migrationBuilder.DropTable(
                name: "LotSatisHareketleri");

            migrationBuilder.DropTable(
                name: "MustahsilMakbuzlari");

            migrationBuilder.DropTable(
                name: "RehinFisleri");

            migrationBuilder.DropTable(
                name: "SyncLoglar");

            migrationBuilder.DropTable(
                name: "UrunKapEslestirmeleri");

            migrationBuilder.DropTable(
                name: "KasaStokDurumlari");

            migrationBuilder.DropTable(
                name: "KesintTanimlari");

            migrationBuilder.DropTable(
                name: "LotBilgileri");

            migrationBuilder.DropTable(
                name: "SatisFaturasiKalemleri");

            migrationBuilder.DropTable(
                name: "GirisIrsaliyesiKalemleri");

            migrationBuilder.DropTable(
                name: "SatisFaturalari");

            migrationBuilder.DropTable(
                name: "GirisIrsaliyeleri");

            migrationBuilder.DropTable(
                name: "KapTipleri");

            migrationBuilder.DropTable(
                name: "Urunler");

            migrationBuilder.DropTable(
                name: "CariHesaplar");

            migrationBuilder.DropTable(
                name: "UrunGruplari");

            migrationBuilder.DropTable(
                name: "Ilceler");

            migrationBuilder.DropTable(
                name: "Iller");
        }
    }
}
