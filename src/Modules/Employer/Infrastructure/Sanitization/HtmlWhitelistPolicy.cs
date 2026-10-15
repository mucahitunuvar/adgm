namespace GenclikMerkezi.Modules.Employer.Infrastructure.Sanitization;

// The single source of truth for HtmlSanitizerContentSanitizer's whitelist (Employer public jobs
// master prompt, Görev 2). Extracted out of the sanitizer's constructor so a machine-readable
// contract (docs/api/html-whitelist.v1.json, audited by
// tests/ArchitectureTests/HtmlWhitelistContractTests.cs) can read the same values the sanitizer
// itself builds on, instead of a hand-maintained copy that could silently drift from it.
// Deliberately smaller than Website's own policy - no img/iframe (AboutHtml has no media-embedding
// feature), no table/heading levels beyond what a short "about us" paragraph needs.
public static class HtmlWhitelistPolicy
{
    public static readonly IReadOnlyCollection<string> AllowedTags =
        ["p", "br", "strong", "b", "em", "i", "u", "ul", "ol", "li", "blockquote", "a", "h2", "h3", "h4"];

    public static readonly IReadOnlyCollection<string> AllowedAttributes = ["href", "title", "target"];

    public static readonly IReadOnlyCollection<string> AllowedHrefSchemes = ["http", "https", "mailto", "tel"];

    public static readonly IReadOnlyCollection<string> RelValuesOnBlankTarget = ["noopener", "noreferrer"];
}
