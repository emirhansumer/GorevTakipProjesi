using System.Text;
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

    // Tüm kullanıcılar — görev/kategori sayılarıyla + arama
    public async Task<IActionResult> Kullanicilar(string? q)
    {
        var sorgu = _db.Kullanicilar.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var ara = q.Trim().ToLower();
            sorgu = sorgu.Where(k => k.AdSoyad.ToLower().Contains(ara) || k.Email.ToLower().Contains(ara));
        }

        var liste = await sorgu
            .OrderByDescending(k => k.Rol)
            .ThenBy(k => k.AdSoyad)
            .Select(k => new KullaniciOzet
            {
                Kullanici = k,
                GorevSayisi = k.Gorevler.Count,
                KategoriSayisi = k.Kategoriler.Count
            })
            .ToListAsync();

        ViewBag.Arama = q;
        return View(liste);
    }

    // Tek kullanıcı detay + yönetim ekranı
    public async Task<IActionResult> KullaniciDetay(int id)
    {
        var k = await _db.Kullanicilar
            .Include(u => u.Gorevler).ThenInclude(g => g.Kategori)
            .Include(u => u.Kategoriler)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (k is null)
        {
            TempData["Hata"] = "Kullanıcı bulunamadı.";
            return RedirectToAction(nameof(Kullanicilar));
        }

        var model = new AdminKullaniciDetayViewModel
        {
            Kullanici = k,
            GorevSayisi = k.Gorevler.Count,
            TamamlananGorev = k.Gorevler.Count(g => g.Durum == GorevDurum.Tamamlandi),
            KategoriSayisi = k.Kategoriler.Count,
            Gorevler = k.Gorevler.OrderByDescending(g => g.OlusturmaTarihi).ToList()
        };
        return View(model);
    }

    // Kullanıcı ad/email düzenle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KullaniciDuzenle(int id, string adSoyad, string email)
    {
        var k = await _db.Kullanicilar.FindAsync(id);
        if (k is null)
        {
            TempData["Hata"] = "Kullanıcı bulunamadı.";
            return RedirectToAction(nameof(Kullanicilar));
        }

        adSoyad = (adSoyad ?? string.Empty).Trim();
        email = (email ?? string.Empty).Trim().ToLowerInvariant();

        if (adSoyad.Length < 2 || string.IsNullOrWhiteSpace(email))
        {
            TempData["Hata"] = "Ad soyad en az 2 karakter ve e-posta dolu olmalı.";
            return RedirectToAction(nameof(KullaniciDetay), new { id });
        }

        if (await _db.Kullanicilar.AnyAsync(u => u.Email == email && u.Id != id))
        {
            TempData["Hata"] = "Bu e-posta başka bir kullanıcıda kayıtlı.";
            return RedirectToAction(nameof(KullaniciDetay), new { id });
        }

        k.AdSoyad = adSoyad;
        k.Email = email;
        await _db.SaveChangesAsync();

        TempData["Basari"] = "Kullanıcı bilgileri güncellendi.";
        return RedirectToAction(nameof(KullaniciDetay), new { id });
    }

    // Kullanıcının şifresini admin sıfırlar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SifreSifirla(int id, string yeniSifre)
    {
        var k = await _db.Kullanicilar.FindAsync(id);
        if (k is null)
        {
            TempData["Hata"] = "Kullanıcı bulunamadı.";
            return RedirectToAction(nameof(Kullanicilar));
        }

        if (string.IsNullOrWhiteSpace(yeniSifre) || yeniSifre.Length < 6)
        {
            TempData["Hata"] = "Yeni şifre en az 6 karakter olmalı.";
            return RedirectToAction(nameof(KullaniciDetay), new { id });
        }

        k.SifreHash = BCrypt.Net.BCrypt.HashPassword(yeniSifre);
        await _db.SaveChangesAsync();

        TempData["Basari"] = $"\"{k.AdSoyad}\" için yeni şifre belirlendi.";
        return RedirectToAction(nameof(KullaniciDetay), new { id });
    }

    // Aktif/Pasif (askıya alma) — kendi hesabına uygulanamaz
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AktiflikToggle(int id)
    {
        if (id == AktifKullaniciId)
        {
            TempData["Hata"] = "Kendi hesabını askıya alamazsın.";
            return RedirectToAction(nameof(Kullanicilar));
        }

        var k = await _db.Kullanicilar.FindAsync(id);
        if (k is null)
        {
            TempData["Hata"] = "Kullanıcı bulunamadı.";
            return RedirectToAction(nameof(Kullanicilar));
        }

        k.Aktif = !k.Aktif;
        await _db.SaveChangesAsync();

        TempData["Basari"] = k.Aktif ? $"\"{k.AdSoyad}\" aktifleştirildi." : $"\"{k.AdSoyad}\" askıya alındı.";
        return RedirectToAction(nameof(Kullanicilar));
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

    // Kullanıcının rolünü değiştir — kendi rolünü değiştiremezsin (kilitlenme önlemi)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RolDegistir(int id, KullaniciRol rol)
    {
        if (id == AktifKullaniciId)
        {
            TempData["Hata"] = "Kendi rolünü değiştiremezsin.";
            return RedirectToAction(nameof(Kullanicilar));
        }

        var kullanici = await _db.Kullanicilar.FindAsync(id);
        if (kullanici is null)
        {
            TempData["Hata"] = "Kullanıcı bulunamadı.";
            return RedirectToAction(nameof(Kullanicilar));
        }

        kullanici.Rol = rol;
        await _db.SaveChangesAsync();

        TempData["Basari"] = $"\"{kullanici.AdSoyad}\" kullanıcısının rolü artık: {rol.Etiket()}.";
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

    // --- İletişim mesajları (gelen kutusu) ---

    public async Task<IActionResult> Mesajlar()
    {
        var liste = await _db.IletisimMesajlari
            .OrderByDescending(m => m.OlusturmaTarihi)
            .ToListAsync();
        return View(liste);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MesajOkundu(int id)
    {
        var mesaj = await _db.IletisimMesajlari.FindAsync(id);
        if (mesaj is not null)
        {
            mesaj.Okundu = !mesaj.Okundu;
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Mesajlar));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MesajSil(int id)
    {
        var mesaj = await _db.IletisimMesajlari.FindAsync(id);
        if (mesaj is not null)
        {
            _db.IletisimMesajlari.Remove(mesaj);
            await _db.SaveChangesAsync();
            TempData["Basari"] = "Mesaj silindi.";
        }
        return RedirectToAction(nameof(Mesajlar));
    }

    // --- Site ayarları ---

    public async Task<IActionResult> Ayarlar()
    {
        var ayar = await _db.SiteAyarlari.FirstOrDefaultAsync() ?? new SiteAyar();
        return View(ayar);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ayarlar(SiteAyar model)
    {
        var ayar = await _db.SiteAyarlari.FirstOrDefaultAsync();
        if (ayar is null)
        {
            ayar = new SiteAyar();
            _db.SiteAyarlari.Add(ayar);
        }

        ayar.BakimModu = model.BakimModu;
        ayar.KayitAcik = model.KayitAcik;
        ayar.Duyuru = string.IsNullOrWhiteSpace(model.Duyuru) ? null : model.Duyuru.Trim();
        ayar.DuyuruAktif = model.DuyuruAktif;
        await _db.SaveChangesAsync();

        TempData["Basari"] = "Site ayarları kaydedildi.";
        return RedirectToAction(nameof(Ayarlar));
    }

    // --- CSV dışa aktarma ---

    public async Task<IActionResult> KullanicilarCsv()
    {
        var liste = await _db.Kullanicilar
            .Select(k => new { k.AdSoyad, k.Email, k.Rol, k.Aktif, k.KayitTarihi, GorevSayisi = k.Gorevler.Count })
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Ad Soyad;E-posta;Rol;Aktif;Kayit Tarihi;Gorev Sayisi");
        foreach (var k in liste)
            sb.AppendLine($"{CsvKacis(k.AdSoyad)};{CsvKacis(k.Email)};{CsvKacis(k.Rol.Etiket())};{(k.Aktif ? "Evet" : "Hayir")};{k.KayitTarihi:dd.MM.yyyy HH:mm};{k.GorevSayisi}");

        return CsvDosya(sb.ToString(), "kullanicilar.csv");
    }

    public async Task<IActionResult> GorevlerCsv()
    {
        var liste = await _db.Gorevler
            .Include(g => g.Kullanici)
            .Include(g => g.Kategori)
            .OrderByDescending(g => g.OlusturmaTarihi)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Baslik;Sahibi;Kategori;Oncelik;Durum;Bitis;Olusturma");
        foreach (var g in liste)
            sb.AppendLine($"{CsvKacis(g.Baslik)};{CsvKacis(g.Kullanici?.AdSoyad ?? "")};{CsvKacis(g.Kategori?.Ad ?? "")};{g.Oncelik.Etiket()};{g.Durum.Etiket()};{(g.BitisTarihi?.ToString("dd.MM.yyyy") ?? "")};{g.OlusturmaTarihi:dd.MM.yyyy HH:mm}");

        return CsvDosya(sb.ToString(), "gorevler.csv");
    }

    private static string CsvKacis(string s) =>
        s.Contains(';') || s.Contains('"') || s.Contains('\n')
            ? "\"" + s.Replace("\"", "\"\"") + "\""
            : s;

    private FileContentResult CsvDosya(string icerik, string ad)
    {
        // UTF-8 BOM — Excel Türkçe karakterleri doğru göstersin
        var bom = new byte[] { 0xEF, 0xBB, 0xBF };
        var govde = Encoding.UTF8.GetBytes(icerik);
        var bytes = bom.Concat(govde).ToArray();
        return File(bytes, "text/csv", ad);
    }
}
