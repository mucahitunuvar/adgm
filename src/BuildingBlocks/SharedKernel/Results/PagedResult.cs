namespace GenclikMerkezi.SharedKernel.Results;

public sealed record PagedResult<TItem>(IReadOnlyList<TItem> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
