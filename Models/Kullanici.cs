using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GorevTakip.Models;

public class Kullanici
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ad Soyad zorunludur.")]
    [StringLength(100)]
    public string AdSoyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string SifreHash { get; set; } = string.Empty;

    // Kullanıcının yetki seviyesi (kullanıcı / proje lideri / sistem yöneticisi).
    public KullaniciRol Rol { get; set; } = KullaniciRol.Kullanici;

    // Geriye dönük kolaylık: "yönetici mi?" — DB'ye yazılmaz, Rol'den türetilir.
    [NotMapped]
    public bool IsAdmin => Rol == KullaniciRol.Yonetici;

    // Pasif kullanıcı giriş yapamaz (yönetici tarafından askıya alınabilir).
    public bool Aktif { get; set; } = true;

    public DateTime KayitTarihi { get; set; } = DateTime.Now;

    public ICollection<Gorev> Gorevler { get; set; } = new List<Gorev>();
    public ICollection<Kategori> Kategoriler { get; set; } = new List<Kategori>();

    // Liderlik ettiği projeler + üyesi olduğu projeler (ProjeUye üzerinden)
    public ICollection<Proje> LiderOlduguProjeler { get; set; } = new List<Proje>();
    public ICollection<ProjeUye> ProjeUyelikleri { get; set; } = new List<ProjeUye>();
}
