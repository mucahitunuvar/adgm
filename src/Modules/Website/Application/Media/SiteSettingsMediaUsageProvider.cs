using GenclikMerkezi.Modules.Website.Application.Abstractions;

namespace GenclikMerkezi.Modules.Website.Application.Media;

// Görev 6: the first real IMediaUsageProvider (CompositeMediaUsageChecker previously had none
// registered). Blocks deleting a MediaAsset that SiteSettings.Theme currently references as the
// light/dark logo or the favicon.
public sealed class SiteSettingsMediaUsageProvider(ISiteSettingsRepository siteSettingsRepository) : IMediaUsageProvider
{
    public async Task<IReadOnlyList<MediaUsage>> GetUsagesAsync(Guid mediaAssetId, CancellationToken cancellationToken = default)
    {
        var settings = await siteSettingsRepository.GetAsync(cancellationToken);
        if (settings is null)
        {
            return [];
        }

        var usages = new List<MediaUsage>();

        if (settings.Theme.LogoLightMediaAssetId == mediaAssetId)
        {
            usages.Add(new MediaUsage("site-settings", settings.Id, "Site ayarları - Logo (açık tema)", "/admin/website/settings"));
        }

        if (settings.Theme.LogoDarkMediaAssetId == mediaAssetId)
        {
            usages.Add(new MediaUsage("site-settings", settings.Id, "Site ayarları - Logo (koyu tema)", "/admin/website/settings"));
        }

        if (settings.Theme.FaviconMediaAssetId == mediaAssetId)
        {
            usages.Add(new MediaUsage("site-settings", settings.Id, "Site ayarları - Favicon", "/admin/website/settings"));
        }

        return usages;
    }
}
