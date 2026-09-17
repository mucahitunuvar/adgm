using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;

// Lives here (BuildingBlocks.Infrastructure), not SharedKernel: AGENTS.md §16/§18 forbid IQueryable<T>
// and EF Core outside a module's own Infrastructure namespace (enforced by FeatureDbContextTests),
// and SharedKernel is referenced by every module's Domain - giving it an EF Core dependency would
// taint Domain project-wide. This project already depends on EF Core and sits only in the
// Infrastructure tier, so it is the correct home. Callers are repositories (e.g.
// UserRepository.SearchAsync, ReferenceDataLookupReader.ListByParentAsync) that build the query
// internally and hand a materialized PagedResult<T> back up to their Feature handler - handlers never
// see an IQueryable.
public static class QueryablePagingExtensions
{
    // Two round trips (Count, then Skip/Take), not one: EF Core has no reliable, provider-agnostic
    // way to return a total count alongside a page of rows in a single query without either
    // materializing the full result set first or relying on fragile provider-specific tricks. This
    // is the same pattern the codebase already used by hand (UserRepository.SearchAsync,
    // AdminAuditLogRepository.SearchAsync) before this extension centralized it.
    public static async Task<PagedResult<TItem>> ToPagedResultAsync<TItem>(
        this IQueryable<TItem> query, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = totalCount == 0
            ? []
            : await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

        return new PagedResult<TItem>(items, totalCount, request.Page, request.PageSize);
    }
}
