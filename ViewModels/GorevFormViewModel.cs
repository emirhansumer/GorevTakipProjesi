using System.ComponentModel.DataAnnotations;
using GorevTakip.Models;

namespace GorevTakip.ViewModels;

public class GorevFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Başlık 2-150 karakter olmalıdır.")]
    [Display(Name = "Başlık")]
    public string Baslik { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    [DataType(DataType.MultilineText)]
    public string? Aciklama { get; set; }

    [Display(Name = "Bitiş Tarihi")]
    [DataType(DataType.Date)]
    public DateTime? BitisTarihi { get; set; }

    [Display(Name = "Durum")]
    public GorevDurum Durum { get; set; } = GorevDurum.Bekliyor;

    [Display(Name = "Öncelik")]
    public Oncelik Oncelik { get; set; } = Oncelik.Orta;
}
