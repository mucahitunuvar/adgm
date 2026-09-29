namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §5 (Faz 1b Görev 2): DeleteVideoCommandHandler's guard against removing a Video some
// ContentItem still references. Content videos does not exist until Görev 4 - see VideoUsageChecker's
// own remarks for why the port is introduced now with an empty implementation.
public interface IVideoUsageChecker
{
    Task<IReadOnlyList<VideoUsage>> GetUsagesAsync(Guid videoId, CancellationToken cancellationToken = default);
}
