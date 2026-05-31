using GorevTakip.Data;
using GorevTakip.Helpers;
using GorevTakip.Models;
using Microsoft.EntityFrameworkCore;

namespace GorevTakip.Middleware;

// Her istekte site ayarını yükler (HttpContext.Items'a koyar — layout/banner kullanır)
// ve bakım modu açıksa admin olmayanları "bakımda" sayfasına yönlendirir.
public class SiteAyarMiddleware
{
    private readonly RequestDelegate _next;

    public SiteAyarMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var ayar = await db.SiteAyarlari.AsNoTracking().FirstOrDefaultAsync() ?? new SiteAyar();
        context.Items["SiteAyar"] = ayar;

        var uid = context.Session.GetKullaniciId();
        if (uid.HasValue)
        {
            var hesap = await db.Kullanicilar.AsNoTracking()
                .Where(k => k.Id == uid.Value)
                .Select(k => new { k.Aktif })
                .FirstOrDefaultAsync();

            var path = context.Request.Path.Value ?? string.Empty;
            var accountYolu = path.StartsWith("/Account", StringComparison.OrdinalIgnoreCase);

            // Hesap silinmiş ya da pasife alınmışsa oturumu sonlandır (her istekte kontrol)
            if ((hesap is null || !hesap.Aktif) && !accountYolu)
            {
                context.Session.CikisYap();
                context.Response.Redirect("/Account/Login");
                return;
            }

            // Bekleyen davet sayısı (navbar rozeti için)
            context.Items["BekleyenDavet"] = await db.ProjeDavetleri
                .CountAsync(d => d.KullaniciId == uid.Value && d.Durum == DavetDurum.Bekliyor);

            // Yöneticiye okunmamış iletişim mesajı sayısı (navbar rozeti için)
            if (context.Session.AdminMi())
            {
                context.Items["OkunmamisMesaj"] = await db.IletisimMesajlari.CountAsync(m => !m.Okundu);
            }
        }

        if (ayar.BakimModu && !context.Session.AdminMi())
        {
            var path = context.Request.Path.Value ?? string.Empty;
            // Bakım modunda bile erişilebilir: giriş/çıkış, admin paneli, bakım/yasal/iletişim sayfaları
            var izinli = path.StartsWith("/Account", StringComparison.OrdinalIgnoreCase)
                      || path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase)
                      || path.StartsWith("/Home/Bakim", StringComparison.OrdinalIgnoreCase)
                      || path.StartsWith("/Home/Kvkk", StringComparison.OrdinalIgnoreCase)
                      || path.StartsWith("/Home/Iletisim", StringComparison.OrdinalIgnoreCase);

            if (!izinli)
            {
                context.Response.Redirect("/Home/Bakim");
                return;
            }
        }

        await _next(context);
    }
}
