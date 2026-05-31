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

        // Giriş yapan kullanıcının bekleyen davet sayısı (navbar rozeti için)
        var uid = context.Session.GetKullaniciId();
        if (uid.HasValue)
        {
            context.Items["BekleyenDavet"] = await db.ProjeDavetleri
                .CountAsync(d => d.KullaniciId == uid.Value && d.Durum == DavetDurum.Bekliyor);
        }

        if (ayar.BakimModu && !context.Session.AdminMi())
        {
            var path = context.Request.Path.Value ?? string.Empty;
            // Bakım modunda bile erişilebilir: giriş/çıkış, admin paneli, bakım sayfası
            var izinli = path.StartsWith("/Account", StringComparison.OrdinalIgnoreCase)
                      || path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase)
                      || path.StartsWith("/Home/Bakim", StringComparison.OrdinalIgnoreCase);

            if (!izinli)
            {
                context.Response.Redirect("/Home/Bakim");
                return;
            }
        }

        await _next(context);
    }
}
