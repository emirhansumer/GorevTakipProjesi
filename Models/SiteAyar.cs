using System.ComponentModel.DataAnnotations;

namespace GorevTakip.Models;

// Site geneli ayarlar — tek satırlık (singleton, Id = 1).
public class SiteAyar
{
    public int Id { get; set; }

    // Açıkken site sadece adminlere açık; diğerleri "bakımda" sayfası görür.
    [Display(Name = "Bakım Modu")]
    public bool BakimModu { get; set; }

    // Kapalıyken yeni kullanıcı kaydı engellenir.
    [Display(Name = "Kayıtlara İzin Ver")]
    public bool KayitAcik { get; set; } = true;

    // Tüm kullanıcılara üstte gösterilecek duyuru metni.
    [StringLength(300)]
    [Display(Name = "Duyuru Metni")]
    public string? Duyuru { get; set; }

    [Display(Name = "Duyuruyu Göster")]
    public bool DuyuruAktif { get; set; }
}
