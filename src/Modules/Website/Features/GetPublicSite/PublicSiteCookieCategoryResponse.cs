namespace GenclikMerkezi.Modules.Website.Features.GetPublicSite;

public sealed record PublicSiteCookieCategoryResponse(
    string Category, string Description, IReadOnlyList<PublicSiteCookieCategoryScriptResponse> Scripts);
