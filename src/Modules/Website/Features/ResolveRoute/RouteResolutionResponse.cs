using System.Text.Json.Serialization;

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
    int? StatusCode,
    // [JsonIgnore]: endpoint-internal only - ResolveRouteEndpoint reads this to log the not-found path
    // (post-Faz-1a fix) without it ever appearing in the public JSON response body.
    [property: JsonIgnore] string? NotFoundLogPath = null,
    // [JsonIgnore]: endpoint-internal only - ResolveRouteEndpoint reads this to send
    // RecordRedirectHitCommand (post-Faz-1a fix) without it ever appearing in the public JSON body.
    [property: JsonIgnore] Guid? RedirectId = null);
