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

    public async Task<IActionResult> Index(int? kategoriId = null, bool kategorisiz = false, string? q = null)
    {
        ViewBag.Filtre = "tumu";
        ViewBag.Baslik = "Tüm Görevlerim";
        ViewBag.SeciliKategoriId = kategoriId;
        ViewBag.Kategorisiz = kategorisiz;
        ViewBag.AramaSorgusu = q;

        if (!string.IsNullOrWhiteSpace(q))
        {
            ViewBag.Baslik = $"\"{q}\" arama sonuçları";
        }
        else if (kategorisiz)
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
        var gorevler = await GorevleriGetir(null, kategoriId, kategorisiz, q);
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

    // Takvim sayfası (FullCalendar.js render eder, view boş bir container)
    public IActionResult Takvim()
    {
        return View();
    }

    // FullCalendar AJAX endpoint — bitiş tarihi olan görevleri JSON event olarak döner
    [HttpGet]
    public async Task<IActionResult> TakvimVerileri(DateTime? start, DateTime? end)
    {
        var sorgu = _db.Gorevler
            .Include(g => g.Kategori)
            .Where(g => g.KullaniciId == AktifKullaniciId && g.BitisTarihi.HasValue);

        if (start.HasValue)
            sorgu = sorgu.Where(g => g.BitisTarihi >= start.Value);
        if (end.HasValue)
            sorgu = sorgu.Where(g => g.BitisTarihi < end.Value);

        var gorevler = await sorgu.ToListAsync();

        var events = gorevler.Select(g =>
        {
            var oncelikRenk = OncelikRengiKodu(g.Oncelik);
            var anaRenk = string.IsNullOrWhiteSpace(g.Kategori?.Renk) ? oncelikRenk : g.Kategori.Renk;
            return new
            {
                id = g.Id,
                title = g.Baslik,
                start = g.BitisTarihi!.Value.ToString("yyyy-MM-dd"),
                allDay = true,
                // Görsel düzenleme JS tarafında (Linear stili soft background)
                // CSS variable'ları üzerinden inline style ile uygulanır.
                backgroundColor = "transparent",
                borderColor = "transparent",
                textColor = anaRenk,
                url = Url.Action("Detail", "Gorev", new { id = g.Id }),
                extendedProps = new
                {
                    durum = g.Durum.Etiket(),
                    durumKodu = g.Durum.ToString(),
                    kategoriAd = g.Kategori?.Ad,
                    kategoriRenk = g.Kategori?.Renk,
                    oncelik = g.Oncelik.Etiket(),
                    oncelikRenk = oncelikRenk,
                    anaRenk = anaRenk
                }
            };
        });

        return Json(events);
    }

    private static string OncelikRengiKodu(Oncelik oncelik) => oncelik switch
    {
        Oncelik.Acil => "#dc2626",
        Oncelik.Yuksek => "#f59e0b",
        Oncelik.Orta => "#64748b",
        Oncelik.Dusuk => "#0ea5e9",
        _ => "#6366f1"
    };

    private static string OncelikRengi(Gorev g)
    {
        // Kategori rengi varsa onu kullan, yoksa öncelik renkine düş
        if (!string.IsNullOrWhiteSpace(g.Kategori?.Renk))
            return g.Kategori.Renk;
        return g.Oncelik switch
        {
            Oncelik.Acil => "#dc2626",
            Oncelik.Yuksek => "#f59e0b",
            Oncelik.Orta => "#64748b",
            Oncelik.Dusuk => "#0ea5e9",
            _ => "#6366f1"
        };
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

    private Task<List<Gorev>> GorevleriGetir(GorevDurum? durum, int? kategoriId, bool kategorisiz = false, string? aramaSorgusu = null)
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

        if (!string.IsNullOrWhiteSpace(aramaSorgusu))
        {
            var ara = aramaSorgusu.Trim().ToLower();
            sorgu = sorgu.Where(g =>
                g.Baslik.ToLower().Contains(ara) ||
                (g.Aciklama != null && g.Aciklama.ToLower().Contains(ara)));
        }

        return sorgu
            .OrderByDescending(g => g.Oncelik)
            .ThenByDescending(g => g.OlusturmaTarihi)
            .ToListAsync();
    }

    private async Task KategoriListesiniDoldur()
    {
        ViewBag.Kategoriler = await _db.Kategoriler
            .Where(k => k.KullaniciId == AktifKullaniciId)
            .OrderBy(k => k.Sira).ThenBy(k => k.Ad)
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
            .OrderBy(k => k.Sira).ThenBy(k => k.Ad)
            .ToListAsync();

    private async Task<bool> KategoriGecerliMi(int? kategoriId)
    {
        if (!kategoriId.HasValue) return true;
        return await _db.Kategoriler
            .AnyAsync(k => k.Id == kategoriId.Value && k.KullaniciId == AktifKullaniciId);
    }
}
