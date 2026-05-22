using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GorevTakip.Models;

public class Gorev
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [StringLength(150, ErrorMessage = "Başlık en fazla 150 karakter olabilir.")]
    public string Baslik { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
    public string? Aciklama { get; set; }

    [Display(Name = "Oluşturma Tarihi")]
    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

    [Display(Name = "Bitiş Tarihi")]
    [DataType(DataType.Date)]
    public DateTime? BitisTarihi { get; set; }

    [Display(Name = "Durum")]
    public GorevDurum Durum { get; set; } = GorevDurum.Bekliyor;

    [Display(Name = "Öncelik")]
    public Oncelik Oncelik { get; set; } = Oncelik.Orta;

    [ForeignKey(nameof(Kullanici))]
    public int KullaniciId { get; set; }

    public Kullanici? Kullanici { get; set; }
}
