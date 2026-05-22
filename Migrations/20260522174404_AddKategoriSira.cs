using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GorevTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddKategoriSira : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Sira",
                table: "Kategoriler",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            // Mevcut kayıtların Sira değerini kullanıcı bazında Id'ye göre 1,2,3... şeklinde ata.
            // SQLite ROW_NUMBER() PARTITION BY desteklediği için tek update ile yapılabilir.
            migrationBuilder.Sql(@"
                UPDATE Kategoriler
                SET Sira = (
                    SELECT sira_no
                    FROM (
                        SELECT Id AS kId,
                               ROW_NUMBER() OVER (PARTITION BY KullaniciId ORDER BY Id) AS sira_no
                        FROM Kategoriler
                    ) AS sub
                    WHERE sub.kId = Kategoriler.Id
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sira",
                table: "Kategoriler");
        }
    }
}
