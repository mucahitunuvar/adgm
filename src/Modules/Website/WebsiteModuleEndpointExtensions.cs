using GenclikMerkezi.Modules.Website.Features.ActivateContentCategory;
using GenclikMerkezi.Modules.Website.Features.ActivateContentType;
using GenclikMerkezi.Modules.Website.Features.ActivateSiteLanguage;
using GenclikMerkezi.Modules.Website.Features.ActivateVideo;
using GenclikMerkezi.Modules.Website.Features.ArchiveContentItem;
using GenclikMerkezi.Modules.Website.Features.ConvertNotFoundPathToRedirect;
using GenclikMerkezi.Modules.Website.Features.CreateContentCategory;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.CreateContentType;
using GenclikMerkezi.Modules.Website.Features.CreateRedirect;
using GenclikMerkezi.Modules.Website.Features.CreateSiteLanguage;
using GenclikMerkezi.Modules.Website.Features.CreateVideo;
using GenclikMerkezi.Modules.Website.Features.DeactivateContentCategory;
using GenclikMerkezi.Modules.Website.Features.DeactivateContentType;
using GenclikMerkezi.Modules.Website.Features.DeactivateSiteLanguage;
using GenclikMerkezi.Modules.Website.Features.DeactivateVideo;
using GenclikMerkezi.Modules.Website.Features.DeleteContentCategory;
using GenclikMerkezi.Modules.Website.Features.DeleteContentCategoryTranslation;
using GenclikMerkezi.Modules.Website.Features.DeleteContentItemTranslation;
using GenclikMerkezi.Modules.Website.Features.DeleteContentTypeTranslation;
using GenclikMerkezi.Modules.Website.Features.DeleteMediaAsset;
using GenclikMerkezi.Modules.Website.Features.DeleteNotFoundPath;
using GenclikMerkezi.Modules.Website.Features.DeleteRedirect;
using GenclikMerkezi.Modules.Website.Features.DeleteTag;
using GenclikMerkezi.Modules.Website.Features.DeleteVideo;
using GenclikMerkezi.Modules.Website.Features.DeleteVideoTranslation;
using GenclikMerkezi.Modules.Website.Features.GetContentCategoriesByType;
using GenclikMerkezi.Modules.Website.Features.GetContentItemById;
using GenclikMerkezi.Modules.Website.Features.GetContentItems;
using GenclikMerkezi.Modules.Website.Features.GetContentTypeById;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.GetMediaAssetById;
using GenclikMerkezi.Modules.Website.Features.GetMediaAssetFolders;
using GenclikMerkezi.Modules.Website.Features.GetMediaAssets;
using GenclikMerkezi.Modules.Website.Features.GetNotFoundPaths;
using GenclikMerkezi.Modules.Website.Features.GetPublicSite;
using GenclikMerkezi.Modules.Website.Features.GetPublicVideos;
using GenclikMerkezi.Modules.Website.Features.GetRedirects;
using GenclikMerkezi.Modules.Website.Features.GetSiteLanguages;
using GenclikMerkezi.Modules.Website.Features.GetSiteSettings;
using GenclikMerkezi.Modules.Website.Features.GetTags;
using GenclikMerkezi.Modules.Website.Features.GetVideoById;
using GenclikMerkezi.Modules.Website.Features.GetVideos;
using GenclikMerkezi.Modules.Website.Features.MergeTag;
using GenclikMerkezi.Modules.Website.Features.PublishContentItem;
using GenclikMerkezi.Modules.Website.Features.ResolveRoute;
using GenclikMerkezi.Modules.Website.Features.ScheduleContentItem;
using GenclikMerkezi.Modules.Website.Features.SetContentItemAttachments;
using GenclikMerkezi.Modules.Website.Features.SetContentItemCategories;
using GenclikMerkezi.Modules.Website.Features.SetContentItemGallery;
using GenclikMerkezi.Modules.Website.Features.SetContentItemParent;
using GenclikMerkezi.Modules.Website.Features.SetContentItemTranslationTags;
using GenclikMerkezi.Modules.Website.Features.SetContentItemVideos;
using GenclikMerkezi.Modules.Website.Features.SetDefaultSiteLanguage;
using GenclikMerkezi.Modules.Website.Features.UnarchiveContentItem;
using GenclikMerkezi.Modules.Website.Features.UnpublishContentItem;
using GenclikMerkezi.Modules.Website.Features.UpdateContentCategory;
using GenclikMerkezi.Modules.Website.Features.UpdateContentCategoryTranslation;
using GenclikMerkezi.Modules.Website.Features.UpdateContentItem;
using GenclikMerkezi.Modules.Website.Features.UpdateContentItemTranslation;
using GenclikMerkezi.Modules.Website.Features.UpdateContentType;
using GenclikMerkezi.Modules.Website.Features.UpdateContentTypeTranslation;
using GenclikMerkezi.Modules.Website.Features.UpdateMediaAsset;
using GenclikMerkezi.Modules.Website.Features.UpdateRedirect;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteLanguage;
using GenclikMerkezi.Modules.Website.Features.UpdateVideo;
using GenclikMerkezi.Modules.Website.Features.UpdateVideoTranslation;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsBankAccounts;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsBotProtection;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsContact;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsFeatures;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsIdentity;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsMaintenance;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsTheme;
using GenclikMerkezi.Modules.Website.Features.UpdateTag;
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

        CreateContentTypeEndpoint.Map(app);
        UpdateContentTypeEndpoint.Map(app);
        UpdateContentTypeTranslationEndpoint.Map(app);
        DeleteContentTypeTranslationEndpoint.Map(app);
        ActivateContentTypeEndpoint.Map(app);
        DeactivateContentTypeEndpoint.Map(app);
        GetContentTypesEndpoint.Map(app);
        GetContentTypeByIdEndpoint.Map(app);

        CreateContentItemEndpoint.Map(app);
        UpdateContentItemEndpoint.Map(app);
        UpdateContentItemTranslationEndpoint.Map(app);
        DeleteContentItemTranslationEndpoint.Map(app);
        PublishContentItemEndpoint.Map(app);
        UnpublishContentItemEndpoint.Map(app);
        ArchiveContentItemEndpoint.Map(app);
        UnarchiveContentItemEndpoint.Map(app);
        ScheduleContentItemEndpoint.Map(app);
        SetContentItemParentEndpoint.Map(app);
        GetContentItemsEndpoint.Map(app);
        GetContentItemByIdEndpoint.Map(app);

        CreateRedirectEndpoint.Map(app);
        UpdateRedirectEndpoint.Map(app);
        DeleteRedirectEndpoint.Map(app);
        GetRedirectsEndpoint.Map(app);

        GetNotFoundPathsEndpoint.Map(app);
        DeleteNotFoundPathEndpoint.Map(app);
        ConvertNotFoundPathToRedirectEndpoint.Map(app);

        ResolveRouteEndpoint.Map(app);

        CreateVideoEndpoint.Map(app);
        UpdateVideoEndpoint.Map(app);
        UpdateVideoTranslationEndpoint.Map(app);
        DeleteVideoTranslationEndpoint.Map(app);
        ActivateVideoEndpoint.Map(app);
        DeactivateVideoEndpoint.Map(app);
        DeleteVideoEndpoint.Map(app);
        GetVideosEndpoint.Map(app);
        GetVideoByIdEndpoint.Map(app);
        GetPublicVideosEndpoint.Map(app);

        CreateContentCategoryEndpoint.Map(app);
        UpdateContentCategoryEndpoint.Map(app);
        UpdateContentCategoryTranslationEndpoint.Map(app);
        DeleteContentCategoryTranslationEndpoint.Map(app);
        ActivateContentCategoryEndpoint.Map(app);
        DeactivateContentCategoryEndpoint.Map(app);
        DeleteContentCategoryEndpoint.Map(app);
        GetContentCategoriesByTypeEndpoint.Map(app);

        GetTagsEndpoint.Map(app);
        UpdateTagEndpoint.Map(app);
        DeleteTagEndpoint.Map(app);
        MergeTagEndpoint.Map(app);

        SetContentItemCategoriesEndpoint.Map(app);
        SetContentItemTranslationTagsEndpoint.Map(app);
        SetContentItemGalleryEndpoint.Map(app);
        SetContentItemVideosEndpoint.Map(app);
        SetContentItemAttachmentsEndpoint.Map(app);

        return app;
    }
}
