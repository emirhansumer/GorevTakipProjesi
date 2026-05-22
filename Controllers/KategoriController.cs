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
        var liste = await _db.Kategoriler
            .Where(k => k.KullaniciId == AktifKullaniciId)
            .Select(k => new KategoriIstatistik
            {
                Kategori = k,
                Toplam = k.Gorevler.Count,
                Tamamlanan = k.Gorevler.Count(g => g.Durum == GorevDurum.Tamamlandi)
            })
            .OrderBy(x => x.Kategori.Ad)
            .ToListAsync();

        return View(liste);
    }

    public class KategoriIstatistik
    {
        public Kategori Kategori { get; set; } = default!;
        public int Toplam { get; set; }
        public int Tamamlanan { get; set; }
        public int Yuzde => Toplam == 0 ? 0 : (int)Math.Round((double)Tamamlanan * 100 / Toplam);
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
    public async Task<IActionResult> HizliEkle([FromForm] KategoriFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var hatalar = ModelState
                .Where(kv => kv.Value!.Errors.Count > 0)
                .ToDictionary(kv => kv.Key, kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
            return BadRequest(new { ok = false, errors = hatalar });
        }

        var ad = model.Ad.Trim();
        var ayniIsim = await _db.Kategoriler
            .AnyAsync(k => k.KullaniciId == AktifKullaniciId &&
                          k.Ad.ToLower() == ad.ToLower());
        if (ayniIsim)
        {
            return BadRequest(new { ok = false, message = "Bu isimde bir kategorin zaten var." });
        }

        var kategori = new Kategori
        {
            Ad = ad,
            Renk = model.Renk,
            KullaniciId = AktifKullaniciId,
            OlusturmaTarihi = DateTime.Now
        };

        _db.Kategoriler.Add(kategori);
        await _db.SaveChangesAsync();

        return Json(new { ok = true, id = kategori.Id, ad = kategori.Ad, renk = kategori.Renk });
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
