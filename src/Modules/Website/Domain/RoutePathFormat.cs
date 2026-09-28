using System.Text.RegularExpressions;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §15 (Faz 1a Görev 6): pure path-string math for public route resolution - no repository
// access, same Domain/Application split ContentPathService (Görev 4) already established for
// cross-aggregate path computation. The internal representation used here matches every other path
// value in this module (Redirect.FromPath, ContentItemTranslation.FullPath): lowercase, no leading or
// trailing slash, single slashes between segments. Only BuildPublicPath produces the leading-slash
// form a browser/HTTP Location header needs.
public static partial class RoutePathFormat
{
    private static readonly Regex MultipleSlashesPattern = MultipleSlashesRegex();

    // Lowercases, collapses consecutive slashes to one, and trims leading/trailing slashes - the
    // internal ("haberler/yeni-proje") form, not the public ("/haberler/yeni-proje") one.
    public static string Normalize(string? rawPath)
    {
        var lower = (rawPath ?? string.Empty).Trim().ToLowerInvariant();
        var collapsed = MultipleSlashesPattern.Replace(lower, "/");
        return collapsed.Trim('/');
    }

    // The first path segment and everything after it (also internal form, no leading/trailing
    // slashes). Both are empty for an already-empty path; Remainder is empty when there is only one
    // segment.
    public static (string FirstSegment, string Remainder) SplitFirstSegment(string normalizedPath)
    {
        if (normalizedPath.Length == 0)
        {
            return (string.Empty, string.Empty);
        }

        var slashIndex = normalizedPath.IndexOf('/');
        return slashIndex < 0
            ? (normalizedPath, string.Empty)
            : (normalizedPath[..slashIndex], normalizedPath[(slashIndex + 1)..]);
    }

    // The canonical public path for languageCode: "/" alone for home, otherwise "/" + (a language
    // segment, only when languageCode is not the default language) + remainderPath.
    public static string BuildPublicPath(string languageCode, string defaultLanguageCode, string remainderPath)
    {
        var languageSegment = string.Equals(languageCode, defaultLanguageCode, StringComparison.Ordinal) ? null : languageCode;
        var segments = new List<string>(2);
        if (languageSegment is { Length: > 0 })
        {
            segments.Add(languageSegment);
        }

        if (remainderPath.Length > 0)
        {
            segments.Add(remainderPath);
        }

        return segments.Count == 0 ? "/" : "/" + string.Join('/', segments);
    }

    [GeneratedRegex("/{2,}")]
    private static partial Regex MultipleSlashesRegex();
}
