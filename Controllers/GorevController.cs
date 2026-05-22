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

    public async Task<IActionResult> Index()
    {
        ViewBag.Baslik = "Tüm Görevlerim";
        ViewBag.Filtre = "tumu";
        var gorevler = await GorevleriGetir(null);
        return View(gorevler);
    }

    public async Task<IActionResult> Tamamlanan()
    {
        ViewBag.Baslik = "Tamamlanan Görevler";
        ViewBag.Filtre = "tamamlanan";
        var gorevler = await GorevleriGetir(GorevDurum.Tamamlandi);
        return View("Index", gorevler);
    }

    public async Task<IActionResult> Bekleyen()
    {
        ViewBag.Baslik = "Bekleyen Görevler";
        ViewBag.Filtre = "bekleyen";
        var gorevler = await GorevleriGetir(GorevDurum.Bekliyor);
        return View("Index", gorevler);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var gorev = await _db.Gorevler
            .FirstOrDefaultAsync(g => g.Id == id && g.KullaniciId == AktifKullaniciId);

        if (gorev is null)
            return NotFound();

        return View(gorev);
    }

    private Task<List<Gorev>> GorevleriGetir(GorevDurum? durum)
    {
        var sorgu = _db.Gorevler.Where(g => g.KullaniciId == AktifKullaniciId);
        if (durum.HasValue)
            sorgu = sorgu.Where(g => g.Durum == durum.Value);
        // Önce öncelik (yüksek üstte), sonra oluşturma tarihi (yeni üstte)
        return sorgu
            .OrderByDescending(g => g.Oncelik)
            .ThenByDescending(g => g.OlusturmaTarihi)
            .ToListAsync();
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new GorevFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GorevFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var gorev = new Gorev
        {
            Baslik = model.Baslik.Trim(),
            Aciklama = string.IsNullOrWhiteSpace(model.Aciklama) ? null : model.Aciklama.Trim(),
            BitisTarihi = model.BitisTarihi,
            Durum = model.Durum,
            Oncelik = model.Oncelik,
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
            Oncelik = gorev.Oncelik
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GorevFormViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var gorev = await _db.Gorevler
            .FirstOrDefaultAsync(g => g.Id == id && g.KullaniciId == AktifKullaniciId);

        if (gorev is null)
            return NotFound();

        gorev.Baslik = model.Baslik.Trim();
        gorev.Aciklama = string.IsNullOrWhiteSpace(model.Aciklama) ? null : model.Aciklama.Trim();
        gorev.BitisTarihi = model.BitisTarihi;
        gorev.Durum = model.Durum;
        gorev.Oncelik = model.Oncelik;

        await _db.SaveChangesAsync();

        TempData["Basari"] = "Görev güncellendi.";
        return RedirectToAction(nameof(Index));
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
}
