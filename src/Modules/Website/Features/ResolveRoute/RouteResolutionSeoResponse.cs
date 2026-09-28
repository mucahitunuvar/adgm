namespace GenclikMerkezi.Modules.Website.Features.ResolveRoute;

public sealed record RouteResolutionSeoResponse(
    string MetaTitle,
    string MetaDescription,
    string MetaKeywords,
    string OgTitle,
    string OgDescription,
    Guid? OgImageMediaId,
    string? CanonicalUrl,
    bool NoIndex);
