using GenclikMerkezi.Modules.Website.Application.Abstractions;

namespace GenclikMerkezi.Modules.Website.Application.Media;

// Faz 1a Görev 3: the second IMediaUsageProvider (after SiteSettingsMediaUsageProvider, Faz 0 Görev 6).
// Blocks deleting a MediaAsset currently referenced as a content item's cover image, detail image, or
// (in any language) SEO Open Graph image.
public sealed class ContentItemMediaUsageProvider(IContentItemRepository contentItemRepository, ISiteLanguageRepository siteLanguageRepository)
    : IMediaUsageProvider
{
    public async Task<IReadOnlyList<MediaUsage>> GetUsagesAsync(Guid mediaAssetId, CancellationToken cancellationToken = default)
    {
        var items = await contentItemRepository.GetByMediaAssetIdAsync(mediaAssetId, cancellationToken);
        if (items.Count == 0)
        {
            return [];
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);

        var usages = new List<MediaUsage>();
        foreach (var item in items)
        {
            var title = ResolveDisplayTitle(item, defaultLanguage);
            var url = $"/admin/website/content/{item.Id}";

            if (item.CoverImageMediaId == mediaAssetId)
            {
                usages.Add(new MediaUsage("content-item", item.Id, $"İçerik - Kapak görseli: {title}", url));
            }

            if (item.DetailImageMediaId == mediaAssetId)
            {
                usages.Add(new MediaUsage("content-item", item.Id, $"İçerik - Detay görseli: {title}", url));
            }

            if (item.Translations.Any(t => t.Seo.OgImageMediaId == mediaAssetId))
            {
                usages.Add(new MediaUsage("content-item", item.Id, $"İçerik - OG görseli: {title}", url));
            }
        }

        return usages;
    }

    private static string ResolveDisplayTitle(Domain.ContentItem item, Domain.SiteLanguage? defaultLanguage)
    {
        var translation = defaultLanguage is not null
            ? item.Translations.FirstOrDefault(t => t.LanguageCode == defaultLanguage.Code)
            : null;

        return (translation ?? item.Translations.FirstOrDefault())?.Title ?? item.Id.ToString();
    }
}
