using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GorevTakip.Migrations
{
    /// <inheritdoc />
    public partial class ProjeTakimVeRol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsAdmin",
                table: "Kullanicilar",
                newName: "Rol");

            // Eski yöneticiler (IsAdmin=1 → şu an Rol=1) Yönetici rolüne (2) yükseltilir.
            // Bu noktada Rol=1 yalnızca eski adminleri ifade eder (henüz proje lideri yok).
            migrationBuilder.Sql("UPDATE \"Kullanicilar\" SET \"Rol\" = 2 WHERE \"Rol\" = 1;");

            migrationBuilder.AddColumn<int>(
                name: "AtayanId",
                table: "Gorevler",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjeId",
                table: "Gorevler",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Projeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LiderId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projeler_Kullanicilar_LiderId",
                        column: x => x.LiderId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjeDavetleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjeId = table.Column<int>(type: "INTEGER", nullable: false),
                    KullaniciId = table.Column<int>(type: "INTEGER", nullable: false),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjeDavetleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjeDavetleri_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjeDavetleri_Projeler_ProjeId",
                        column: x => x.ProjeId,
                        principalTable: "Projeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjeUyeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjeId = table.Column<int>(type: "INTEGER", nullable: false),
                    KullaniciId = table.Column<int>(type: "INTEGER", nullable: false),
                    KatilmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjeUyeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjeUyeleri_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjeUyeleri_Projeler_ProjeId",
                        column: x => x.ProjeId,
                        principalTable: "Projeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gorevler_AtayanId",
                table: "Gorevler",
                column: "AtayanId");

            migrationBuilder.CreateIndex(
                name: "IX_Gorevler_ProjeId",
                table: "Gorevler",
                column: "ProjeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjeDavetleri_KullaniciId",
                table: "ProjeDavetleri",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjeDavetleri_ProjeId",
                table: "ProjeDavetleri",
                column: "ProjeId");

            migrationBuilder.CreateIndex(
                name: "IX_Projeler_LiderId",
                table: "Projeler",
                column: "LiderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjeUyeleri_KullaniciId",
                table: "ProjeUyeleri",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjeUyeleri_ProjeId_KullaniciId",
                table: "ProjeUyeleri",
                columns: new[] { "ProjeId", "KullaniciId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevler_Kullanicilar_AtayanId",
                table: "Gorevler",
                column: "AtayanId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevler_Projeler_ProjeId",
                table: "Gorevler",
                column: "ProjeId",
                principalTable: "Projeler",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gorevler_Kullanicilar_AtayanId",
                table: "Gorevler");

            migrationBuilder.DropForeignKey(
                name: "FK_Gorevler_Projeler_ProjeId",
                table: "Gorevler");

            migrationBuilder.DropTable(
                name: "ProjeDavetleri");

            migrationBuilder.DropTable(
                name: "ProjeUyeleri");

            migrationBuilder.DropTable(
                name: "Projeler");

            migrationBuilder.DropIndex(
                name: "IX_Gorevler_AtayanId",
                table: "Gorevler");

            migrationBuilder.DropIndex(
                name: "IX_Gorevler_ProjeId",
                table: "Gorevler");

            migrationBuilder.DropColumn(
                name: "AtayanId",
                table: "Gorevler");

            migrationBuilder.DropColumn(
                name: "ProjeId",
                table: "Gorevler");

            migrationBuilder.RenameColumn(
                name: "Rol",
                table: "Kullanicilar",
                newName: "IsAdmin");
        }
    }
}
