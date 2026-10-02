using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class PageLayoutRepository(WebsiteDbContext dbContext) : IPageLayoutRepository
{
    public Task<PageLayout?> GetHomeAsync(CancellationToken cancellationToken = default) =>
        dbContext.PageLayouts.FirstOrDefaultAsync(p => p.TargetKind == PageLayoutTargetKind.Home, cancellationToken);

    public Task<PageLayout?> GetByContentItemIdAsync(Guid contentItemId, CancellationToken cancellationToken = default) =>
        dbContext.PageLayouts.FirstOrDefaultAsync(p => p.ContentItemId == contentItemId, cancellationToken);

    public async Task<IReadOnlyList<PageLayout>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.PageLayouts.ToListAsync(cancellationToken);

    public void Add(PageLayout pageLayout) => dbContext.PageLayouts.Add(pageLayout);

    public void Remove(PageLayout pageLayout) => dbContext.PageLayouts.Remove(pageLayout);
}
