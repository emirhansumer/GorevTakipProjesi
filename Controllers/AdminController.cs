using GorevTakip.Data;
using GorevTakip.Filters;
using GorevTakip.Helpers;
using GorevTakip.Models;
using GorevTakip.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GorevTakip.Controllers;

[AdminCheck]
public class AdminController : Controller
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    private int AktifKullaniciId => HttpContext.Session.GetKullaniciId()!.Value;

    // Dashboard — sistem genelinde istatistikler
    public async Task<IActionResult> Index()
    {
        var gorevler = await _db.Gorevler.ToListAsync();

        var kullaniciOzetleri = await _db.Kullanicilar
            .Select(k => new KullaniciOzet
            {
                Kullanici = k,
                GorevSayisi = k.Gorevler.Count,
                KategoriSayisi = k.Kategoriler.Count
            })
            .ToListAsync();

        var model = new AdminDashboardViewModel
        {
            ToplamKullanici = kullaniciOzetleri.Count,
            ToplamGorev = gorevler.Count,
            ToplamKategori = await _db.Kategoriler.CountAsync(),
            ToplamAltGorev = await _db.AltGorevler.CountAsync(),
            BekleyenGorev = gorevler.Count(g => g.Durum == GorevDurum.Bekliyor),
            TamamlananGorev = gorevler.Count(g => g.Durum == GorevDurum.Tamamlandi),
            IptalGorev = gorevler.Count(g => g.Durum == GorevDurum.Iptal),
            SonKullanicilar = kullaniciOzetleri
                .OrderByDescending(x => x.Kullanici.KayitTarihi)
                .Take(5)
                .ToList(),
            EnAktifKullanicilar = kullaniciOzetleri
                .Where(x => x.GorevSayisi > 0)
                .OrderByDescending(x => x.GorevSayisi)
                .Take(5)
                .ToList()
        };

        return View(model);
    }

    // Tüm kullanıcılar — görev/kategori sayılarıyla
    public async Task<IActionResult> Kullanicilar()
    {
        var liste = await _db.Kullanicilar
            .OrderByDescending(k => k.IsAdmin)
            .ThenBy(k => k.AdSoyad)
            .Select(k => new KullaniciOzet
            {
                Kullanici = k,
                GorevSayisi = k.Gorevler.Count,
                KategoriSayisi = k.Kategoriler.Count
            })
            .ToListAsync();

        return View(liste);
    }

    // Kullanıcı sil — ilişkili görev/kategori/alt görevleri cascade siler
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KullaniciSil(int id)
    {
        if (id == AktifKullaniciId)
        {
            TempData["Hata"] = "Kendi hesabını silemezsin.";
            return RedirectToAction(nameof(Kullanicilar));
        }

        var kullanici = await _db.Kullanicilar.FindAsync(id);
        if (kullanici is null)
        {
            TempData["Hata"] = "Kullanıcı bulunamadı.";
            return RedirectToAction(nameof(Kullanicilar));
        }

        _db.Kullanicilar.Remove(kullanici);
        await _db.SaveChangesAsync();

        TempData["Basari"] = $"\"{kullanici.AdSoyad}\" kullanıcısı ve tüm verileri silindi.";
        return RedirectToAction(nameof(Kullanicilar));
    }

    // Admin yetkisini aç/kapat — kendi yetkini değiştiremezsin (kilitlenme önlemi)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminYetkiToggle(int id)
    {
        if (id == AktifKullaniciId)
        {
            TempData["Hata"] = "Kendi yetkini değiştiremezsin.";
            return RedirectToAction(nameof(Kullanicilar));
        }

        var kullanici = await _db.Kullanicilar.FindAsync(id);
        if (kullanici is null)
        {
            TempData["Hata"] = "Kullanıcı bulunamadı.";
            return RedirectToAction(nameof(Kullanicilar));
        }

        kullanici.IsAdmin = !kullanici.IsAdmin;
        await _db.SaveChangesAsync();

        TempData["Basari"] = kullanici.IsAdmin
            ? $"\"{kullanici.AdSoyad}\" artık admin."
            : $"\"{kullanici.AdSoyad}\" kullanıcısının admin yetkisi kaldırıldı.";
        return RedirectToAction(nameof(Kullanicilar));
    }

    // Tüm görevler — kullanıcıya/duruma göre filtreli
    public async Task<IActionResult> Gorevler(GorevDurum? durum, int? kullaniciId)
    {
        var sorgu = _db.Gorevler
            .Include(g => g.Kullanici)
            .Include(g => g.Kategori)
            .AsQueryable();

        if (durum.HasValue)
            sorgu = sorgu.Where(g => g.Durum == durum.Value);
        if (kullaniciId.HasValue)
            sorgu = sorgu.Where(g => g.KullaniciId == kullaniciId.Value);

        var model = new AdminGorevlerViewModel
        {
            Gorevler = await sorgu.OrderByDescending(g => g.OlusturmaTarihi).ToListAsync(),
            Kullanicilar = await _db.Kullanicilar.OrderBy(k => k.AdSoyad).ToListAsync(),
            SeciliDurum = durum,
            SeciliKullaniciId = kullaniciId
        };

        return View(model);
    }

    // Herhangi bir kullanıcının görevini sil
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GorevSil(int id)
    {
        var gorev = await _db.Gorevler.FindAsync(id);
        if (gorev is null)
        {
            TempData["Hata"] = "Görev bulunamadı.";
            return RedirectToAction(nameof(Gorevler));
        }

        _db.Gorevler.Remove(gorev);
        await _db.SaveChangesAsync();

        TempData["Basari"] = "Görev silindi.";
        return RedirectToAction(nameof(Gorevler));
    }
}
