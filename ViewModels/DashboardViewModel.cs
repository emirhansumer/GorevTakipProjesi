using GorevTakip.Models;

namespace GorevTakip.ViewModels;

public class DashboardViewModel
{
    public string AdSoyad { get; set; } = string.Empty;
    public int ToplamGorev { get; set; }
    public int BekleyenGorev { get; set; }
    public int TamamlananGorev { get; set; }
    public int IptalGorev { get; set; }
    public List<Gorev> SonGorevler { get; set; } = new();
}
