using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NeoHal.Data.Migrations
{
    /// <inheritdoc />
    public partial class SevkiyatciUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AlisFiyati",
                table: "SatisFaturasiKalemleri",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "KomisyoncuId",
                table: "SatisFaturasiKalemleri",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EkMasrafAciklama",
                table: "SatisFaturalari",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EkMasrafTutari",
                table: "SatisFaturalari",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "AnaCariId",
                table: "CariHesaplar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EFaturaAyarlari",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Entegrator = table.Column<int>(type: "INTEGER", nullable: false),
                    TestModu = table.Column<bool>(type: "INTEGER", nullable: false),
                    KullaniciAdi = table.Column<string>(type: "TEXT", nullable: false),
                    Sifre = table.Column<string>(type: "TEXT", nullable: false),
                    FirmaVkn = table.Column<string>(type: "TEXT", nullable: false),
                    FirmaUnvan = table.Column<string>(type: "TEXT", nullable: false),
                    FirmaAdres = table.Column<string>(type: "TEXT", nullable: false),
                    FirmaIl = table.Column<string>(type: "TEXT", nullable: false),
                    FirmaIlce = table.Column<string>(type: "TEXT", nullable: false),
                    FirmaVergiDairesi = table.Column<string>(type: "TEXT", nullable: false),
                    FirmaTelefon = table.Column<string>(type: "TEXT", nullable: true),
                    FirmaEposta = table.Column<string>(type: "TEXT", nullable: true),
                    PostaKutusuEtiketi = table.Column<string>(type: "TEXT", nullable: true),
                    SonSeriNo = table.Column<int>(type: "INTEGER", nullable: false),
                    SeriOnEki = table.Column<string>(type: "TEXT", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EFaturaAyarlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EFaturalar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Uuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    FaturaNo = table.Column<string>(type: "TEXT", nullable: false),
                    SatisFaturasiId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FaturaTipi = table.Column<int>(type: "INTEGER", nullable: false),
                    Senaryo = table.Column<int>(type: "INTEGER", nullable: false),
                    FaturaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GondericiVkn = table.Column<string>(type: "TEXT", nullable: false),
                    GondericiUnvan = table.Column<string>(type: "TEXT", nullable: false),
                    AliciVkn = table.Column<string>(type: "TEXT", nullable: false),
                    AliciUnvan = table.Column<string>(type: "TEXT", nullable: false),
                    AliciEposta = table.Column<string>(type: "TEXT", nullable: true),
                    AraToplam = table.Column<decimal>(type: "TEXT", nullable: false),
                    KdvToplam = table.Column<decimal>(type: "TEXT", nullable: false),
                    GenelToplam = table.Column<decimal>(type: "TEXT", nullable: false),
                    ParaBirimi = table.Column<string>(type: "TEXT", nullable: false),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    EntegratorId = table.Column<string>(type: "TEXT", nullable: true),
                    GonderimTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    YanitTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    YanitAciklama = table.Column<string>(type: "TEXT", nullable: true),
                    UblXml = table.Column<string>(type: "TEXT", nullable: true),
                    PdfBase64 = table.Column<string>(type: "TEXT", nullable: true),
                    ImzaliXml = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EFaturalar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EFaturalar_SatisFaturalari_SatisFaturasiId",
                        column: x => x.SatisFaturasiId,
                        principalTable: "SatisFaturalari",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EFaturaKalemleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EFaturaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    MalKodu = table.Column<string>(type: "TEXT", nullable: true),
                    MalAdi = table.Column<string>(type: "TEXT", nullable: false),
                    Miktar = table.Column<decimal>(type: "TEXT", nullable: false),
                    Birim = table.Column<string>(type: "TEXT", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "TEXT", nullable: false),
                    KdvOrani = table.Column<decimal>(type: "TEXT", nullable: false),
                    KdvTutari = table.Column<decimal>(type: "TEXT", nullable: false),
                    Tutar = table.Column<decimal>(type: "TEXT", nullable: false),
                    ToplamTutar = table.Column<decimal>(type: "TEXT", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EFaturaKalemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EFaturaKalemleri_EFaturalar_EFaturaId",
                        column: x => x.EFaturaId,
                        principalTable: "EFaturalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CariHesaplar",
                columns: new[] { "Id", "Adres", "Aktif", "AnaCariId", "Bakiye", "BlokajNedeni", "CariTipi", "CariTipiDetay", "CekKabulEder", "DeletedAt", "Email", "GuncellemeTarihi", "HksSicilNo", "IlId", "IlceId", "IsDeleted", "KasaBakiye", "Kod", "MevcutRehinKasa", "OlusturmaTarihi", "RehinLimiti", "RiskLimiti", "SatisBlokajli", "SenetKabulEder", "SyncDurumu", "TcKimlikNo", "Telefon", "Telefon2", "Unvan", "VarsayilanVadeGun", "VergiDairesi", "VergiNo" },
                values: new object[,]
                {
                    { new Guid("a0000001-0000-0000-0000-000000000001"), null, true, null, 0m, null, 4, 4, true, null, null, null, null, 6, null, false, 0m, "SUB001", 0, new DateTime(2025, 12, 4, 20, 4, 4, 423, DateTimeKind.Local).AddTicks(4960), 0, 0m, false, false, 0, null, "0312 111 2233", null, "ANKARA ŞUBESİ", 30, null, null },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), null, true, null, 0m, null, 4, 4, true, null, null, null, null, 34, null, false, 0m, "SUB002", 0, new DateTime(2025, 12, 4, 20, 4, 4, 423, DateTimeKind.Local).AddTicks(4990), 0, 0m, false, false, 0, null, "0212 222 3344", null, "İSTANBUL ŞUBESİ", 30, null, null },
                    { new Guid("a0000001-0000-0000-0000-000000000003"), null, true, null, 0m, null, 4, 4, true, null, null, null, null, 7, null, false, 0m, "SUB003", 0, new DateTime(2025, 12, 4, 20, 4, 4, 423, DateTimeKind.Local).AddTicks(5000), 0, 0m, false, false, 0, null, "0242 333 4455", null, "ANTALYA ŞUBESİ", 30, null, null }
                });

            migrationBuilder.InsertData(
                table: "Iller",
                columns: new[] { "Id", "Ad", "PlakaKodu" },
                values: new object[] { 33, "Mersin", 33 });

            migrationBuilder.UpdateData(
                table: "KapTipleri",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 423, DateTimeKind.Local).AddTicks(8790));

            migrationBuilder.UpdateData(
                table: "KapTipleri",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 423, DateTimeKind.Local).AddTicks(9850));

            migrationBuilder.UpdateData(
                table: "KapTipleri",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 423, DateTimeKind.Local).AddTicks(9860));

            migrationBuilder.UpdateData(
                table: "KesintTanimlari",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000001"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 424, DateTimeKind.Local).AddTicks(5340));

            migrationBuilder.UpdateData(
                table: "KesintTanimlari",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000002"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 424, DateTimeKind.Local).AddTicks(6900));

            migrationBuilder.UpdateData(
                table: "KesintTanimlari",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000003"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 424, DateTimeKind.Local).AddTicks(6920));

            migrationBuilder.UpdateData(
                table: "KesintTanimlari",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000004"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 424, DateTimeKind.Local).AddTicks(6930));

            migrationBuilder.UpdateData(
                table: "KesintTanimlari",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000005"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 424, DateTimeKind.Local).AddTicks(7140));

            migrationBuilder.UpdateData(
                table: "KesintTanimlari",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000006"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 424, DateTimeKind.Local).AddTicks(7150));

            migrationBuilder.UpdateData(
                table: "Kullanicilar",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 424, DateTimeKind.Local).AddTicks(3790));

            migrationBuilder.UpdateData(
                table: "UrunGruplari",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 423, DateTimeKind.Local).AddTicks(6730));

            migrationBuilder.UpdateData(
                table: "UrunGruplari",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 423, DateTimeKind.Local).AddTicks(7600));

            migrationBuilder.UpdateData(
                table: "Urunler",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 424, DateTimeKind.Local).AddTicks(950));

            migrationBuilder.UpdateData(
                table: "Urunler",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 424, DateTimeKind.Local).AddTicks(1620));

            migrationBuilder.UpdateData(
                table: "Urunler",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 424, DateTimeKind.Local).AddTicks(1630));

            migrationBuilder.UpdateData(
                table: "Urunler",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 424, DateTimeKind.Local).AddTicks(1640));

            migrationBuilder.UpdateData(
                table: "Urunler",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 424, DateTimeKind.Local).AddTicks(1650));

            migrationBuilder.InsertData(
                table: "CariHesaplar",
                columns: new[] { "Id", "Adres", "Aktif", "AnaCariId", "Bakiye", "BlokajNedeni", "CariTipi", "CariTipiDetay", "CekKabulEder", "DeletedAt", "Email", "GuncellemeTarihi", "HksSicilNo", "IlId", "IlceId", "IsDeleted", "KasaBakiye", "Kod", "MevcutRehinKasa", "OlusturmaTarihi", "RehinLimiti", "RiskLimiti", "SatisBlokajli", "SenetKabulEder", "SyncDurumu", "TcKimlikNo", "Telefon", "Telefon2", "Unvan", "VarsayilanVadeGun", "VergiDairesi", "VergiNo" },
                values: new object[,]
                {
                    { new Guid("a0000001-0000-0000-0000-000000000004"), null, true, null, 0m, null, 4, 6, true, null, null, null, null, 33, null, false, 0m, "ALC001", 0, new DateTime(2025, 12, 4, 20, 4, 4, 423, DateTimeKind.Local).AddTicks(5010), 0, 0m, false, false, 0, null, "0536 555 6677", null, "NAZMI DOĞAN", 30, null, null },
                    { new Guid("a0000001-0000-0000-0000-000000000005"), null, true, null, 0m, null, 4, 6, true, null, null, null, null, 33, null, false, 0m, "ALC002", 0, new DateTime(2025, 12, 4, 20, 4, 4, 423, DateTimeKind.Local).AddTicks(5020), 0, 0m, false, false, 0, null, "0537 666 7788", null, "ŞENOL DOĞAN", 30, null, null },
                    { new Guid("c0000001-0000-0000-0000-000000000001"), null, true, null, 0m, null, 2, 2, true, null, null, null, null, 33, null, false, 0m, "KOM001", 0, new DateTime(2025, 12, 4, 20, 4, 4, 401, DateTimeKind.Local).AddTicks(2510), 0, 0m, false, false, 0, null, "0532 111 2233", null, "MEHMET ALİ KOMİSYON", 30, null, null },
                    { new Guid("c0000001-0000-0000-0000-000000000002"), null, true, null, 0m, null, 2, 2, true, null, null, null, null, 33, null, false, 0m, "KOM002", 0, new DateTime(2025, 12, 4, 20, 4, 4, 423, DateTimeKind.Local).AddTicks(4910), 0, 0m, false, false, 0, null, "0533 222 3344", null, "ERKAN CENGİZ KOMİSYON", 30, null, null },
                    { new Guid("c0000001-0000-0000-0000-000000000003"), null, true, null, 0m, null, 2, 2, true, null, null, null, null, 33, null, false, 0m, "KOM003", 0, new DateTime(2025, 12, 4, 20, 4, 4, 423, DateTimeKind.Local).AddTicks(4940), 0, 0m, false, false, 0, null, "0534 333 4455", null, "HALİL KARDEŞLER", 30, null, null },
                    { new Guid("c0000001-0000-0000-0000-000000000004"), null, true, null, 0m, null, 2, 8, true, null, null, null, null, 33, null, false, 0m, "KOM004", 0, new DateTime(2025, 12, 4, 20, 4, 4, 423, DateTimeKind.Local).AddTicks(4950), 0, 0m, false, false, 0, null, "0535 444 5566", null, "DOĞAN TİCARET", 30, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturasiKalemleri_KomisyoncuId",
                table: "SatisFaturasiKalemleri",
                column: "KomisyoncuId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturalari_FaturaTipi",
                table: "SatisFaturalari",
                column: "FaturaTipi");

            migrationBuilder.CreateIndex(
                name: "IX_CariHesaplar_AnaCariId",
                table: "CariHesaplar",
                column: "AnaCariId");

            migrationBuilder.CreateIndex(
                name: "IX_EFaturaKalemleri_EFaturaId",
                table: "EFaturaKalemleri",
                column: "EFaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_EFaturalar_SatisFaturasiId",
                table: "EFaturalar",
                column: "SatisFaturasiId");

            migrationBuilder.AddForeignKey(
                name: "FK_CariHesaplar_CariHesaplar_AnaCariId",
                table: "CariHesaplar",
                column: "AnaCariId",
                principalTable: "CariHesaplar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SatisFaturasiKalemleri_CariHesaplar_KomisyoncuId",
                table: "SatisFaturasiKalemleri",
                column: "KomisyoncuId",
                principalTable: "CariHesaplar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CariHesaplar_CariHesaplar_AnaCariId",
                table: "CariHesaplar");

            migrationBuilder.DropForeignKey(
                name: "FK_SatisFaturasiKalemleri_CariHesaplar_KomisyoncuId",
                table: "SatisFaturasiKalemleri");

            migrationBuilder.DropTable(
                name: "EFaturaAyarlari");

            migrationBuilder.DropTable(
                name: "EFaturaKalemleri");

            migrationBuilder.DropTable(
                name: "EFaturalar");

            migrationBuilder.DropIndex(
                name: "IX_SatisFaturasiKalemleri_KomisyoncuId",
                table: "SatisFaturasiKalemleri");

            migrationBuilder.DropIndex(
                name: "IX_SatisFaturalari_FaturaTipi",
                table: "SatisFaturalari");

            migrationBuilder.DropIndex(
                name: "IX_CariHesaplar_AnaCariId",
                table: "CariHesaplar");

            migrationBuilder.DeleteData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("a0000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("a0000001-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("a0000001-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("a0000001-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("a0000001-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("c0000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("c0000001-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("c0000001-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("c0000001-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Iller",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DropColumn(
                name: "AlisFiyati",
                table: "SatisFaturasiKalemleri");

            migrationBuilder.DropColumn(
                name: "KomisyoncuId",
                table: "SatisFaturasiKalemleri");

            migrationBuilder.DropColumn(
                name: "EkMasrafAciklama",
                table: "SatisFaturalari");

            migrationBuilder.DropColumn(
                name: "EkMasrafTutari",
                table: "SatisFaturalari");

            migrationBuilder.DropColumn(
                name: "AnaCariId",
                table: "CariHesaplar");

            migrationBuilder.UpdateData(
                table: "KapTipleri",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 3, 16, 54, 53, 145, DateTimeKind.Local).AddTicks(7150));

            migrationBuilder.UpdateData(
                table: "KapTipleri",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 3, 16, 54, 53, 145, DateTimeKind.Local).AddTicks(7570));

            migrationBuilder.UpdateData(
                table: "KapTipleri",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 3, 16, 54, 53, 145, DateTimeKind.Local).AddTicks(7580));

            migrationBuilder.UpdateData(
                table: "KesintTanimlari",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000001"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 3, 16, 54, 53, 146, DateTimeKind.Local).AddTicks(40));

            migrationBuilder.UpdateData(
                table: "KesintTanimlari",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000002"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 3, 16, 54, 53, 146, DateTimeKind.Local).AddTicks(650));

            migrationBuilder.UpdateData(
                table: "KesintTanimlari",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000003"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 3, 16, 54, 53, 146, DateTimeKind.Local).AddTicks(660));

            migrationBuilder.UpdateData(
                table: "KesintTanimlari",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000004"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 3, 16, 54, 53, 146, DateTimeKind.Local).AddTicks(660));

            migrationBuilder.UpdateData(
                table: "KesintTanimlari",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000005"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 3, 16, 54, 53, 146, DateTimeKind.Local).AddTicks(750));

            migrationBuilder.UpdateData(
                table: "KesintTanimlari",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000006"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 3, 16, 54, 53, 146, DateTimeKind.Local).AddTicks(750));

            migrationBuilder.UpdateData(
                table: "Kullanicilar",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 3, 16, 54, 53, 145, DateTimeKind.Local).AddTicks(9440));

            migrationBuilder.UpdateData(
                table: "UrunGruplari",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 3, 16, 54, 53, 139, DateTimeKind.Local).AddTicks(8630));

            migrationBuilder.UpdateData(
                table: "UrunGruplari",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 3, 16, 54, 53, 145, DateTimeKind.Local).AddTicks(6640));

            migrationBuilder.UpdateData(
                table: "Urunler",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 3, 16, 54, 53, 145, DateTimeKind.Local).AddTicks(8200));

            migrationBuilder.UpdateData(
                table: "Urunler",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 3, 16, 54, 53, 145, DateTimeKind.Local).AddTicks(8540));

            migrationBuilder.UpdateData(
                table: "Urunler",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 3, 16, 54, 53, 145, DateTimeKind.Local).AddTicks(8550));

            migrationBuilder.UpdateData(
                table: "Urunler",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 3, 16, 54, 53, 145, DateTimeKind.Local).AddTicks(8550));

            migrationBuilder.UpdateData(
                table: "Urunler",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 3, 16, 54, 53, 145, DateTimeKind.Local).AddTicks(8560));
        }
    }
}
