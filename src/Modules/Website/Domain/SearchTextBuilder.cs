using System.Net;
using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Text;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §10 (Faz 5 Görev 1): the single place that turns HTML into search-oriented text. Görev 2
// (Website content indexing) and Görev 4 (external source adapters, via ExternalSearchDocument)
// both route through this instead of duplicating the tag/entity-stripping regex - same approach as
// Employer's JobSummaryTextBuilder (ADR-023 §6), independently reimplemented (Website must not
// depend on Employer).
public static partial class SearchTextBuilder
{
    public static string StripHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var withoutTags = TagPattern().Replace(html, " ");
        var decoded = WebUtility.HtmlDecode(withoutTags);

        return WhitespacePattern().Replace(decoded, " ").Trim();
    }

    // Word-boundary truncation (never cuts mid-word) - this is the SearchDocument.Summary value.
    public static string BuildSummary(string? html, int maxLength)
    {
        return Truncate(StripHtml(html), maxLength);
    }

    // SearchDocument.NormalizedText = title + summary + a short body excerpt, all folded through
    // TurkishTextNormalizer so Görev 3's token-AND matching is diacritic/case-insensitive, then capped
    // at maxLength with a hard cut (this field is never displayed, only matched against).
    public static string BuildNormalizedText(string title, string summary, string? bodyHtml, int maxLength)
    {
        var bodyExcerpt = StripHtml(bodyHtml);
        var parts = new[] { title, summary, bodyExcerpt }.Where(part => !string.IsNullOrWhiteSpace(part));
        var combined = string.Join(' ', parts);
        var normalized = TurkishTextNormalizer.Normalize(combined);

        return normalized.Length > maxLength ? normalized[..maxLength] : normalized;
    }

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
