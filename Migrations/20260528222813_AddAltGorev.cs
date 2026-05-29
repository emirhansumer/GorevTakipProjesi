using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GorevTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddAltGorev : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AltGorevler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Metin = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Tamamlandi = table.Column<bool>(type: "INTEGER", nullable: false),
                    Sira = table.Column<int>(type: "INTEGER", nullable: false),
                    GorevId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AltGorevler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AltGorevler_Gorevler_GorevId",
                        column: x => x.GorevId,
                        principalTable: "Gorevler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AltGorevler_GorevId",
                table: "AltGorevler",
                column: "GorevId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AltGorevler");
        }
    }
}
