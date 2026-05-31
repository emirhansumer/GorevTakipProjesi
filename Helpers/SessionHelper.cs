using GorevTakip.Models;

namespace GorevTakip.Helpers;

public static class SessionKeys
{
    public const string KullaniciId = "KullaniciId";
    public const string AdSoyad = "AdSoyad";
    public const string Email = "Email";
    public const string Rol = "Rol";
}

public static class SessionHelper
{
    public static void GirisYap(this ISession session, int kullaniciId, string adSoyad, string email, KullaniciRol rol = KullaniciRol.Kullanici)
    {
        session.SetInt32(SessionKeys.KullaniciId, kullaniciId);
        session.SetString(SessionKeys.AdSoyad, adSoyad);
        session.SetString(SessionKeys.Email, email);
        session.SetInt32(SessionKeys.Rol, (int)rol);
    }

    public static void CikisYap(this ISession session)
    {
        session.Clear();
    }

    public static int? GetKullaniciId(this ISession session) =>
        session.GetInt32(SessionKeys.KullaniciId);

    public static string? GetAdSoyad(this ISession session) =>
        session.GetString(SessionKeys.AdSoyad);

    public static string? GetEmail(this ISession session) =>
        session.GetString(SessionKeys.Email);

    public static bool GirisYapildiMi(this ISession session) =>
        session.GetInt32(SessionKeys.KullaniciId).HasValue;

    public static KullaniciRol GetRol(this ISession session) =>
        (KullaniciRol)(session.GetInt32(SessionKeys.Rol) ?? 0);

    // Sistem yöneticisi mi?
    public static bool AdminMi(this ISession session) =>
        session.GetRol() == KullaniciRol.Yonetici;

    // En az proje lideri (proje lideri veya yönetici)?
    public static bool ProjeLideriVeUstu(this ISession session) =>
        session.GetRol() >= KullaniciRol.ProjeLideri;
}
