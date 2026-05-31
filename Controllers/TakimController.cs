using GorevTakip.Data;
using GorevTakip.Filters;
using GorevTakip.Helpers;
using GorevTakip.Models;
using GorevTakip.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GorevTakip.Controllers;

// Üye/davet tarafı — her giriş yapan kullanıcı erişebilir.
[AuthCheck]
public class TakimController : Controller
{
    private readonly AppDbContext _db;

    public TakimController(AppDbContext db)
    {
        _db = db;
    }

    private int AktifKullaniciId => HttpContext.Session.GetKullaniciId()!.Value;

    // Takımlarım: bekleyen davetler + üyesi olduğum projeler (bana atanan görevlerle)
    public async Task<IActionResult> Index()
    {
        var benId = AktifKullaniciId;

        var davetler = await _db.ProjeDavetleri
            .Include(d => d.Proje).ThenInclude(p => p!.Lider)
            .Where(d => d.KullaniciId == benId && d.Durum == DavetDurum.Bekliyor)
            .OrderByDescending(d => d.OlusturmaTarihi)
            .ToListAsync();

        var uyelikler = await _db.ProjeUyeleri
            .Include(u => u.Proje).ThenInclude(p => p!.Lider)
            .Where(u => u.KullaniciId == benId)
            .OrderBy(u => u.Proje!.Ad)
            .ToListAsync();

        var projeIdler = uyelikler.Select(u => u.ProjeId).ToList();
        var gorevler = await _db.Gorevler
            .Where(g => g.KullaniciId == benId && g.ProjeId != null && projeIdler.Contains(g.ProjeId.Value))
            .ToListAsync();

        var model = new TakimlarimViewModel
        {
            BekleyenDavetler = davetler,
            Projeler = uyelikler.Select(u => new UyeProjeViewModel
            {
                Proje = u.Proje!,
                BanaAtananlar = gorevler
                    .Where(g => g.ProjeId == u.ProjeId)
                    .OrderByDescending(g => g.OlusturmaTarihi)
                    .ToList()
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DavetKabul(int id)
    {
        var benId = AktifKullaniciId;
        var davet = await _db.ProjeDavetleri
            .FirstOrDefaultAsync(d => d.Id == id && d.KullaniciId == benId && d.Durum == DavetDurum.Bekliyor);

        if (davet is null)
        {
            TempData["Hata"] = "Davet bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var zatenUye = await _db.ProjeUyeleri.AnyAsync(u => u.ProjeId == davet.ProjeId && u.KullaniciId == benId);
        if (!zatenUye)
            _db.ProjeUyeleri.Add(new ProjeUye { ProjeId = davet.ProjeId, KullaniciId = benId });

        _db.ProjeDavetleri.Remove(davet);
        await _db.SaveChangesAsync();

        TempData["Basari"] = "Daveti kabul ettin, artık üyesin.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DavetReddet(int id)
    {
        var benId = AktifKullaniciId;
        var davet = await _db.ProjeDavetleri
            .FirstOrDefaultAsync(d => d.Id == id && d.KullaniciId == benId && d.Durum == DavetDurum.Bekliyor);

        if (davet is not null)
        {
            _db.ProjeDavetleri.Remove(davet);
            await _db.SaveChangesAsync();
            TempData["Basari"] = "Davet reddedildi.";
        }
        return RedirectToAction(nameof(Index));
    }
}
