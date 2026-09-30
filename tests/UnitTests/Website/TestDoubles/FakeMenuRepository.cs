using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeMenuRepository : IMenuRepository
{
    private readonly List<Menu> _menus = [];

    public void Seed(Menu menu) => _menus.Add(menu);

    public Task<Menu?> GetByLocationAsync(MenuLocation location, CancellationToken cancellationToken = default) =>
        Task.FromResult(_menus.FirstOrDefault(m => m.Location == location));

    public Task<IReadOnlyList<Menu>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Menu>>(_menus.ToList());

    public Task<IReadOnlyList<Menu>> GetByLinkedContentItemIdAsync(Guid contentItemId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Menu>>(
            _menus.Where(m => m.Items.Any(i => i.LinkTarget.Kind == LinkTargetKind.Content && i.LinkTarget.ContentItemId == contentItemId))
                .ToList());
}
