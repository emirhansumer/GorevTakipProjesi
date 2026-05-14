namespace GorevTakip.ViewModels;

public class ProfileViewModel
{
    public int KullaniciId { get; set; }
    public string AdSoyad { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime KayitTarihi { get; set; }
    public int ToplamGorev { get; set; }

    public string SessionId { get; set; } = string.Empty;
    public bool SessionAktif { get; set; }
}
