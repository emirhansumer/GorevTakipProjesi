using System.Diagnostics;
using GorevTakip.Data;
using GorevTakip.Filters;
using GorevTakip.Helpers;
using GorevTakip.Models;
using GorevTakip.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GorevTakip.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    [AuthCheck]
    public async Task<IActionResult> Index()
    {
        var kullaniciId = HttpContext.Session.GetKullaniciId()!.Value;

        var gorevler = await _db.Gorevler
            .Where(g => g.KullaniciId == kullaniciId)
            .ToListAsync();

        var model = new DashboardViewModel
        {
            AdSoyad = HttpContext.Session.GetAdSoyad() ?? "",
            ToplamGorev = gorevler.Count,
            BekleyenGorev = gorevler.Count(g => g.Durum == GorevDurum.Bekliyor),
            TamamlananGorev = gorevler.Count(g => g.Durum == GorevDurum.Tamamlandi),
            IptalGorev = gorevler.Count(g => g.Durum == GorevDurum.Iptal),
            SonGorevler = gorevler
                .OrderByDescending(g => g.OlusturmaTarihi)
                .Take(5)
                .ToList()
        };

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("Home/StatusCode/{code:int}")]
    public IActionResult HataKodu(int code)
    {
        ViewBag.Code = code;
        ViewBag.Mesaj = code switch
        {
            404 => "Aradığın sayfa bulunamadı.",
            403 => "Bu sayfaya erişim yetkin yok.",
            401 => "Bu sayfayı görmek için giriş yapmalısın.",
            500 => "Beklenmeyen bir hata oluştu.",
            _ => "Bir hata oluştu."
        };
        ViewBag.Baslik = code switch
        {
            404 => "Sayfa Bulunamadı",
            403 => "Erişim Engellendi",
            401 => "Yetkisiz Erişim",
            500 => "Sunucu Hatası",
            _ => "Hata"
        };
        return View("StatusCode");
    }
}
