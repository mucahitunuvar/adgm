using GenclikMerkezi.Modules.Website.Application.Abstractions;

namespace GenclikMerkezi.Modules.Website.Application.Media;

// ADR-024 §6: fans out to every registered IMediaUsageProvider and merges the results. Registered
// with an empty provider list in Faz 0 (Görev 6 adds the first, for SiteSettings) - Faz 0 is
// therefore correctly "nothing is ever in use", not an error.
public sealed class CompositeMediaUsageChecker(IEnumerable<IMediaUsageProvider> providers) : IMediaUsageChecker
{
    public async Task<IReadOnlyList<MediaUsage>> GetUsagesAsync(Guid mediaAssetId, CancellationToken cancellationToken = default)
    {
        var usages = new List<MediaUsage>();

        foreach (var provider in providers)
        {
            usages.AddRange(await provider.GetUsagesAsync(mediaAssetId, cancellationToken));
        }

        return usages;
    }
}
