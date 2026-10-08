using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class ContentItemRevisionRepository(WebsiteDbContext dbContext) : IContentItemRevisionRepository
{
    public void Add(ContentItemRevision revision) => dbContext.ContentItemRevisions.Add(revision);

    public Task<ContentItemRevision?> GetLatestAsync(Guid contentItemId, CancellationToken cancellationToken = default) =>
        dbContext.ContentItemRevisions
            .Where(r => r.ContentItemId == contentItemId)
            .OrderByDescending(r => r.RevisionNumber)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<ContentItemRevision?> GetByRevisionNumberAsync(
        Guid contentItemId, int revisionNumber, CancellationToken cancellationToken = default) =>
        dbContext.ContentItemRevisions
            .FirstOrDefaultAsync(r => r.ContentItemId == contentItemId && r.RevisionNumber == revisionNumber, cancellationToken);

    public async Task<PagedResult<ContentItemRevisionSummary>> SearchAsync(
        Guid contentItemId, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ContentItemRevisions.AsNoTracking().Where(r => r.ContentItemId == contentItemId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(r => r.RevisionNumber)
            .Skip((pagedRequest.Page - 1) * pagedRequest.PageSize)
            .Take(pagedRequest.PageSize)
            .Select(r => new ContentItemRevisionSummary(
                r.Id, r.RevisionNumber, r.SavedAtUtc, r.SavedByUserId, r.Kind, r.ChangedLanguages, r.IsPublishedSnapshot))
            .ToListAsync(cancellationToken);

        return new PagedResult<ContentItemRevisionSummary>(items, totalCount, pagedRequest.Page, pagedRequest.PageSize);
    }

    public async Task<IReadOnlyList<Guid>> GetContentItemIdsWithMoreThanAsync(
        int keepCount, int maxItems, CancellationToken cancellationToken = default) =>
        await dbContext.ContentItemRevisions
            .GroupBy(r => r.ContentItemId)
            .Where(g => g.Count() > keepCount)
            .OrderBy(g => g.Key)
            .Select(g => g.Key)
            .Take(maxItems)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ContentItemRevisionRetentionRow>> GetRetentionRowsAsync(
        Guid contentItemId, CancellationToken cancellationToken = default) =>
        await dbContext.ContentItemRevisions.AsNoTracking()
            .Where(r => r.ContentItemId == contentItemId)
            .Select(r => new ContentItemRevisionRetentionRow(r.Id, r.RevisionNumber, r.IsPublishedSnapshot))
            .ToListAsync(cancellationToken);

    public Task<int> DeleteByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return Task.FromResult(0);
        }

        return dbContext.ContentItemRevisions.Where(r => ids.Contains(r.Id)).ExecuteDeleteAsync(cancellationToken);
    }

    public Task<int> DeleteAllForContentItemAsync(Guid contentItemId, CancellationToken cancellationToken = default) =>
        dbContext.ContentItemRevisions.Where(r => r.ContentItemId == contentItemId).ExecuteDeleteAsync(cancellationToken);
}
