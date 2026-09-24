using GenclikMerkezi.Modules.Website.Application.Abstractions;

namespace GenclikMerkezi.Modules.Website.Application.Media;

// Görev 6: the first real IMediaUsageProvider (CompositeMediaUsageChecker previously had none
// registered). Blocks deleting a MediaAsset that SiteSettings' identity group currently references
// as the light/dark logo, the favicon or the default OG image.
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

        if (settings.LogoLightMediaAssetId == mediaAssetId)
        {
            usages.Add(new MediaUsage("site-settings", settings.Id, "Site ayarları - Logo (açık tema)", "/admin/website/settings"));
        }

        if (settings.LogoDarkMediaAssetId == mediaAssetId)
        {
            usages.Add(new MediaUsage("site-settings", settings.Id, "Site ayarları - Logo (koyu tema)", "/admin/website/settings"));
        }

        if (settings.FaviconMediaAssetId == mediaAssetId)
        {
            usages.Add(new MediaUsage("site-settings", settings.Id, "Site ayarları - Favicon", "/admin/website/settings"));
        }

        if (settings.DefaultOgImageMediaId == mediaAssetId)
        {
            usages.Add(new MediaUsage("site-settings", settings.Id, "Site ayarları - Varsayılan OG görseli", "/admin/website/settings"));
        }

        return usages;
    }
}
