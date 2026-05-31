using System.ComponentModel.DataAnnotations;

namespace GorevTakip.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ad Soyad zorunludur.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Ad Soyad 3-100 karakter olmalıdır.")]
    [Display(Name = "Ad Soyad")]
    public string AdSoyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
    [StringLength(150)]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Sifre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Sifre), ErrorMessage = "Şifreler eşleşmiyor.")]
    [Display(Name = "Şifre Tekrarı")]
    public string SifreTekrar { get; set; } = string.Empty;

    // KVKK aydınlatma metni onayı — kayıt için zorunlu (true olmalı)
    [Range(typeof(bool), "true", "true", ErrorMessage = "Kayıt için KVKK Aydınlatma Metni'ni onaylamalısın.")]
    [Display(Name = "KVKK Onayı")]
    public bool KvkkOnay { get; set; }
}
