using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakePageLayoutRepository : IPageLayoutRepository
{
    private readonly List<PageLayout> _pageLayouts = [];

    public void Seed(PageLayout pageLayout) => _pageLayouts.Add(pageLayout);

    public Task<PageLayout?> GetHomeAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_pageLayouts.FirstOrDefault(p => p.TargetKind == PageLayoutTargetKind.Home));

    public Task<PageLayout?> GetByContentItemIdAsync(Guid contentItemId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_pageLayouts.FirstOrDefault(p => p.ContentItemId == contentItemId));

    public Task<IReadOnlyList<PageLayout>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PageLayout>>(_pageLayouts.ToList());

    public void Add(PageLayout pageLayout) => _pageLayouts.Add(pageLayout);

    public void Remove(PageLayout pageLayout) => _pageLayouts.Remove(pageLayout);
}
