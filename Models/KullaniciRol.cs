namespace GorevTakip.Models;

// Kullanıcı yetki seviyeleri (artan): normal kullanıcı < proje lideri < sistem yöneticisi.
public enum KullaniciRol
{
    Kullanici = 0,
    ProjeLideri = 1,
    Yonetici = 2
}
