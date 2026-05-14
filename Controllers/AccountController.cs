using GorevTakip.Data;
using GorevTakip.Filters;
using GorevTakip.Helpers;
using GorevTakip.Models;
using GorevTakip.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GorevTakip.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;

    public AccountController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (HttpContext.Session.GirisYapildiMi())
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var emailVar = await _db.Kullanicilar.AnyAsync(k => k.Email == model.Email);
        if (emailVar)
        {
            ModelState.AddModelError(nameof(model.Email), "Bu e-posta zaten kayıtlı.");
            return View(model);
        }

        var kullanici = new Kullanici
        {
            AdSoyad = model.AdSoyad.Trim(),
            Email = model.Email.Trim().ToLowerInvariant(),
            SifreHash = BCrypt.Net.BCrypt.HashPassword(model.Sifre),
            KayitTarihi = DateTime.Now
        };

        _db.Kullanicilar.Add(kullanici);
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "Kayıt başarılı. Lütfen giriş yapın.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.GirisYapildiMi())
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var email = model.Email.Trim().ToLowerInvariant();
        var kullanici = await _db.Kullanicilar.FirstOrDefaultAsync(k => k.Email == email);

        if (kullanici is null || !BCrypt.Net.BCrypt.Verify(model.Sifre, kullanici.SifreHash))
        {
            ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
            return View(model);
        }

        HttpContext.Session.GirisYap(kullanici.Id, kullanici.AdSoyad, kullanici.Email);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.CikisYap();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AuthCheck]
    public async Task<IActionResult> Profile()
    {
        var kullaniciId = HttpContext.Session.GetKullaniciId()!.Value;

        var kullanici = await _db.Kullanicilar.FindAsync(kullaniciId);
        if (kullanici is null)
        {
            HttpContext.Session.CikisYap();
            return RedirectToAction(nameof(Login));
        }

        var toplamGorev = await _db.Gorevler.CountAsync(g => g.KullaniciId == kullaniciId);

        var model = new ProfileViewModel
        {
            KullaniciId = kullanici.Id,
            AdSoyad = kullanici.AdSoyad,
            Email = kullanici.Email,
            KayitTarihi = kullanici.KayitTarihi,
            ToplamGorev = toplamGorev,
            SessionId = HttpContext.Session.Id,
            SessionAktif = HttpContext.Session.IsAvailable
        };

        return View(model);
    }
}
