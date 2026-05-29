using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GorevTakip.Models;

public class AltGorev
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Madde metni zorunludur.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Madde 1-200 karakter olmalıdır.")]
    public string Metin { get; set; } = string.Empty;

    public bool Tamamlandi { get; set; }

    // Listede sürükle-bırak sıralama için. Küçük değer önce gösterilir.
    public int Sira { get; set; }

    [ForeignKey(nameof(Gorev))]
    public int GorevId { get; set; }
    public Gorev? Gorev { get; set; }
}
