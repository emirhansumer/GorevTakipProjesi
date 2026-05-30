using GorevTakip.Models;

namespace GorevTakip.ViewModels;

// Admin dashboard — sistem genelinde istatistikler
public class AdminDashboardViewModel
{
    public int ToplamKullanici { get; set; }
    public int ToplamGorev { get; set; }
    public int ToplamKategori { get; set; }
    public int ToplamAltGorev { get; set; }

    public int BekleyenGorev { get; set; }
    public int TamamlananGorev { get; set; }
    public int IptalGorev { get; set; }

    public List<KullaniciOzet> SonKullanicilar { get; set; } = new();
    public List<KullaniciOzet> EnAktifKullanicilar { get; set; } = new();
}

// Kullanıcı + sahip olduğu görev/kategori sayıları (admin listeleri için)
public class KullaniciOzet
{
    public Kullanici Kullanici { get; set; } = null!;
    public int GorevSayisi { get; set; }
    public int KategoriSayisi { get; set; }
}

// Tüm görevler ekranı — filtre durumu + sonuçlar
public class AdminGorevlerViewModel
{
    public List<Gorev> Gorevler { get; set; } = new();
    public List<Kullanici> Kullanicilar { get; set; } = new();
    public GorevDurum? SeciliDurum { get; set; }
    public int? SeciliKullaniciId { get; set; }
}
