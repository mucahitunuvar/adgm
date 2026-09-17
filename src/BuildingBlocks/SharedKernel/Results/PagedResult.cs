namespace GenclikMerkezi.SharedKernel.Results;

// The standard shape for every paginated list response in the system (ARCHITECTURE.md §9).
// Produced from a PagedRequest, typically via QueryablePagingExtensions.ToPagedResultAsync
// (BuildingBlocks.Infrastructure) in a repository, or directly by a reader that pages an
// already-materialized in-memory list (e.g. ReferenceDataLookupReader's cached lookups).
public sealed record PagedResult<TItem>(IReadOnlyList<TItem> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasNextPage => Page < TotalPages;

    public bool HasPreviousPage => Page > 1;
}
