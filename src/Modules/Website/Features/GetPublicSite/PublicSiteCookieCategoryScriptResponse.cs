namespace GenclikMerkezi.Modules.Website.Features.GetPublicSite;

// §13 "her kategorideki aktif script'lerin ad ve amaçları (çerez tercihleri panelinde listelenir)" -
// display-only, unlike PublicSiteScriptResponse's full typed definition.
public sealed record PublicSiteCookieCategoryScriptResponse(string Name, string Purpose);
