using System.ComponentModel.DataAnnotations.Schema;

namespace GorevTakip.Models;

// Proje ↔ Kullanıcı üyeliği (çok-çok bağlantı tablosu).
public class ProjeUye
{
    public int Id { get; set; }

    [ForeignKey(nameof(Proje))]
    public int ProjeId { get; set; }
    public Proje? Proje { get; set; }

    [ForeignKey(nameof(Kullanici))]
    public int KullaniciId { get; set; }
    public Kullanici? Kullanici { get; set; }

    public DateTime KatilmaTarihi { get; set; } = DateTime.Now;
}
