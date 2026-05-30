using System.ComponentModel.DataAnnotations;

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

    // Admin paneline erişim yetkisi. Sadece yönetici hesaplarında true.
    public bool IsAdmin { get; set; }

    public DateTime KayitTarihi { get; set; } = DateTime.Now;

    public ICollection<Gorev> Gorevler { get; set; } = new List<Gorev>();
    public ICollection<Kategori> Kategoriler { get; set; } = new List<Kategori>();
}
