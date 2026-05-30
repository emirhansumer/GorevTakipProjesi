namespace GorevTakip.Helpers;

public static class SessionKeys
{
    public const string KullaniciId = "KullaniciId";
    public const string AdSoyad = "AdSoyad";
    public const string Email = "Email";
    public const string IsAdmin = "IsAdmin";
}

public static class SessionHelper
{
    public static void GirisYap(this ISession session, int kullaniciId, string adSoyad, string email, bool isAdmin = false)
    {
        session.SetInt32(SessionKeys.KullaniciId, kullaniciId);
        session.SetString(SessionKeys.AdSoyad, adSoyad);
        session.SetString(SessionKeys.Email, email);
        session.SetInt32(SessionKeys.IsAdmin, isAdmin ? 1 : 0);
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

    public static bool AdminMi(this ISession session) =>
        session.GetInt32(SessionKeys.IsAdmin) == 1;
}
