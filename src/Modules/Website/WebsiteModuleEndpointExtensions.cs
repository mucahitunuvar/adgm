using GenclikMerkezi.Modules.Website.Features.ActivateSiteLanguage;
using GenclikMerkezi.Modules.Website.Features.CreateSiteLanguage;
using GenclikMerkezi.Modules.Website.Features.DeactivateSiteLanguage;
using GenclikMerkezi.Modules.Website.Features.GetSiteLanguages;
using GenclikMerkezi.Modules.Website.Features.SetDefaultSiteLanguage;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteLanguage;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website;

public static class WebsiteModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapWebsiteModuleEndpoints(this IEndpointRouteBuilder app)
    {
        CreateSiteLanguageEndpoint.Map(app);
        UpdateSiteLanguageEndpoint.Map(app);
        ActivateSiteLanguageEndpoint.Map(app);
        DeactivateSiteLanguageEndpoint.Map(app);
        SetDefaultSiteLanguageEndpoint.Map(app);
        GetSiteLanguagesEndpoint.Map(app);

        return app;
    }
}
