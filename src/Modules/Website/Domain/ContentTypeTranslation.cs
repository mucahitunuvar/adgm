using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4.1 (Faz 1a Görev 2): per-language name, URL prefix and listing-page SEO. RoutePrefix may
// be empty (a root-level type like "page") - only a non-empty value is run through Slug's
// normalization, so an admin-typed prefix follows exactly the same rules a ContentItem's own slug
// will (Görev 3). Update/Create are the only place that normalizes it; ContentType (the aggregate
// root) is what enforces the empty-prefix/HasListingPage cross-field invariant, since that also
// depends on the root's own flags.
public sealed class ContentTypeTranslation : Entity
{
    public const int MaxNameLength = 100;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public string RoutePrefix { get; private set; } = string.Empty;

    public SeoMetadata Seo { get; private set; } = SeoMetadata.CreateEmpty();

    private ContentTypeTranslation(Guid id, LanguageCode languageCode, string name, string routePrefix, SeoMetadata seo)
        : base(id)
    {
        LanguageCode = languageCode;
        Name = name;
        RoutePrefix = routePrefix;
        Seo = seo;
    }

    private ContentTypeTranslation()
    {
    }

    public static Result<ContentTypeTranslation> Create(LanguageCode languageCode, string? name, string? routePrefix, SeoMetadata seo)
    {
        var nameResult = NormalizeName(name);
        if (nameResult.IsFailure)
        {
            return Result.Failure<ContentTypeTranslation>(nameResult.Error);
        }

        var routePrefixResult = NormalizeRoutePrefix(routePrefix);
        if (routePrefixResult.IsFailure)
        {
            return Result.Failure<ContentTypeTranslation>(routePrefixResult.Error);
        }

        return Result.Success(new ContentTypeTranslation(Guid.NewGuid(), languageCode, nameResult.Value, routePrefixResult.Value, seo));
    }

    internal Result Update(string? name, string? routePrefix, SeoMetadata seo)
    {
        var nameResult = NormalizeName(name);
        if (nameResult.IsFailure)
        {
            return nameResult;
        }

        var routePrefixResult = NormalizeRoutePrefix(routePrefix);
        if (routePrefixResult.IsFailure)
        {
            return routePrefixResult;
        }

        Name = nameResult.Value;
        RoutePrefix = routePrefixResult.Value;
        Seo = seo;

        return Result.Success();
    }

    private static Result<string> NormalizeName(string? name)
    {
        var normalized = (name ?? string.Empty).Trim();
        if (normalized.Length == 0 || normalized.Length > MaxNameLength)
        {
            return Result.Failure<string>(Error.Validation(
                "ContentTypeTranslation.NameInvalid", $"Name is required and must be at most {MaxNameLength} characters."));
        }

        return Result.Success(normalized);
    }

    private static Result<string> NormalizeRoutePrefix(string? routePrefix)
    {
        if (string.IsNullOrWhiteSpace(routePrefix))
        {
            return Result.Success(string.Empty);
        }

        var slugResult = Slug.Create(routePrefix);
        return slugResult.IsFailure ? Result.Failure<string>(slugResult.Error) : Result.Success(slugResult.Value.Value);
    }
}
