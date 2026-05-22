using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GorevTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddKategoriIkon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ikon",
                table: "Kategoriler",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ikon",
                table: "Kategoriler");
        }
    }
}
