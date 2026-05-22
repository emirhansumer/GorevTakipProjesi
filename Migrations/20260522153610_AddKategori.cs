using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GorevTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddKategori : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KategoriId",
                table: "Gorevler",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Kategoriler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Renk = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    KullaniciId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kategoriler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kategoriler_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gorevler_KategoriId",
                table: "Gorevler",
                column: "KategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_Kategoriler_KullaniciId",
                table: "Kategoriler",
                column: "KullaniciId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevler_Kategoriler_KategoriId",
                table: "Gorevler",
                column: "KategoriId",
                principalTable: "Kategoriler",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gorevler_Kategoriler_KategoriId",
                table: "Gorevler");

            migrationBuilder.DropTable(
                name: "Kategoriler");

            migrationBuilder.DropIndex(
                name: "IX_Gorevler_KategoriId",
                table: "Gorevler");

            migrationBuilder.DropColumn(
                name: "KategoriId",
                table: "Gorevler");
        }
    }
}
