namespace GenclikMerkezi.Modules.Website.Infrastructure.Sanitization;

// The single source of truth for HtmlSanitizerContentSanitizer's whitelist (ADR-024 §16). Extracted
// out of the sanitizer's constructor so a machine-readable contract (docs/api/html-whitelist.v1.json,
// audited by tests/ArchitectureTests/HtmlWhitelistContractTests.cs) can read the same values the
// sanitizer itself builds on, instead of a hand-maintained copy that could silently drift from it.
public static class HtmlWhitelistPolicy
{
    public static readonly IReadOnlyCollection<string> AllowedTags =
    [
        "p", "br", "strong", "b", "em", "i", "u", "s", "h2", "h3", "h4", "ul", "ol", "li", "blockquote",
        "a", "img", "figure", "figcaption", "table", "thead", "tbody", "tr", "th", "td", "hr", "iframe",
    ];

    public static readonly IReadOnlyCollection<string> AllowedAttributes =
        ["href", "title", "target", "src", "alt", "width", "height"];

    // Applies to <a href> only - img/iframe src are validated separately (see HtmlSanitizerContentSanitizer).
    public static readonly IReadOnlyCollection<string> AllowedHrefSchemes = ["http", "https", "mailto", "tel"];

    // Harmless wrappers that lose their own tag but keep their (separately sanitized) content instead
    // of being removed entirely - see HtmlSanitizerContentSanitizer.OnRemovingTag.
    public static readonly IReadOnlyCollection<string> UnwrappableTags =
        ["div", "span", "section", "article", "font", "h1", "h5", "h6"];

    public static readonly IReadOnlyCollection<string> RelValuesOnBlankTarget = ["noopener", "noreferrer"];

    public const string AllowedIframeHost = "www.youtube-nocookie.com";
    public const string AllowedIframeEmbedPathPrefix = "/embed/";

    // Tokens rejected in a normalized <img src>; see HtmlSanitizerContentSanitizer.ContainsPathTraversal
    // for why each one is here.
    public static readonly IReadOnlyCollection<string> ImageSourcePathTraversalTokens =
        ["..", "\\", "?", "#", "%2e", "%2f", "%5c"];
}
