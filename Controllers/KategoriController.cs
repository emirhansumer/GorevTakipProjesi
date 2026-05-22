using GorevTakip.Data;
using GorevTakip.Filters;
using GorevTakip.Helpers;
using GorevTakip.Models;
using GorevTakip.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GorevTakip.Controllers;

[AuthCheck]
public class KategoriController : Controller
{
    private readonly AppDbContext _db;

    public KategoriController(AppDbContext db)
    {
        _db = db;
    }

    private int AktifKullaniciId => HttpContext.Session.GetKullaniciId()!.Value;

    public async Task<IActionResult> Index()
    {
        var kategoriler = await _db.Kategoriler
            .Where(k => k.KullaniciId == AktifKullaniciId)
            .Select(k => new
            {
                Kategori = k,
                GorevSayisi = k.Gorevler.Count
            })
            .OrderBy(x => x.Kategori.Ad)
            .ToListAsync();

        ViewBag.GorevSayilari = kategoriler.ToDictionary(x => x.Kategori.Id, x => x.GorevSayisi);
        return View(kategoriler.Select(x => x.Kategori).ToList());
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new KategoriFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(KategoriFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var ayniIsim = await _db.Kategoriler
            .AnyAsync(k => k.KullaniciId == AktifKullaniciId &&
                          k.Ad.ToLower() == model.Ad.Trim().ToLower());
        if (ayniIsim)
        {
            ModelState.AddModelError(nameof(model.Ad), "Bu isimde bir kategorin zaten var.");
            return View(model);
        }

        var kategori = new Kategori
        {
            Ad = model.Ad.Trim(),
            Renk = model.Renk,
            KullaniciId = AktifKullaniciId,
            OlusturmaTarihi = DateTime.Now
        };

        _db.Kategoriler.Add(kategori);
        await _db.SaveChangesAsync();

        TempData["Basari"] = "Kategori eklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var kategori = await _db.Kategoriler
            .FirstOrDefaultAsync(k => k.Id == id && k.KullaniciId == AktifKullaniciId);

        if (kategori is null)
            return NotFound();

        return View(new KategoriFormViewModel
        {
            Id = kategori.Id,
            Ad = kategori.Ad,
            Renk = kategori.Renk
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, KategoriFormViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var kategori = await _db.Kategoriler
            .FirstOrDefaultAsync(k => k.Id == id && k.KullaniciId == AktifKullaniciId);

        if (kategori is null) return NotFound();

        var ayniIsim = await _db.Kategoriler
            .AnyAsync(k => k.KullaniciId == AktifKullaniciId &&
                          k.Id != id &&
                          k.Ad.ToLower() == model.Ad.Trim().ToLower());
        if (ayniIsim)
        {
            ModelState.AddModelError(nameof(model.Ad), "Bu isimde bir kategorin zaten var.");
            return View(model);
        }

        kategori.Ad = model.Ad.Trim();
        kategori.Renk = model.Renk;
        await _db.SaveChangesAsync();

        TempData["Basari"] = "Kategori güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var kategori = await _db.Kategoriler
            .FirstOrDefaultAsync(k => k.Id == id && k.KullaniciId == AktifKullaniciId);

        if (kategori is null) return NotFound();

        // Bu kategoriye atanmış görevler "Kategorisiz" olur (KategoriId=null) — OnDelete=SetNull
        _db.Kategoriler.Remove(kategori);
        await _db.SaveChangesAsync();

        TempData["Basari"] = "Kategori silindi.";
        return RedirectToAction(nameof(Index));
    }
}
