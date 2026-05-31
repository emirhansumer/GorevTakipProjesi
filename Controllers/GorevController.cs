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
            .Include(g => g.AltGorevler)
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
                    anaRenk = anaRenk,
                    altToplam = g.AltGorevler.Count,
                    altTamam = g.AltGorevler.Count(a => a.Tamamlandi)
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
            .Include(g => g.AltGorevler.OrderBy(a => a.Sira).ThenBy(a => a.Id))
            .FirstOrDefaultAsync(g => g.Id == id && g.KullaniciId == AktifKullaniciId);

        if (gorev is null)
            return NotFound();

        return View(gorev);
    }

    private Task<List<Gorev>> GorevleriGetir(GorevDurum? durum, int? kategoriId, bool kategorisiz = false, string? aramaSorgusu = null)
    {
        var sorgu = _db.Gorevler
            .Include(g => g.Kategori)
            .Include(g => g.AltGorevler)
            .Include(g => g.Proje)
            .Include(g => g.Atayan)
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

    // --- alt görevler (checklist) — AJAX endpoint'leri ---

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AltGorevEkle([FromForm] int gorevId, [FromForm] string metin)
    {
        metin = (metin ?? string.Empty).Trim();
        if (metin.Length < 1 || metin.Length > 200)
            return BadRequest(new { ok = false, message = "Madde 1-200 karakter olmalıdır." });

        var gorev = await _db.Gorevler
            .FirstOrDefaultAsync(g => g.Id == gorevId && g.KullaniciId == AktifKullaniciId);
        if (gorev is null)
            return NotFound(new { ok = false, message = "Görev bulunamadı." });

        var sonrakiSira = (await _db.AltGorevler
            .Where(a => a.GorevId == gorevId)
            .Select(a => (int?)a.Sira)
            .MaxAsync()) ?? 0;

        var alt = new AltGorev
        {
            Metin = metin,
            Tamamlandi = false,
            Sira = sonrakiSira + 1,
            GorevId = gorevId
        };
        _db.AltGorevler.Add(alt);
        await _db.SaveChangesAsync();

        return Json(new
        {
            ok = true,
            id = alt.Id,
            metin = alt.Metin,
            tamamlandi = alt.Tamamlandi,
            ilerleme = await AltGorevIlerlemesi(gorevId)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AltGorevToggle([FromForm] int id)
    {
        var alt = await _db.AltGorevler
            .Include(a => a.Gorev)
            .FirstOrDefaultAsync(a => a.Id == id && a.Gorev!.KullaniciId == AktifKullaniciId);
        if (alt is null)
            return NotFound(new { ok = false, message = "Madde bulunamadı." });

        alt.Tamamlandi = !alt.Tamamlandi;
        await _db.SaveChangesAsync();

        return Json(new
        {
            ok = true,
            id = alt.Id,
            tamamlandi = alt.Tamamlandi,
            ilerleme = await AltGorevIlerlemesi(alt.GorevId)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AltGorevSil([FromForm] int id)
    {
        var alt = await _db.AltGorevler
            .Include(a => a.Gorev)
            .FirstOrDefaultAsync(a => a.Id == id && a.Gorev!.KullaniciId == AktifKullaniciId);
        if (alt is null)
            return NotFound(new { ok = false, message = "Madde bulunamadı." });

        var gorevId = alt.GorevId;
        _db.AltGorevler.Remove(alt);
        await _db.SaveChangesAsync();

        return Json(new
        {
            ok = true,
            id,
            ilerleme = await AltGorevIlerlemesi(gorevId)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AltGorevSirala([FromForm] int gorevId, [FromForm] int[] siraliIdler)
    {
        if (siraliIdler is null || siraliIdler.Length == 0)
            return BadRequest(new { ok = false, message = "Boş liste." });

        var gorev = await _db.Gorevler
            .FirstOrDefaultAsync(g => g.Id == gorevId && g.KullaniciId == AktifKullaniciId);
        if (gorev is null)
            return NotFound(new { ok = false, message = "Görev bulunamadı." });

        var altlar = await _db.AltGorevler
            .Where(a => a.GorevId == gorevId && siraliIdler.Contains(a.Id))
            .ToListAsync();

        // Güvenlik: gelen tüm id'lerin gerçekten bu göreve ait olduğunu doğrula
        if (altlar.Count != siraliIdler.Length)
            return BadRequest(new { ok = false, message = "Bazı maddeler size ait değil." });

        for (int i = 0; i < siraliIdler.Length; i++)
        {
            var a = altlar.First(x => x.Id == siraliIdler[i]);
            a.Sira = i + 1;
        }
        await _db.SaveChangesAsync();

        return Json(new { ok = true, count = siraliIdler.Length });
    }

    private async Task<object> AltGorevIlerlemesi(int gorevId)
    {
        var toplam = await _db.AltGorevler.CountAsync(a => a.GorevId == gorevId);
        var tamamlanan = await _db.AltGorevler.CountAsync(a => a.GorevId == gorevId && a.Tamamlandi);
        var yuzde = toplam == 0 ? 0 : (int)Math.Round((double)tamamlanan * 100 / toplam);
        return new { toplam, tamamlanan, yuzde };
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
