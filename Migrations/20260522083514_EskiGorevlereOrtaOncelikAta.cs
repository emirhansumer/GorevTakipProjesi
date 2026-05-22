using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GorevTakip.Migrations
{
    /// <inheritdoc />
    public partial class EskiGorevlereOrtaOncelikAta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AddOncelik migration'ından sonra eski kayıtlar Oncelik=0 ile kaldı.
            // Enum 1'den başlıyor (Düşük=1), bu yüzden eski kayıtları "Orta" (2) yapıyoruz.
            migrationBuilder.Sql("UPDATE Gorevler SET Oncelik = 2 WHERE Oncelik = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri alma: eski kayıtları tekrar 0 yapmaya gerek yok, anlamsız.
        }
    }
}
