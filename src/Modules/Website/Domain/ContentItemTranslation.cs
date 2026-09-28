using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4.2/§4.3 (Faz 1a Görev 3): per-language title, slug, computed full path, summary,
// sanitized body and SEO. Body arrives already sanitized - HtmlSanitizerContentSanitizer runs in the
// Application-layer command handler (ADR-024 Faz 1a §1: Domain must not depend on the sanitizer), not
// here.
//
// FullPath's Görev 3 rule (temporary until Görev 4 generalizes it to full hierarchy): "{routePrefix}/
// {slug}", or just "{slug}" when routePrefix is empty. routePrefix itself is never stored here - it
// belongs to ContentType and can change independently, so it is supplied by the caller (ContentItem)
// every time FullPath needs recomputing, never cached across calls.
public sealed class ContentItemTranslation : Entity
{
    public const int MaxTitleLength = 200;
    public const int MaxSummaryLength = 500;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string Title { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string FullPath { get; private set; } = string.Empty;

    public string Summary { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public SeoMetadata Seo { get; private set; } = SeoMetadata.CreateEmpty();

    private ContentItemTranslation(
        Guid id, LanguageCode languageCode, string title, string slug, string fullPath, string summary, string body, SeoMetadata seo)
        : base(id)
    {
        LanguageCode = languageCode;
        Title = title;
        Slug = slug;
        FullPath = fullPath;
        Summary = summary;
        Body = body;
        Seo = seo;
    }

    private ContentItemTranslation()
    {
    }

    public static Result<ContentItemTranslation> Create(
        LanguageCode languageCode, string? title, string? slug, string routePrefix,
        IReadOnlyList<string> ancestorSlugsRootToParent, string? summary, string? body, SeoMetadata seo)
    {
        var titleResult = NormalizeTitle(title);
        if (titleResult.IsFailure)
        {
            return Result.Failure<ContentItemTranslation>(titleResult.Error);
        }

        var slugResult = Domain.Slug.Create(string.IsNullOrWhiteSpace(slug) ? title : slug);
        if (slugResult.IsFailure)
        {
            return Result.Failure<ContentItemTranslation>(slugResult.Error);
        }

        var summaryResult = NormalizeSummary(summary);
        if (summaryResult.IsFailure)
        {
            return Result.Failure<ContentItemTranslation>(summaryResult.Error);
        }

        var fullPath = ContentPathService.ComputeFullPath(routePrefix, ancestorSlugsRootToParent, slugResult.Value.Value);

        return Result.Success(new ContentItemTranslation(
            Guid.NewGuid(), languageCode, titleResult.Value, slugResult.Value.Value, fullPath, summaryResult.Value,
            (body ?? string.Empty).Trim(), seo));
    }

    internal Result Update(
        string? title, string? slug, string routePrefix, IReadOnlyList<string> ancestorSlugsRootToParent,
        string? summary, string? body, SeoMetadata seo)
    {
        var titleResult = NormalizeTitle(title);
        if (titleResult.IsFailure)
        {
            return titleResult;
        }

        var slugResult = Domain.Slug.Create(string.IsNullOrWhiteSpace(slug) ? title : slug);
        if (slugResult.IsFailure)
        {
            return slugResult;
        }

        var summaryResult = NormalizeSummary(summary);
        if (summaryResult.IsFailure)
        {
            return summaryResult;
        }

        Title = titleResult.Value;
        Slug = slugResult.Value.Value;
        FullPath = ContentPathService.ComputeFullPath(routePrefix, ancestorSlugsRootToParent, Slug);
        Summary = summaryResult.Value;
        Body = (body ?? string.Empty).Trim();
        Seo = seo;

        return Result.Success();
    }

    // Recomputes FullPath alone, leaving Title/Slug/Summary/Body/Seo untouched - called when an
    // ancestor's RoutePrefix or slug changes and this translation's own text did not (Faz 1a Görev 4's
    // cascade). Idempotent no-op when the path does not actually move.
    internal void RecomputeFullPath(string routePrefix, IReadOnlyList<string> ancestorSlugsRootToParent) =>
        FullPath = ContentPathService.ComputeFullPath(routePrefix, ancestorSlugsRootToParent, Slug);

    private static Result<string> NormalizeTitle(string? title)
    {
        var normalized = (title ?? string.Empty).Trim();
        if (normalized.Length == 0 || normalized.Length > MaxTitleLength)
        {
            return Result.Failure<string>(Error.Validation(
                "ContentItemTranslation.TitleInvalid", $"Title is required and must be at most {MaxTitleLength} characters."));
        }

        return Result.Success(normalized);
    }

    private static Result<string> NormalizeSummary(string? summary)
    {
        var normalized = (summary ?? string.Empty).Trim();
        if (normalized.Length > MaxSummaryLength)
        {
            return Result.Failure<string>(Error.Validation(
                "ContentItemTranslation.SummaryTooLong", $"Summary must be at most {MaxSummaryLength} characters."));
        }

        return Result.Success(normalized);
    }
}
