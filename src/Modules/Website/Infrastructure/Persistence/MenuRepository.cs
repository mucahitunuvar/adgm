using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class MenuRepository(WebsiteDbContext dbContext) : IMenuRepository
{
    public Task<Menu?> GetByLocationAsync(MenuLocation location, CancellationToken cancellationToken = default) =>
        dbContext.Menus.FirstOrDefaultAsync(m => m.Location == location, cancellationToken);

    public async Task<IReadOnlyList<Menu>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Menus.AsNoTracking().OrderBy(m => m.Location).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Menu>> GetByLinkedContentItemIdAsync(Guid contentItemId, CancellationToken cancellationToken = default) =>
        await dbContext.Menus
            .Where(m => m.Items.Any(i => i.LinkTarget.Kind == LinkTargetKind.Content && i.LinkTarget.ContentItemId == contentItemId))
            .ToListAsync(cancellationToken);
}
