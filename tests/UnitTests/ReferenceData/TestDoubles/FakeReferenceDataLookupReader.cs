using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.ReferenceData.TestDoubles;

public sealed class FakeReferenceDataLookupReader : IReferenceDataLookupReader
{
    private readonly HashSet<(ReferenceDataLookupType Type, Guid Id)> _activeIds = [];
    private readonly Dictionary<ReferenceDataLookupType, List<LookupItemSummary>> _items = [];

    public void SeedActive(ReferenceDataLookupType type, Guid id) => _activeIds.Add((type, id));

    public void SeedList(ReferenceDataLookupType type, params LookupItemSummary[] items) =>
        _items[type] = [.. items];

    public Task<bool> ExistsAndActiveAsync(
        ReferenceDataLookupType type, Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_activeIds.Contains((type, id)));

    public Task<PagedResult<LookupItemSummary>> ListAsync(
        ReferenceDataLookupType type,
        PagedRequest paging,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var items = _items.TryGetValue(type, out var list) ? list : [];
        var filtered = activeOnly ? items.Where(i => i.IsActive).ToList() : items;
        var page = filtered.Skip((paging.Page - 1) * paging.PageSize).Take(paging.PageSize).ToList();

        return Task.FromResult(new PagedResult<LookupItemSummary>(page, filtered.Count, paging.Page, paging.PageSize));
    }

    public Task<PagedResult<LookupItemSummary>> ListByParentAsync(
        ReferenceDataLookupType type,
        Guid parentId,
        PagedRequest paging,
        bool activeOnly = true,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new PagedResult<LookupItemSummary>([], 0, paging.Page, paging.PageSize));
}
