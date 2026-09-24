using GenclikMerkezi.Modules.Website.Features.ActivateSiteLanguage;
using GenclikMerkezi.Modules.Website.Features.CreateSiteLanguage;
using GenclikMerkezi.Modules.Website.Features.DeactivateSiteLanguage;
using GenclikMerkezi.Modules.Website.Features.DeleteMediaAsset;
using GenclikMerkezi.Modules.Website.Features.GetMediaAssetById;
using GenclikMerkezi.Modules.Website.Features.GetMediaAssetFolders;
using GenclikMerkezi.Modules.Website.Features.GetMediaAssets;
using GenclikMerkezi.Modules.Website.Features.GetPublicSite;
using GenclikMerkezi.Modules.Website.Features.GetSiteLanguages;
using GenclikMerkezi.Modules.Website.Features.GetSiteSettings;
using GenclikMerkezi.Modules.Website.Features.SetDefaultSiteLanguage;
using GenclikMerkezi.Modules.Website.Features.UpdateMediaAsset;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteLanguage;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettings;
using GenclikMerkezi.Modules.Website.Features.UploadMediaAsset;
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

        UploadMediaAssetEndpoint.Map(app);
        GetMediaAssetsEndpoint.Map(app);
        GetMediaAssetFoldersEndpoint.Map(app);
        GetMediaAssetByIdEndpoint.Map(app);
        UpdateMediaAssetEndpoint.Map(app);
        DeleteMediaAssetEndpoint.Map(app);

        UpdateSiteSettingsEndpoint.Map(app);
        GetSiteSettingsEndpoint.Map(app);
        GetPublicSiteEndpoint.Map(app);

        return app;
    }
}
