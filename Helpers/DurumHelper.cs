using GorevTakip.Models;

namespace GorevTakip.Helpers;

public static class DurumHelper
{
    public static string CssClass(this GorevDurum durum) => durum switch
    {
        GorevDurum.Bekliyor => "bg-warning text-dark",
        GorevDurum.Tamamlandi => "bg-success",
        GorevDurum.Iptal => "bg-secondary",
        _ => "bg-light text-dark"
    };

    public static string Etiket(this GorevDurum durum) => durum switch
    {
        GorevDurum.Bekliyor => "Bekliyor",
        GorevDurum.Tamamlandi => "Tamamlandı",
        GorevDurum.Iptal => "İptal",
        _ => durum.ToString()
    };
}
