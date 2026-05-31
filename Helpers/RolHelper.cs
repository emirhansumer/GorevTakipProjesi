using GorevTakip.Models;

namespace GorevTakip.Helpers;

public static class RolHelper
{
    public static string Etiket(this KullaniciRol rol) => rol switch
    {
        KullaniciRol.Yonetici => "Sistem Yöneticisi",
        KullaniciRol.ProjeLideri => "Proje Lideri",
        _ => "Kullanıcı"
    };

    public static string BadgeClass(this KullaniciRol rol) => rol switch
    {
        KullaniciRol.Yonetici => "bg-primary",
        KullaniciRol.ProjeLideri => "bg-info text-dark",
        _ => "bg-light text-dark border"
    };

    public static string Ikon(this KullaniciRol rol) => rol switch
    {
        KullaniciRol.Yonetici => "bi-shield-fill",
        KullaniciRol.ProjeLideri => "bi-person-badge",
        _ => "bi-person"
    };
}
