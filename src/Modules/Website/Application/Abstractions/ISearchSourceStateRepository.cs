using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface ISearchSourceStateRepository
{
    Task<SearchSourceState?> GetAsync(string sourceKey, CancellationToken cancellationToken = default);

    // Finds the existing row for sourceKey or creates (and stages, via Add) a brand new one - used by
    // ExternalSearchSourceSynchronizer/WebsiteSearchIndexReconciler (Görev 4) so a source's very first
    // sync does not need a separate "does a row already exist" check at every call site.
    Task<SearchSourceState> GetOrCreateAsync(string sourceKey, CancellationToken cancellationToken = default);

    // Every known source's current row (GetSearchSources admin endpoint, Görev 4) - small, unpaged;
    // one row per registered source plus "website".
    Task<IReadOnlyList<SearchSourceState>> GetAllAsync(CancellationToken cancellationToken = default);
}
