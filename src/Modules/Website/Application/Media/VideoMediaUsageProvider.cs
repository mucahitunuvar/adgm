using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Media;

// ADR-024 §5 (Faz 1b Görev 2): registered into CompositeMediaUsageChecker (like
// SiteSettingsMediaUsageProvider/ContentItemMediaUsageProvider before it) - a MediaAsset currently used
// as a Video's cover image cannot be deleted.
public sealed class VideoMediaUsageProvider(IVideoRepository videoRepository) : IMediaUsageProvider
{
    public async Task<IReadOnlyList<MediaUsage>> GetUsagesAsync(Guid mediaAssetId, CancellationToken cancellationToken = default)
    {
        var videos = await videoRepository.SearchByCoverImageIdAsync(mediaAssetId, cancellationToken);

        return videos
            .Select(v => new MediaUsage("video", v.Id, BuildDescription(v), "/admin/website/videos"))
            .ToList();
    }

    private static string BuildDescription(Video video)
    {
        var title = video.Translations.FirstOrDefault()?.Title;
        return title is { Length: > 0 } ? $"Video - {title} (kapak)" : "Video - kapak";
    }
}
