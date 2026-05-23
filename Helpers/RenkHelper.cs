namespace GorevTakip.Helpers;

/// <summary>
/// Hex renk üzerinden parlaklık (luminance) hesaplaması.
/// Açık renklerde (sarı, limon-yeşili, beyaz vs.) beyaz text okunmaz → koyu text uygulanmalı.
/// </summary>
public static class RenkHelper
{
    /// <summary>
    /// Hex renk açık mı? (luminance > 0.62)
    /// </summary>
    public static bool ArkaPlanAcikMi(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex) || hex.Length != 7 || hex[0] != '#')
            return false;

        try
        {
            int r = Convert.ToInt32(hex.Substring(1, 2), 16);
            int g = Convert.ToInt32(hex.Substring(3, 2), 16);
            int b = Convert.ToInt32(hex.Substring(5, 2), 16);
            // ITU-R BT.601 luminance — JS tarafıyla aynı formül
            double lum = (0.299 * r + 0.587 * g + 0.114 * b) / 255.0;
            return lum > 0.62;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Razor'da: <span class="kategori-chip @RenkHelper.ChipKoyuClass(kat.Renk)">
    /// Açık renk → "chip-koyu-text", aksi → boş string.
    /// </summary>
    public static string ChipKoyuClass(string? hex) =>
        ArkaPlanAcikMi(hex) ? "chip-koyu-text" : "";
}
