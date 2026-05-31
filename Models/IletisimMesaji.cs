using System.ComponentModel.DataAnnotations;

namespace GorevTakip.Models;

// Ziyaretçi/kullanıcı tarafından gönderilen iletişim mesajı (yöneticiye ulaşır).
public class IletisimMesaji
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad soyad 2-100 karakter olmalıdır.")]
    [Display(Name = "Ad Soyad")]
    public string AdSoyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
    [StringLength(150)]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [StringLength(150, ErrorMessage = "Konu en fazla 150 karakter olabilir.")]
    [Display(Name = "Konu")]
    public string? Konu { get; set; }

    [Required(ErrorMessage = "Mesaj zorunludur.")]
    [StringLength(2000, MinimumLength = 5, ErrorMessage = "Mesaj 5-2000 karakter olmalıdır.")]
    [Display(Name = "Mesaj")]
    public string Mesaj { get; set; } = string.Empty;

    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

    public bool Okundu { get; set; }
}
