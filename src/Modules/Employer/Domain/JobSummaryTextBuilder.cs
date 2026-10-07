using System.Net;
using System.Text.RegularExpressions;

namespace GenclikMerkezi.Modules.Employer.Domain;

// Görev 3 (Employer public jobs master prompt): HTML'den düz metin üretimi tek yerde - Görev 4'ün
// IPublishedJobModuleContract özeti (500 karakter) de bu metodu kullanır. Saf domain mantığı
// (JobSlugGenerator ile aynı gerekçe: infra bağımlılığı yok).
public static partial class JobSummaryTextBuilder
{
    public static string Build(string? html, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var withoutTags = TagPattern().Replace(html, " ");
        var decoded = WebUtility.HtmlDecode(withoutTags);
        var collapsed = WhitespacePattern().Replace(decoded, " ").Trim();

        return Truncate(collapsed, maxLength);
    }

    // Kelime sınırında kırpar - kelime ortasında kesilen JobSlugGenerator'dan farklı olarak master
    // prompt burada "kelime sınırında" şartını açıkça koyuyor (Görev 3 Testler).
    private static string Truncate(string text, int maxLength)
    {
        if (text.Length <= maxLength)
        {
            return text;
        }

        var cut = text[..maxLength];
        var lastSpaceIndex = cut.LastIndexOf(' ');

        return lastSpaceIndex > 0 ? cut[..lastSpaceIndex] : cut;
    }

    [GeneratedRegex("<[^>]*>")]
    private static partial Regex TagPattern();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespacePattern();
}
