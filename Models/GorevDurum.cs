using System.ComponentModel.DataAnnotations;

namespace GorevTakip.Models;

public enum GorevDurum
{
    [Display(Name = "Bekliyor")]
    Bekliyor = 1,

    [Display(Name = "Tamamlandı")]
    Tamamlandi = 2,

    [Display(Name = "İptal")]
    Iptal = 3
}
