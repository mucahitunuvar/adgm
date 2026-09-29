using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Media;

// ADR-024 §5 (Faz 1b Görev 4): the real implementation, replacing the always-empty stub Görev 2
// introduced before ContentItem.VideoIds existed - mirrors ContentItemMediaUsageProvider's own
// display-title resolution (default language's title, falling back to the first translation).
public sealed class VideoUsageChecker(IContentItemRepository contentItemRepository, ISiteLanguageRepository siteLanguageRepository)
    : IVideoUsageChecker
{
    public async Task<IReadOnlyList<VideoUsage>> GetUsagesAsync(Guid videoId, CancellationToken cancellationToken = default)
    {
        var items = await contentItemRepository.GetByVideoIdAsync(videoId, cancellationToken);
        if (items.Count == 0)
        {
            return [];
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);

        return items
            .Select(item =>
            {
                var title = ResolveDisplayTitle(item, defaultLanguage);
                var url = $"/admin/website/content/{item.Id}";
                return new VideoUsage("content-item", item.Id, $"İçerik - Video: {title}", url);
            })
            .ToList();
    }

    private static string ResolveDisplayTitle(ContentItem item, SiteLanguage? defaultLanguage)
    {
        var translation = defaultLanguage is not null
            ? item.Translations.FirstOrDefault(t => t.LanguageCode == defaultLanguage.Code)
            : null;

        return (translation ?? item.Translations.FirstOrDefault())?.Title ?? item.Id.ToString();
    }
}
