using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Features.GetSiteLanguages;

public sealed record SiteLanguageResponse(Guid Id, string Code, string Name, bool IsDefault, bool IsActive, int SortOrder)
{
    public static SiteLanguageResponse FromDomain(SiteLanguage siteLanguage) => new(
        siteLanguage.Id, siteLanguage.Code.Value, siteLanguage.Name,
        siteLanguage.IsDefault, siteLanguage.IsActive, siteLanguage.SortOrder);
}
