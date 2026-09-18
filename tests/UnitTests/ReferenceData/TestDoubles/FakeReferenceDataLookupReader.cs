using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.ReferenceData.TestDoubles;

public sealed class FakeReferenceDataLookupReader : IReferenceDataLookupReader
{
    private readonly HashSet<(ReferenceDataLookupType Type, Guid Id)> _activeIds = [];
    private readonly Dictionary<ReferenceDataLookupType, List<LookupItemSummary>> _items = [];
    private readonly Dictionary<(ReferenceDataLookupType Type, Guid ParentId), List<LookupItemSummary>> _itemsByParent = [];

    public void SeedActive(ReferenceDataLookupType type, Guid id) => _activeIds.Add((type, id));

    public void SeedList(ReferenceDataLookupType type, params LookupItemSummary[] items) =>
        _items[type] = [.. items];

    public void SeedListByParent(ReferenceDataLookupType type, Guid parentId, params LookupItemSummary[] items) =>
        _itemsByParent[(type, parentId)] = [.. items];

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
        CancellationToken cancellationToken = default)
    {
        var items = _itemsByParent.TryGetValue((type, parentId), out var list) ? list : [];
        var filtered = activeOnly ? items.Where(i => i.IsActive).ToList() : items;
        var page = filtered.Skip((paging.Page - 1) * paging.PageSize).Take(paging.PageSize).ToList();

        return Task.FromResult(new PagedResult<LookupItemSummary>(page, filtered.Count, paging.Page, paging.PageSize));
    }

    public Task<IReadOnlyCollection<LookupItemSummary>> GetByIdsAsync(
        ReferenceDataLookupType type, IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        var items = _items.TryGetValue(type, out var list) ? list : [];
        var matched = items.Where(i => ids.Contains(i.Id)).ToList();

        return Task.FromResult<IReadOnlyCollection<LookupItemSummary>>(matched);
    }
}
