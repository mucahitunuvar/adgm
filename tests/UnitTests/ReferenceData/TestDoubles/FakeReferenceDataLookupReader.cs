using GenclikMerkezi.Contracts.ReferenceData;

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

    public Task<IReadOnlyList<LookupItemSummary>> ListAsync(
        ReferenceDataLookupType type, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var items = _items.TryGetValue(type, out var list) ? list : [];
        IReadOnlyList<LookupItemSummary> result = activeOnly ? items.Where(i => i.IsActive).ToList() : items;
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<LookupItemSummary>> ListByParentAsync(
        ReferenceDataLookupType type,
        Guid parentId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<LookupItemSummary>>([]);
}
