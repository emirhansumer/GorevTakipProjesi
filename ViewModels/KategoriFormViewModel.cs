using System.ComponentModel.DataAnnotations;

namespace GorevTakip.ViewModels;

public class KategoriFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Kategori adı 2-50 karakter olmalıdır.")]
    [Display(Name = "Ad")]
    public string Ad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Renk seçimi zorunludur.")]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Geçerli bir renk seçiniz.")]
    [Display(Name = "Renk")]
    public string Renk { get; set; } = "#6366f1";

    [StringLength(50)]
    [RegularExpression(@"^bi-[a-z0-9-]+$", ErrorMessage = "Geçerli bir ikon seçiniz.")]
    [Display(Name = "İkon")]
    public string Ikon { get; set; } = "bi-bookmark";
}
