using GenclikMerkezi.Modules.ReferenceData.Application.Abstractions;
using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.UnitTests.ReferenceData.TestDoubles;

public sealed class FakeAdminLookupCrudService<TLookup> : IAdminLookupCrudService<TLookup>
    where TLookup : LookupItem
{
    private readonly List<TLookup> _items = [];

    public IReadOnlyCollection<TLookup> Items => _items.AsReadOnly();

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.Any(i => i.Code == code));

    public Task<TLookup?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.FirstOrDefault(i => i.Id == id));

    public void Add(TLookup entity) => _items.Add(entity);
}
