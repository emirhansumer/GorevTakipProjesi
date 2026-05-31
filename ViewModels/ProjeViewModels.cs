using GorevTakip.Models;

namespace GorevTakip.ViewModels;

// "Projelerim" (liderlik) listesi için özet
public class ProjeOzetViewModel
{
    public Proje Proje { get; set; } = null!;
    public int UyeSayisi { get; set; }
    public int GorevSayisi { get; set; }
    public int TamamlananGorev { get; set; }
    public int Yuzde => GorevSayisi == 0 ? 0 : (int)System.Math.Round((double)TamamlananGorev * 100 / GorevSayisi);
}

// Proje detay/yönetim ekranı (lider)
public class ProjeDetayViewModel
{
    public Proje Proje { get; set; } = null!;
    public List<ProjeUye> Uyeler { get; set; } = new();
    public List<ProjeDavet> BekleyenDavetler { get; set; } = new();
    public List<Gorev> Gorevler { get; set; } = new();
    public List<Kategori> Kategoriler { get; set; } = new();
    public int GorevSayisi => Gorevler.Count;
    public int TamamlananGorev => Gorevler.Count(g => g.Durum == GorevDurum.Tamamlandi);
    public int Yuzde => GorevSayisi == 0 ? 0 : (int)System.Math.Round((double)TamamlananGorev * 100 / GorevSayisi);
}

// Üye tarafı: dahil olduğum bir proje + bana atanan görevler
public class UyeProjeViewModel
{
    public Proje Proje { get; set; } = null!;
    public List<Gorev> BanaAtananlar { get; set; } = new();
}

// "Takımlarım" ekranı: bekleyen davetler + üyesi olduğum projeler
public class TakimlarimViewModel
{
    public List<ProjeDavet> BekleyenDavetler { get; set; } = new();
    public List<UyeProjeViewModel> Projeler { get; set; } = new();
}
