using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Media;

// ADR-024 §5 (Faz 1b Görev 4): ContentItem.VideoIds usage, extended in Faz 2 Görev 4 to also cover a
// video-feature block referencing this video in any PageLayout's draft or published blocks.
public sealed class VideoUsageChecker(
    IContentItemRepository contentItemRepository, ISiteLanguageRepository siteLanguageRepository, PageLayoutReferenceScanner scanner)
    : IVideoUsageChecker
{
    public async Task<IReadOnlyList<VideoUsage>> GetUsagesAsync(Guid videoId, CancellationToken cancellationToken = default)
    {
        var usages = new List<VideoUsage>();

        var items = await contentItemRepository.GetByVideoIdAsync(videoId, cancellationToken);
        if (items.Count > 0)
        {
            var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
            usages.AddRange(items.Select(item =>
            {
                var title = ResolveDisplayTitle(item, defaultLanguage);
                var url = $"/admin/website/content/{item.Id}";
                return new VideoUsage("content-item", item.Id, $"İçerik - Video: {title}", url);
            }));
        }

        var layouts = await scanner.FindReferencingAsync(refs => refs.VideoIds.Contains(videoId), cancellationToken);
        usages.AddRange(layouts.Select(layout => new VideoUsage(
            "page-layout", layout.Id, PageLayoutReferenceScanner.DescribeLayout(layout), PageLayoutReferenceScanner.DescribeLayoutUrl(layout))));

        return usages;
    }

    private static string ResolveDisplayTitle(ContentItem item, SiteLanguage? defaultLanguage)
    {
        var translation = defaultLanguage is not null
            ? item.Translations.FirstOrDefault(t => t.LanguageCode == defaultLanguage.Code)
            : null;

        return (translation ?? item.Translations.FirstOrDefault())?.Title ?? item.Id.ToString();
    }
}
