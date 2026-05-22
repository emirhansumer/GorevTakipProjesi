using GorevTakip.Data;
using GorevTakip.Filters;
using GorevTakip.Helpers;
using GorevTakip.Models;
using GorevTakip.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GorevTakip.Controllers;

[AuthCheck]
public class GorevController : Controller
{
    private readonly AppDbContext _db;

    public GorevController(AppDbContext db)
    {
        _db = db;
    }

    private int AktifKullaniciId => HttpContext.Session.GetKullaniciId()!.Value;

    public async Task<IActionResult> Index(int? kategoriId = null, bool kategorisiz = false)
    {
        ViewBag.Filtre = "tumu";
        ViewBag.Baslik = "Tüm Görevlerim";
        ViewBag.SeciliKategoriId = kategoriId;
        ViewBag.Kategorisiz = kategorisiz;

        if (kategorisiz)
        {
            ViewBag.Baslik = "Kategorisiz Görevler";
        }
        else if (kategoriId.HasValue)
        {
            var kategori = await _db.Kategoriler
                .FirstOrDefaultAsync(k => k.Id == kategoriId && k.KullaniciId == AktifKullaniciId);
            if (kategori is not null)
            {
                ViewBag.Baslik = $"{kategori.Ad} kategorisindeki görevler";
                ViewBag.SeciliKategoriRenk = kategori.Renk;
            }
        }

        await KategoriListesiniDoldur();
        var gorevler = await GorevleriGetir(null, kategoriId, kategorisiz);
        return View(gorevler);
    }

    public async Task<IActionResult> Tamamlanan()
    {
        ViewBag.Baslik = "Tamamlanan Görevler";
        ViewBag.Filtre = "tamamlanan";
        await KategoriListesiniDoldur();
        var gorevler = await GorevleriGetir(GorevDurum.Tamamlandi, null);
        return View("Index", gorevler);
    }

    public async Task<IActionResult> Bekleyen()
    {
        ViewBag.Baslik = "Bekleyen Görevler";
        ViewBag.Filtre = "bekleyen";
        await KategoriListesiniDoldur();
        var gorevler = await GorevleriGetir(GorevDurum.Bekliyor, null);
        return View("Index", gorevler);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var gorev = await _db.Gorevler
            .Include(g => g.Kategori)
            .FirstOrDefaultAsync(g => g.Id == id && g.KullaniciId == AktifKullaniciId);

        if (gorev is null)
            return NotFound();

        return View(gorev);
    }

    private Task<List<Gorev>> GorevleriGetir(GorevDurum? durum, int? kategoriId, bool kategorisiz = false)
    {
        var sorgu = _db.Gorevler
            .Include(g => g.Kategori)
            .Where(g => g.KullaniciId == AktifKullaniciId);

        if (durum.HasValue)
            sorgu = sorgu.Where(g => g.Durum == durum.Value);
        if (kategorisiz)
            sorgu = sorgu.Where(g => g.KategoriId == null);
        else if (kategoriId.HasValue)
            sorgu = sorgu.Where(g => g.KategoriId == kategoriId.Value);

        return sorgu
            .OrderByDescending(g => g.Oncelik)
            .ThenByDescending(g => g.OlusturmaTarihi)
            .ToListAsync();
    }

    private async Task KategoriListesiniDoldur()
    {
        ViewBag.Kategoriler = await _db.Kategoriler
            .Where(k => k.KullaniciId == AktifKullaniciId)
            .OrderBy(k => k.Ad)
            .ToListAsync();
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new GorevFormViewModel
        {
            KullaniciKategorileri = await KullaniciKategorileri()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GorevFormViewModel model)
    {
        if (!await KategoriGecerliMi(model.KategoriId))
            ModelState.AddModelError(nameof(model.KategoriId), "Geçersiz kategori.");

        if (!ModelState.IsValid)
        {
            model.KullaniciKategorileri = await KullaniciKategorileri();
            return View(model);
        }

        var gorev = new Gorev
        {
            Baslik = model.Baslik.Trim(),
            Aciklama = string.IsNullOrWhiteSpace(model.Aciklama) ? null : model.Aciklama.Trim(),
            BitisTarihi = model.BitisTarihi,
            Durum = model.Durum,
            Oncelik = model.Oncelik,
            KategoriId = model.KategoriId,
            OlusturmaTarihi = DateTime.Now,
            KullaniciId = AktifKullaniciId
        };

        _db.Gorevler.Add(gorev);
        await _db.SaveChangesAsync();

        TempData["Basari"] = "Görev başarıyla eklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var gorev = await _db.Gorevler
            .FirstOrDefaultAsync(g => g.Id == id && g.KullaniciId == AktifKullaniciId);

        if (gorev is null)
            return NotFound();

        var model = new GorevFormViewModel
        {
            Id = gorev.Id,
            Baslik = gorev.Baslik,
            Aciklama = gorev.Aciklama,
            BitisTarihi = gorev.BitisTarihi,
            Durum = gorev.Durum,
            Oncelik = gorev.Oncelik,
            KategoriId = gorev.KategoriId,
            KullaniciKategorileri = await KullaniciKategorileri()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GorevFormViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!await KategoriGecerliMi(model.KategoriId))
            ModelState.AddModelError(nameof(model.KategoriId), "Geçersiz kategori.");

        if (!ModelState.IsValid)
        {
            model.KullaniciKategorileri = await KullaniciKategorileri();
            return View(model);
        }

        var gorev = await _db.Gorevler
            .FirstOrDefaultAsync(g => g.Id == id && g.KullaniciId == AktifKullaniciId);

        if (gorev is null)
            return NotFound();

        gorev.Baslik = model.Baslik.Trim();
        gorev.Aciklama = string.IsNullOrWhiteSpace(model.Aciklama) ? null : model.Aciklama.Trim();
        gorev.BitisTarihi = model.BitisTarihi;
        gorev.Durum = model.Durum;
        gorev.Oncelik = model.Oncelik;
        gorev.KategoriId = model.KategoriId;

        await _db.SaveChangesAsync();

        TempData["Basari"] = "Görev güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KategoriDegistir([FromForm] int gorevId, [FromForm] int? kategoriId)
    {
        var gorev = await _db.Gorevler
            .FirstOrDefaultAsync(g => g.Id == gorevId && g.KullaniciId == AktifKullaniciId);
        if (gorev is null)
            return NotFound(new { ok = false, message = "Görev bulunamadı." });

        if (!await KategoriGecerliMi(kategoriId))
            return BadRequest(new { ok = false, message = "Geçersiz kategori." });

        gorev.KategoriId = kategoriId;
        await _db.SaveChangesAsync();

        if (kategoriId.HasValue)
        {
            var kat = await _db.Kategoriler.FirstAsync(k => k.Id == kategoriId.Value);
            return Json(new { ok = true, kategoriId = kat.Id, ad = kat.Ad, renk = kat.Renk });
        }
        return Json(new { ok = true, kategoriId = (int?)null, ad = (string?)null, renk = (string?)null });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var gorev = await _db.Gorevler
            .FirstOrDefaultAsync(g => g.Id == id && g.KullaniciId == AktifKullaniciId);

        if (gorev is null)
            return NotFound();

        _db.Gorevler.Remove(gorev);
        await _db.SaveChangesAsync();

        TempData["Basari"] = "Görev silindi.";
        return RedirectToAction(nameof(Index));
    }

    // --- yardımcı metotlar ---

    private Task<List<Kategori>> KullaniciKategorileri() =>
        _db.Kategoriler
            .Where(k => k.KullaniciId == AktifKullaniciId)
            .OrderBy(k => k.Ad)
            .ToListAsync();

    private async Task<bool> KategoriGecerliMi(int? kategoriId)
    {
        if (!kategoriId.HasValue) return true;
        return await _db.Kategoriler
            .AnyAsync(k => k.Id == kategoriId.Value && k.KullaniciId == AktifKullaniciId);
    }
}
