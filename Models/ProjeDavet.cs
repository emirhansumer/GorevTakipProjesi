using System.ComponentModel.DataAnnotations.Schema;

namespace GorevTakip.Models;

public enum DavetDurum
{
    Bekliyor = 0,
    Kabul = 1,
    Red = 2
}

// Bir kullanıcının bir projeye davet edilmesi (kabul/red bekler).
public class ProjeDavet
{
    public int Id { get; set; }

    [ForeignKey(nameof(Proje))]
    public int ProjeId { get; set; }
    public Proje? Proje { get; set; }

    // Davet edilen kullanıcı
    [ForeignKey(nameof(Kullanici))]
    public int KullaniciId { get; set; }
    public Kullanici? Kullanici { get; set; }

    public DavetDurum Durum { get; set; } = DavetDurum.Bekliyor;
    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
}
