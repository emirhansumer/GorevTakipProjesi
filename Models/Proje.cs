using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GorevTakip.Models;

// Bir proje/takım — bir lideri ve birden çok üyesi olur.
public class Proje
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Proje adı zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Proje adı 2-100 karakter olmalıdır.")]
    [Display(Name = "Proje Adı")]
    public string Ad { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    public string? Aciklama { get; set; }

    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

    [ForeignKey(nameof(Lider))]
    public int LiderId { get; set; }
    public Kullanici? Lider { get; set; }

    public ICollection<ProjeUye> Uyeler { get; set; } = new List<ProjeUye>();
    public ICollection<ProjeDavet> Davetler { get; set; } = new List<ProjeDavet>();
    public ICollection<Gorev> Gorevler { get; set; } = new List<Gorev>();
}
