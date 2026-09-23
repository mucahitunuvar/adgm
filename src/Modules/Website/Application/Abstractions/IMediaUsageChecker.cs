namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IMediaUsageChecker
{
    Task<IReadOnlyList<MediaUsage>> GetUsagesAsync(Guid mediaAssetId, CancellationToken cancellationToken = default);
}
