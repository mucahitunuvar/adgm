namespace GenclikMerkezi.SharedKernel.Results;

// The standard shape for every paginated list query in the system (ARCHITECTURE.md §9). Not
// sealed: a feature's own request/query type may derive from it to add its own filter fields
// (e.g. GetDistrictsQuery adds ProvinceId) instead of PagedRequest itself growing filter concerns
// it has no business owning.
//
// Page/PageSize are silently clamped to a valid range rather than throwing - an out-of-range value
// is not a client error worth a 400, it is just normalized (PERFORMANCE.md §11: pageSize must be
// bounded server-side).
public record PagedRequest
{
    public const int DefaultPageSize = 20;
    public const int MinPageSize = 1;
    public const int MaxPageSize = 100;

    private readonly int _page = 1;

    public int Page
    {
        get => _page;
        init => _page = value < 1 ? 1 : value;
    }

    private readonly int _pageSize = DefaultPageSize;

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = Math.Clamp(value, MinPageSize, MaxPageSize);
    }
}
