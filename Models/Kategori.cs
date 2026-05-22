using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GorevTakip.Models;

public class Kategori
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Kategori adı 2-50 karakter olmalıdır.")]
    [Display(Name = "Ad")]
    public string Ad { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Geçerli bir renk seçiniz.")]
    [StringLength(7)]
    [Display(Name = "Renk")]
    public string Renk { get; set; } = "#6366f1";

    // Bootstrap Icons sınıf adı (örn: "bi-bookmark"). Boş bırakılırsa default ikon kullanılır.
    [StringLength(50)]
    [Display(Name = "İkon")]
    public string Ikon { get; set; } = "bi-bookmark";

    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

    [ForeignKey(nameof(Kullanici))]
    public int KullaniciId { get; set; }
    public Kullanici? Kullanici { get; set; }

    public ICollection<Gorev> Gorevler { get; set; } = new List<Gorev>();
}
