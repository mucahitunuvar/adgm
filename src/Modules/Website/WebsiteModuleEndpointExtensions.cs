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
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsBankAccounts;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsBotProtection;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsContact;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsFeatures;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsIdentity;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsMaintenance;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsTheme;
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

        GetSiteSettingsEndpoint.Map(app);
        UpdateSiteSettingsIdentityEndpoint.Map(app);
        UpdateSiteSettingsThemeEndpoint.Map(app);
        UpdateSiteSettingsContactEndpoint.Map(app);
        UpdateSiteSettingsBankAccountsEndpoint.Map(app);
        UpdateSiteSettingsFeaturesEndpoint.Map(app);
        UpdateSiteSettingsMaintenanceEndpoint.Map(app);
        UpdateSiteSettingsBotProtectionEndpoint.Map(app);
        GetPublicSiteEndpoint.Map(app);

        return app;
    }
}
