using GorevTakip.Data;
using GorevTakip.Filters;
using GorevTakip.Helpers;
using GorevTakip.Models;
using GorevTakip.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GorevTakip.Controllers;

// Proje Lideri (ve üstü) tarafı: kendi projelerini yönetir, üye davet eder, görev atar.
[ProjeLideriCheck]
public class ProjeController : Controller
{
    private readonly AppDbContext _db;

    public ProjeController(AppDbContext db)
    {
        _db = db;
    }

    private int AktifKullaniciId => HttpContext.Session.GetKullaniciId()!.Value;

    // Liderlik ettiğim projeler
    public async Task<IActionResult> Index()
    {
        var projeler = await _db.Projeler
            .Where(p => p.LiderId == AktifKullaniciId)
            .OrderByDescending(p => p.OlusturmaTarihi)
            .Select(p => new ProjeOzetViewModel
            {
                Proje = p,
                UyeSayisi = p.Uyeler.Count,
                GorevSayisi = p.Gorevler.Count,
                TamamlananGorev = p.Gorevler.Count(g => g.Durum == GorevDurum.Tamamlandi)
            })
            .ToListAsync();

        return View(projeler);
    }

    [HttpGet]
    public IActionResult Olustur() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Olustur(Proje model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var proje = new Proje
        {
            Ad = model.Ad.Trim(),
            Aciklama = string.IsNullOrWhiteSpace(model.Aciklama) ? null : model.Aciklama.Trim(),
            LiderId = AktifKullaniciId,
            OlusturmaTarihi = DateTime.Now
        };
        _db.Projeler.Add(proje);
        await _db.SaveChangesAsync();

        TempData["Basari"] = "Proje oluşturuldu. Şimdi üye davet edebilirsin.";
        return RedirectToAction(nameof(Detay), new { id = proje.Id });
    }

    // Proje detay/yönetim — sadece projenin lideri erişebilir
    public async Task<IActionResult> Detay(int id)
    {
        var proje = await _db.Projeler.FirstOrDefaultAsync(p => p.Id == id && p.LiderId == AktifKullaniciId);
        if (proje is null)
            return RedirectToAction(nameof(Index));

        var model = new ProjeDetayViewModel
        {
            Proje = proje,
            Uyeler = await _db.ProjeUyeleri
                .Include(u => u.Kullanici)
                .Where(u => u.ProjeId == id)
                .OrderBy(u => u.Kullanici!.AdSoyad)
                .ToListAsync(),
            BekleyenDavetler = await _db.ProjeDavetleri
                .Include(d => d.Kullanici)
                .Where(d => d.ProjeId == id && d.Durum == DavetDurum.Bekliyor)
                .ToListAsync(),
            Gorevler = await _db.Gorevler
                .Include(g => g.Kullanici)
                .Where(g => g.ProjeId == id)
                .OrderByDescending(g => g.OlusturmaTarihi)
                .ToListAsync(),
            Kategoriler = await _db.Kategoriler
                .Where(k => k.KullaniciId == AktifKullaniciId)
                .OrderBy(k => k.Sira)
                .ToListAsync()
        };
        return View(model);
    }

    // Projeye sahiplik kontrolü
    private Task<Proje?> ProjemiGetir(int projeId) =>
        _db.Projeler.FirstOrDefaultAsync(p => p.Id == projeId && p.LiderId == AktifKullaniciId);

    // E-posta ile üye davet et (lider tüm kullanıcı listesini görmez)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UyeDavet(int projeId, string email)
    {
        var proje = await ProjemiGetir(projeId);
        if (proje is null) return RedirectToAction(nameof(Index));

        email = (email ?? string.Empty).Trim().ToLowerInvariant();
        var hedef = await _db.Kullanicilar.FirstOrDefaultAsync(k => k.Email == email);

        if (hedef is null || !hedef.Aktif)
            TempData["Hata"] = "Bu e-postayla aktif bir kullanıcı bulunamadı.";
        else if (hedef.Id == AktifKullaniciId)
            TempData["Hata"] = "Kendini davet edemezsin.";
        else if (await _db.ProjeUyeleri.AnyAsync(u => u.ProjeId == projeId && u.KullaniciId == hedef.Id))
            TempData["Hata"] = $"\"{hedef.AdSoyad}\" zaten bu projenin üyesi.";
        else if (await _db.ProjeDavetleri.AnyAsync(d => d.ProjeId == projeId && d.KullaniciId == hedef.Id && d.Durum == DavetDurum.Bekliyor))
            TempData["Hata"] = $"\"{hedef.AdSoyad}\" için zaten bekleyen bir davet var.";
        else
        {
            _db.ProjeDavetleri.Add(new ProjeDavet { ProjeId = projeId, KullaniciId = hedef.Id, Durum = DavetDurum.Bekliyor });
            await _db.SaveChangesAsync();
            TempData["Basari"] = $"\"{hedef.AdSoyad}\" davet edildi. Kabul edince üye olacak.";
        }

        return RedirectToAction(nameof(Detay), new { id = projeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DavetIptal(int davetId)
    {
        var davet = await _db.ProjeDavetleri.Include(d => d.Proje).FirstOrDefaultAsync(d => d.Id == davetId);
        if (davet is null || davet.Proje!.LiderId != AktifKullaniciId)
            return RedirectToAction(nameof(Index));

        var projeId = davet.ProjeId;
        _db.ProjeDavetleri.Remove(davet);
        await _db.SaveChangesAsync();
        TempData["Basari"] = "Davet iptal edildi.";
        return RedirectToAction(nameof(Detay), new { id = projeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UyeCikar(int projeId, int kullaniciId)
    {
        var proje = await ProjemiGetir(projeId);
        if (proje is null) return RedirectToAction(nameof(Index));

        var uye = await _db.ProjeUyeleri.FirstOrDefaultAsync(u => u.ProjeId == projeId && u.KullaniciId == kullaniciId);
        if (uye is not null)
        {
            _db.ProjeUyeleri.Remove(uye);
            await _db.SaveChangesAsync();
            TempData["Basari"] = "Üye projeden çıkarıldı.";
        }
        return RedirectToAction(nameof(Detay), new { id = projeId });
    }

    // Seçili üye(ler)e görev ata — her üyeye kendi kopyası oluşur
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GorevAta(int projeId, string baslik, string? aciklama, Oncelik oncelik, DateTime? bitisTarihi, int? kategoriId, int[] uyeIdler)
    {
        var proje = await ProjemiGetir(projeId);
        if (proje is null) return RedirectToAction(nameof(Index));

        baslik = (baslik ?? string.Empty).Trim();
        if (baslik.Length < 1)
        {
            TempData["Hata"] = "Görev başlığı zorunludur.";
            return RedirectToAction(nameof(Detay), new { id = projeId });
        }

        // Sadece bu projenin gerçek üyelerine atanabilir
        var gecerliUyeler = await _db.ProjeUyeleri
            .Where(u => u.ProjeId == projeId && uyeIdler.Contains(u.KullaniciId))
            .Select(u => u.KullaniciId)
            .ToListAsync();

        if (gecerliUyeler.Count == 0)
        {
            TempData["Hata"] = "En az bir üye seçmelisin.";
            return RedirectToAction(nameof(Detay), new { id = projeId });
        }

        // Kategori seçildiyse lidere ait olmalı (başkasının kategorisi atanamaz)
        if (kategoriId.HasValue && !await _db.Kategoriler.AnyAsync(k => k.Id == kategoriId.Value && k.KullaniciId == AktifKullaniciId))
            kategoriId = null;

        var simdi = DateTime.Now;
        foreach (var uyeId in gecerliUyeler)
        {
            _db.Gorevler.Add(new Gorev
            {
                Baslik = baslik,
                Aciklama = string.IsNullOrWhiteSpace(aciklama) ? null : aciklama.Trim(),
                Oncelik = oncelik,
                BitisTarihi = bitisTarihi,
                KategoriId = kategoriId,
                Durum = GorevDurum.Bekliyor,
                OlusturmaTarihi = simdi,
                KullaniciId = uyeId,
                AtayanId = AktifKullaniciId,
                ProjeId = projeId
            });
        }
        await _db.SaveChangesAsync();

        TempData["Basari"] = $"Görev {gecerliUyeler.Count} üyeye atandı.";
        return RedirectToAction(nameof(Detay), new { id = projeId });
    }

    // Atanan görevi düzenle (yalnızca bu projenin lideri)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AtamaDuzenle(int gorevId, string baslik, string? aciklama, Oncelik oncelik, DateTime? bitisTarihi, int? kategoriId)
    {
        var gorev = await _db.Gorevler.Include(g => g.Proje).FirstOrDefaultAsync(g => g.Id == gorevId);
        if (gorev?.Proje is null || gorev.Proje.LiderId != AktifKullaniciId)
            return RedirectToAction(nameof(Index));

        baslik = (baslik ?? string.Empty).Trim();
        if (baslik.Length >= 1)
        {
            if (kategoriId.HasValue && !await _db.Kategoriler.AnyAsync(k => k.Id == kategoriId.Value && k.KullaniciId == AktifKullaniciId))
                kategoriId = null;

            gorev.Baslik = baslik;
            gorev.Aciklama = string.IsNullOrWhiteSpace(aciklama) ? null : aciklama.Trim();
            gorev.Oncelik = oncelik;
            gorev.BitisTarihi = bitisTarihi;
            gorev.KategoriId = kategoriId;
            await _db.SaveChangesAsync();
            TempData["Basari"] = "Atanan görev güncellendi.";
        }
        return RedirectToAction(nameof(Detay), new { id = gorev.ProjeId });
    }

    // Atamayı geri al (görevi sil)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AtamaIptal(int gorevId)
    {
        var gorev = await _db.Gorevler.Include(g => g.Proje).FirstOrDefaultAsync(g => g.Id == gorevId);
        if (gorev?.Proje is null || gorev.Proje.LiderId != AktifKullaniciId)
            return RedirectToAction(nameof(Index));

        var projeId = gorev.ProjeId;
        _db.Gorevler.Remove(gorev);
        await _db.SaveChangesAsync();
        TempData["Basari"] = "Atama geri alındı (görev silindi).";
        return RedirectToAction(nameof(Detay), new { id = projeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProjeSil(int id)
    {
        var proje = await ProjemiGetir(id);
        if (proje is null) return RedirectToAction(nameof(Index));

        _db.Projeler.Remove(proje);
        await _db.SaveChangesAsync();
        TempData["Basari"] = "Proje silindi.";
        return RedirectToAction(nameof(Index));
    }
}
