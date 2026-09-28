using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.RouteResolution;

// ADR-024 §15 (Faz 1a Görev 6): RouteResolutionService's result. Only the fields relevant to Kind are
// populated - the factory methods below are the only way to construct one, so no caller can set an
// inconsistent combination (e.g. a Location on a Detail outcome).
public sealed record RouteResolutionOutcome
{
    public required RouteResolutionKind Kind { get; init; }

    public string? LanguageCode { get; init; }

    public Guid? ContentItemId { get; init; }

    public string? ContentTypeKey { get; init; }

    public string? ListTemplate { get; init; }

    public string? DetailTemplate { get; init; }

    public string? Name { get; init; }

    public SeoMetadata? Seo { get; init; }

    public IReadOnlyList<RouteAlternate>? Alternates { get; init; }

    public string? Location { get; init; }

    public int? StatusCode { get; init; }

    public static RouteResolutionOutcome Home(string languageCode) =>
        new() { Kind = RouteResolutionKind.Home, LanguageCode = languageCode };

    public static RouteResolutionOutcome NotFound(string languageCode) =>
        new() { Kind = RouteResolutionKind.NotFound, LanguageCode = languageCode };

    public static RouteResolutionOutcome Redirect(string location, int statusCode) =>
        new() { Kind = RouteResolutionKind.Redirect, Location = location, StatusCode = statusCode };

    public static RouteResolutionOutcome Detail(
        string languageCode, Guid contentItemId, string contentTypeKey, string detailTemplate, IReadOnlyList<RouteAlternate> alternates) =>
        new()
        {
            Kind = RouteResolutionKind.Detail,
            LanguageCode = languageCode,
            ContentItemId = contentItemId,
            ContentTypeKey = contentTypeKey,
            DetailTemplate = detailTemplate,
            Alternates = alternates,
        };

    public static RouteResolutionOutcome Listing(
        string languageCode, string contentTypeKey, string listTemplate, string name, SeoMetadata seo, IReadOnlyList<RouteAlternate> alternates) =>
        new()
        {
            Kind = RouteResolutionKind.Listing,
            LanguageCode = languageCode,
            ContentTypeKey = contentTypeKey,
            ListTemplate = listTemplate,
            Name = name,
            Seo = seo,
            Alternates = alternates,
        };
}
