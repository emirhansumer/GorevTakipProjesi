using GorevTakip.Models;
using Microsoft.EntityFrameworkCore;

namespace GorevTakip.Data;

public static class DbSeeder
{
    // Sabit yönetici hesabı bilgileri. Demoda "admin ile giriş" için kullanılır.
    public const string AdminEmail = "admin@gorevtakip.com";
    private const string AdminSifre = "Admin1234";

    // Uygulama açılışında çağrılır: bekleyen migration'ları uygular ve
    // admin hesabı yoksa oluşturur (idempotent — tekrar tekrar çalışsa da sorun olmaz).
    public static async Task BaslangicVerileriniHazirla(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        var adminVar = await db.Kullanicilar.AnyAsync(k => k.Email == AdminEmail);
        if (!adminVar)
        {
            db.Kullanicilar.Add(new Kullanici
            {
                AdSoyad = "Site Yöneticisi",
                Email = AdminEmail,
                SifreHash = BCrypt.Net.BCrypt.HashPassword(AdminSifre),
                IsAdmin = true,
                Aktif = true,
                KayitTarihi = DateTime.Now
            });
            await db.SaveChangesAsync();
        }

        // Tek satırlık site ayarı yoksa varsayılanı oluştur
        var ayarVar = await db.SiteAyarlari.AnyAsync();
        if (!ayarVar)
        {
            db.SiteAyarlari.Add(new SiteAyar { BakimModu = false, KayitAcik = true, DuyuruAktif = false });
            await db.SaveChangesAsync();
        }
    }
}
