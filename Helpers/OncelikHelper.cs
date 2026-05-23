using GorevTakip.Models;

namespace GorevTakip.Helpers;

public static class OncelikHelper
{
    public static string CssClass(this Oncelik oncelik) => oncelik switch
    {
        Oncelik.Dusuk => "oncelik-dusuk",
        Oncelik.Orta => "oncelik-orta",
        Oncelik.Yuksek => "oncelik-yuksek",
        Oncelik.Acil => "oncelik-acil",
        _ => "oncelik-orta"
    };

    public static string Etiket(this Oncelik oncelik) => oncelik switch
    {
        Oncelik.Dusuk => "Düşük",
        Oncelik.Orta => "Orta",
        Oncelik.Yuksek => "Yüksek",
        Oncelik.Acil => "Acil",
        _ => oncelik.ToString()
    };

    public static string Ikon(this Oncelik oncelik) => oncelik switch
    {
        Oncelik.Dusuk => "bi-arrow-down-circle-fill",
        Oncelik.Orta => "bi-dash-circle-fill",
        Oncelik.Yuksek => "bi-arrow-up-circle-fill",
        Oncelik.Acil => "bi-exclamation-octagon-fill",
        _ => "bi-dash-circle-fill"
    };
}
