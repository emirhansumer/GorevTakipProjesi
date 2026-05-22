using System.ComponentModel.DataAnnotations;

namespace GorevTakip.Models;

public enum Oncelik
{
    [Display(Name = "Düşük")]
    Dusuk = 1,

    [Display(Name = "Orta")]
    Orta = 2,

    [Display(Name = "Yüksek")]
    Yuksek = 3,

    [Display(Name = "Acil")]
    Acil = 4
}
