using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoHal.Data.Migrations
{
    /// <inheritdoc />
    public partial class GirisKalemKomisyoncu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "KomisyoncuId",
                table: "GirisIrsaliyesiKalemleri",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("a0000001-0000-0000-0000-000000000001"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 245, DateTimeKind.Local).AddTicks(7010));

            migrationBuilder.UpdateData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("a0000001-0000-0000-0000-000000000002"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 245, DateTimeKind.Local).AddTicks(7010));

            migrationBuilder.UpdateData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("a0000001-0000-0000-0000-000000000003"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 245, DateTimeKind.Local).AddTicks(7020));

            migrationBuilder.UpdateData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("a0000001-0000-0000-0000-000000000004"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 245, DateTimeKind.Local).AddTicks(7020));

            migrationBuilder.UpdateData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("a0000001-0000-0000-0000-000000000005"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 245, DateTimeKind.Local).AddTicks(7030));

            migrationBuilder.UpdateData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("c0000001-0000-0000-0000-000000000001"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 239, DateTimeKind.Local).AddTicks(6120));

            migrationBuilder.UpdateData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("c0000001-0000-0000-0000-000000000002"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 245, DateTimeKind.Local).AddTicks(6980));

            migrationBuilder.UpdateData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("c0000001-0000-0000-0000-000000000003"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 245, DateTimeKind.Local).AddTicks(7000));

            migrationBuilder.UpdateData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("c0000001-0000-0000-0000-000000000004"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 245, DateTimeKind.Local).AddTicks(7000));

            migrationBuilder.UpdateData(
                table: "KapTipleri",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 245, DateTimeKind.Local).AddTicks(8170));

            migrationBuilder.UpdateData(
                table: "KapTipleri",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 245, DateTimeKind.Local).AddTicks(8580));

            migrationBuilder.UpdateData(
                table: "KapTipleri",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 245, DateTimeKind.Local).AddTicks(8580));

            migrationBuilder.UpdateData(
                table: "KesintTanimlari",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000001"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 246, DateTimeKind.Local).AddTicks(1000));

            migrationBuilder.UpdateData(
                table: "KesintTanimlari",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000002"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 246, DateTimeKind.Local).AddTicks(1700));

            migrationBuilder.UpdateData(
                table: "KesintTanimlari",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000003"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 246, DateTimeKind.Local).AddTicks(1710));

            migrationBuilder.UpdateData(
                table: "KesintTanimlari",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000004"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 246, DateTimeKind.Local).AddTicks(1710));

            migrationBuilder.UpdateData(
                table: "KesintTanimlari",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000005"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 246, DateTimeKind.Local).AddTicks(1810));

            migrationBuilder.UpdateData(
                table: "KesintTanimlari",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000006"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 246, DateTimeKind.Local).AddTicks(1810));

            migrationBuilder.UpdateData(
                table: "Kullanicilar",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 246, DateTimeKind.Local).AddTicks(320));

            migrationBuilder.UpdateData(
                table: "UrunGruplari",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 245, DateTimeKind.Local).AddTicks(7610));

            migrationBuilder.UpdateData(
                table: "UrunGruplari",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 245, DateTimeKind.Local).AddTicks(7810));

            migrationBuilder.UpdateData(
                table: "Urunler",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 245, DateTimeKind.Local).AddTicks(9040));

            migrationBuilder.UpdateData(
                table: "Urunler",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 245, DateTimeKind.Local).AddTicks(9330));

            migrationBuilder.UpdateData(
                table: "Urunler",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 245, DateTimeKind.Local).AddTicks(9340));

            migrationBuilder.UpdateData(
                table: "Urunler",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 245, DateTimeKind.Local).AddTicks(9340));

            migrationBuilder.UpdateData(
                table: "Urunler",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 26, 1, 245, DateTimeKind.Local).AddTicks(9350));

            migrationBuilder.CreateIndex(
                name: "IX_GirisIrsaliyesiKalemleri_KomisyoncuId",
                table: "GirisIrsaliyesiKalemleri",
                column: "KomisyoncuId");

            migrationBuilder.AddForeignKey(
                name: "FK_GirisIrsaliyesiKalemleri_CariHesaplar_KomisyoncuId",
                table: "GirisIrsaliyesiKalemleri",
                column: "KomisyoncuId",
                principalTable: "CariHesaplar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GirisIrsaliyesiKalemleri_CariHesaplar_KomisyoncuId",
                table: "GirisIrsaliyesiKalemleri");

            migrationBuilder.DropIndex(
                name: "IX_GirisIrsaliyesiKalemleri_KomisyoncuId",
                table: "GirisIrsaliyesiKalemleri");

            migrationBuilder.DropColumn(
                name: "KomisyoncuId",
                table: "GirisIrsaliyesiKalemleri");

            migrationBuilder.UpdateData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("a0000001-0000-0000-0000-000000000001"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 423, DateTimeKind.Local).AddTicks(4960));

            migrationBuilder.UpdateData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("a0000001-0000-0000-0000-000000000002"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 423, DateTimeKind.Local).AddTicks(4990));

            migrationBuilder.UpdateData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("a0000001-0000-0000-0000-000000000003"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 423, DateTimeKind.Local).AddTicks(5000));

            migrationBuilder.UpdateData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("a0000001-0000-0000-0000-000000000004"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 423, DateTimeKind.Local).AddTicks(5010));

            migrationBuilder.UpdateData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("a0000001-0000-0000-0000-000000000005"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 423, DateTimeKind.Local).AddTicks(5020));

            migrationBuilder.UpdateData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("c0000001-0000-0000-0000-000000000001"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 401, DateTimeKind.Local).AddTicks(2510));

            migrationBuilder.UpdateData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("c0000001-0000-0000-0000-000000000002"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 423, DateTimeKind.Local).AddTicks(4910));

            migrationBuilder.UpdateData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("c0000001-0000-0000-0000-000000000003"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 423, DateTimeKind.Local).AddTicks(4940));

            migrationBuilder.UpdateData(
                table: "CariHesaplar",
                keyColumn: "Id",
                keyValue: new Guid("c0000001-0000-0000-0000-000000000004"),
                column: "OlusturmaTarihi",
                value: new DateTime(2025, 12, 4, 20, 4, 4, 423, DateTimeKind.Local).AddTicks(4950));

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
        }
    }
}
