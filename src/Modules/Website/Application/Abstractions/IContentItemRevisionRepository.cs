using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IContentItemRevisionRepository
{
    void Add(ContentItemRevision revision);

    Task<ContentItemRevision?> GetLatestAsync(Guid contentItemId, CancellationToken cancellationToken = default);

    Task<ContentItemRevision?> GetByRevisionNumberAsync(
        Guid contentItemId, int revisionNumber, CancellationToken cancellationToken = default);

    // Newest -> oldest, snapshot/hash excluded from the projection (the admin list row never needs it -
    // GetByRevisionNumberAsync is the only read that loads SnapshotJson).
    Task<PagedResult<ContentItemRevisionSummary>> SearchAsync(
        Guid contentItemId, PagedRequest pagedRequest, CancellationToken cancellationToken = default);

    // ADR-024 §4 (Görev 7 retention): content items with more than `keepCount` revisions, at most
    // `maxItems` of them per call - PruneContentItemRevisionsJob's own "en çok 500 içerik" cap.
    Task<IReadOnlyList<Guid>> GetContentItemIdsWithMoreThanAsync(
        int keepCount, int maxItems, CancellationToken cancellationToken = default);

    // Lightweight projection (no SnapshotJson) the retention job uses to decide, per content item,
    // which rows fall outside the kept set (newest `keepCount` + the newest IsPublishedSnapshot row).
    Task<IReadOnlyList<ContentItemRevisionRetentionRow>> GetRetentionRowsAsync(
        Guid contentItemId, CancellationToken cancellationToken = default);

    Task<int> DeleteByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    // Permanent content item deletion (manual or the 30-day trash sweep) - every revision goes with it.
    Task<int> DeleteAllForContentItemAsync(Guid contentItemId, CancellationToken cancellationToken = default);
}
