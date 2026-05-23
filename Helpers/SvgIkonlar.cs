using System.Net;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GorevTakip.Helpers;

/// <summary>
/// Bootstrap Icons'ta olmayan özel ikonlar (futbol, hakem, vb.)
/// Monokromatik, currentColor kullanır — Bootstrap Icons ile aynı görsel dil.
/// View'da: @Html.Raw(SvgIkonlar.Render("ozel-futbol-topu"))
/// Veya tag helper: <span class="bi-svg">@Html.Raw(SvgIkonlar.Render("ozel-futbol-topu"))</span>
/// </summary>
public static class SvgIkonlar
{
    /// <summary>
    /// "ozel-" prefix'iyle başlayan ikonu döner. Yoksa boş string.
    /// </summary>
    public static HtmlString Render(string ad)
    {
        if (!Map.TryGetValue(ad, out var svg))
            return HtmlString.Empty;
        return new HtmlString(svg);
    }

    public static bool VarMi(string ad) => Map.ContainsKey(ad);

    /// <summary>
    /// Tek satır ikon render — Bootstrap Icons "bi-*" veya özel SVG "ozel-*" otomatik ayırt edilir.
    /// View'da: @Html.KategoriIkon(kategori.Ikon)
    /// </summary>
    public static IHtmlContent KategoriIkon(this IHtmlHelper html, string? ikonAdi)
    {
        var ad = string.IsNullOrWhiteSpace(ikonAdi) ? "bi-bookmark" : ikonAdi.Trim();
        if (ad.StartsWith("ozel-") && Map.TryGetValue(ad, out var svg))
            return new HtmlString(svg);
        // Bootstrap Icons class — XSS koruması için class adını HTML encode et
        return new HtmlString($"<i class=\"bi {WebUtility.HtmlEncode(ad)}\"></i>");
    }

    private const string SvgAttr =
        @"xmlns=""http://www.w3.org/2000/svg"" width=""1.25em"" height=""1.25em"" viewBox=""0 0 24 24"" " +
        @"fill=""none"" stroke=""currentColor"" stroke-width=""2.5"" stroke-linecap=""round"" stroke-linejoin=""round"" " +
        @"style=""display:inline-block;vertical-align:-0.18em""";

    // Tabler Icons (MIT) inspired — tek path/group monokromatik tasarım
    private static readonly Dictionary<string, string> Map = new()
    {
        ["ozel-futbol-topu"] =
            $@"<svg {SvgAttr}>
                <circle cx=""12"" cy=""12"" r=""9""/>
                <path d=""M12 7l4.755 3.455l-1.755 5.545h-6l-1.755 -5.545z""/>
                <path d=""M12 7v-4""/>
                <path d=""M16.755 10.455l3.245 -2.455""/>
                <path d=""M15 16l2 3""/>
                <path d=""M9 16l-2 3""/>
                <path d=""M7.245 10.455l-3.245 -2.455""/>
              </svg>"
    };
}
