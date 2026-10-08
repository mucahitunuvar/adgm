using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeContentItemRevisionRepository : IContentItemRevisionRepository
{
    private readonly List<ContentItemRevision> _revisions = [];

    public void Seed(ContentItemRevision revision) => _revisions.Add(revision);

    public void Add(ContentItemRevision revision) => _revisions.Add(revision);

    public Task<ContentItemRevision?> GetLatestAsync(Guid contentItemId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_revisions
            .Where(r => r.ContentItemId == contentItemId)
            .OrderByDescending(r => r.RevisionNumber)
            .FirstOrDefault());

    public Task<ContentItemRevision?> GetByRevisionNumberAsync(
        Guid contentItemId, int revisionNumber, CancellationToken cancellationToken = default) =>
        Task.FromResult(_revisions.FirstOrDefault(r => r.ContentItemId == contentItemId && r.RevisionNumber == revisionNumber));

    public Task<PagedResult<ContentItemRevisionSummary>> SearchAsync(
        Guid contentItemId, PagedRequest pagedRequest, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not modeled by this fake - covered by integration tests against the real repository.");

    public Task<IReadOnlyList<Guid>> GetContentItemIdsWithMoreThanAsync(
        int keepCount, int maxItems, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Guid> ids = _revisions
            .GroupBy(r => r.ContentItemId)
            .Where(g => g.Count() > keepCount)
            .Select(g => g.Key)
            .Take(maxItems)
            .ToList();
        return Task.FromResult(ids);
    }

    public Task<IReadOnlyList<ContentItemRevisionRetentionRow>> GetRetentionRowsAsync(
        Guid contentItemId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ContentItemRevisionRetentionRow> rows = _revisions
            .Where(r => r.ContentItemId == contentItemId)
            .Select(r => new ContentItemRevisionRetentionRow(r.Id, r.RevisionNumber, r.IsPublishedSnapshot))
            .ToList();
        return Task.FromResult(rows);
    }

    public Task<int> DeleteByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        var removed = _revisions.RemoveAll(r => ids.Contains(r.Id));
        return Task.FromResult(removed);
    }

    public Task<int> DeleteAllForContentItemAsync(Guid contentItemId, CancellationToken cancellationToken = default)
    {
        var removed = _revisions.RemoveAll(r => r.ContentItemId == contentItemId);
        return Task.FromResult(removed);
    }
}
