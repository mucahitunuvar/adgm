namespace GenclikMerkezi.Modules.Website.Features.ResolveRoute;

// ADR-024 §15 (Faz 1a Görev 6): one flat shape for all five kinds - only the fields relevant to Kind
// are populated (e.g. Location/StatusCode for Redirect, ContentItemId/Alternates for Detail). The
// content item's own data (title, body, breadcrumb, images) is Faz 1b's public detail endpoint, not
// this one.
public sealed record RouteResolutionResponse(
    string Kind,
    string? LanguageCode,
    Guid? ContentItemId,
    string? ContentTypeKey,
    string? ListTemplate,
    string? DetailTemplate,
    string? Name,
    RouteResolutionSeoResponse? Seo,
    IReadOnlyList<RouteAlternateResponse>? Alternates,
    string? Location,
    int? StatusCode);
