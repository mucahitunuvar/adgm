using GenclikMerkezi.Modules.Website.Application.Abstractions;

namespace GenclikMerkezi.Modules.Website.Application.Media;

// ADR-024 §5 (Faz 1b Görev 2): the port is introduced now so DeleteVideoCommandHandler already has its
// usage guard wired; ContentItem's video list does not exist until Görev 4, so there is nothing to
// check against yet - always returns empty, same as CompositeMediaUsageChecker did in Faz 0 before its
// first real IMediaUsageProvider was registered. Görev 4 replaces this method's body with a real query
// against IContentItemRepository once that relationship exists.
public sealed class VideoUsageChecker : IVideoUsageChecker
{
    public Task<IReadOnlyList<VideoUsage>> GetUsagesAsync(Guid videoId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<VideoUsage>>([]);
}
