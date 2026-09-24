using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §15 (Görev 3): SEO fields Faz 1's ContentItem (and possibly other future content-bearing
// aggregates) will embed. Built here, ahead of ContentItem itself, as a standalone domain building
// block with no endpoint of its own yet - see the Faz 0 master prompt's Görev 3. Length limits are
// hard validation, not a soft suggestion: search engines truncate past them regardless, so a
// caller-visible error the admin sees immediately is more useful than a silent truncation nobody
// notices until the page is already published.
public sealed class SeoMetadata : ValueObject
{
    public const int MaxMetaTitleLength = 70;
    public const int MaxMetaDescriptionLength = 160;
    public const int MaxMetaKeywordsLength = 255;
    public const int MaxOgTitleLength = 95;
    public const int MaxOgDescriptionLength = 200;
    public const int MaxCanonicalUrlLength = 2048;

    public string MetaTitle { get; }

    public string MetaDescription { get; }

    public string MetaKeywords { get; }

    public string OgTitle { get; }

    public string OgDescription { get; }

    public Guid? OgImageMediaId { get; }

    public string? CanonicalUrl { get; }

    public bool NoIndex { get; }

    private SeoMetadata(
        string metaTitle,
        string metaDescription,
        string metaKeywords,
        string ogTitle,
        string ogDescription,
        Guid? ogImageMediaId,
        string? canonicalUrl,
        bool noIndex)
    {
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        MetaKeywords = metaKeywords;
        OgTitle = ogTitle;
        OgDescription = ogDescription;
        OgImageMediaId = ogImageMediaId;
        CanonicalUrl = canonicalUrl;
        NoIndex = noIndex;
    }

    public static Result<SeoMetadata> Create(
        string? metaTitle,
        string? metaDescription,
        string? metaKeywords,
        string? ogTitle,
        string? ogDescription,
        Guid? ogImageMediaId,
        string? canonicalUrl,
        bool noIndex)
    {
        var normalizedMetaTitle = (metaTitle ?? string.Empty).Trim();
        var normalizedMetaDescription = (metaDescription ?? string.Empty).Trim();
        var normalizedMetaKeywords = (metaKeywords ?? string.Empty).Trim();
        var normalizedOgTitle = (ogTitle ?? string.Empty).Trim();
        var normalizedOgDescription = (ogDescription ?? string.Empty).Trim();
        var normalizedCanonicalUrl = string.IsNullOrWhiteSpace(canonicalUrl) ? null : canonicalUrl.Trim();

        if (normalizedMetaTitle.Length > MaxMetaTitleLength)
        {
            return Result.Failure<SeoMetadata>(Error.Validation(
                "SeoMetadata.MetaTitleTooLong", $"Meta title must be at most {MaxMetaTitleLength} characters."));
        }

        if (normalizedMetaDescription.Length > MaxMetaDescriptionLength)
        {
            return Result.Failure<SeoMetadata>(Error.Validation(
                "SeoMetadata.MetaDescriptionTooLong", $"Meta description must be at most {MaxMetaDescriptionLength} characters."));
        }

        if (normalizedMetaKeywords.Length > MaxMetaKeywordsLength)
        {
            return Result.Failure<SeoMetadata>(Error.Validation(
                "SeoMetadata.MetaKeywordsTooLong", $"Meta keywords must be at most {MaxMetaKeywordsLength} characters."));
        }

        if (normalizedOgTitle.Length > MaxOgTitleLength)
        {
            return Result.Failure<SeoMetadata>(Error.Validation(
                "SeoMetadata.OgTitleTooLong", $"Open Graph title must be at most {MaxOgTitleLength} characters."));
        }

        if (normalizedOgDescription.Length > MaxOgDescriptionLength)
        {
            return Result.Failure<SeoMetadata>(Error.Validation(
                "SeoMetadata.OgDescriptionTooLong", $"Open Graph description must be at most {MaxOgDescriptionLength} characters."));
        }

        if (normalizedCanonicalUrl is not null)
        {
            if (normalizedCanonicalUrl.Length > MaxCanonicalUrlLength)
            {
                return Result.Failure<SeoMetadata>(Error.Validation(
                    "SeoMetadata.CanonicalUrlTooLong", $"Canonical URL must be at most {MaxCanonicalUrlLength} characters."));
            }

            var isAbsoluteHttpUrl = Uri.TryCreate(normalizedCanonicalUrl, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
            if (!isAbsoluteHttpUrl)
            {
                return Result.Failure<SeoMetadata>(Error.Validation(
                    "SeoMetadata.CanonicalUrlInvalid", "Canonical URL must be an absolute http or https URL."));
            }
        }

        return Result.Success(new SeoMetadata(
            normalizedMetaTitle, normalizedMetaDescription, normalizedMetaKeywords, normalizedOgTitle,
            normalizedOgDescription, ogImageMediaId, normalizedCanonicalUrl, noIndex));
    }

    public static SeoMetadata CreateEmpty() =>
        new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, null, null, false);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return MetaTitle;
        yield return MetaDescription;
        yield return MetaKeywords;
        yield return OgTitle;
        yield return OgDescription;
        yield return OgImageMediaId;
        yield return CanonicalUrl;
        yield return NoIndex;
    }
}
