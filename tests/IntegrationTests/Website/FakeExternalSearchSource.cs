using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §10 (Faz 5 Görev 4): a controllable IExternalSearchSource for ExternalSearchSourceSynchronizer
// tests - Documents is mutable so a test can run the synchronizer twice against two different sets
// (proving update/delete), and FailingPages lets a test simulate a page throwing mid-sync ("kısmi hata
// → hiçbir şey silinmez").
public sealed class FakeExternalSearchSource(string sourceKey) : IExternalSearchSource
{
    public string SourceKey { get; } = sourceKey;

    public List<ExternalSearchDocument> Documents { get; set; } = [];

    public HashSet<int> FailingPages { get; set; } = [];

    public Task<PagedResult<ExternalSearchDocument>> GetPublishedDocumentsAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (FailingPages.Contains(page))
        {
            throw new InvalidOperationException($"Simulated failure on page {page} for source '{SourceKey}'.");
        }

        var items = Documents.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult(new PagedResult<ExternalSearchDocument>(items, Documents.Count, page, pageSize));
    }
}
