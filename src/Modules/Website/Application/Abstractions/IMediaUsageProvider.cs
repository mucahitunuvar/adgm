namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// One provider per Website aggregate that can reference a MediaAsset (none in Faz 0 - SiteSettings
// is the first, in Görev 6). CompositeMediaUsageChecker fans out to every registered provider.
public interface IMediaUsageProvider
{
    Task<IReadOnlyList<MediaUsage>> GetUsagesAsync(Guid mediaAssetId, CancellationToken cancellationToken = default);
}
