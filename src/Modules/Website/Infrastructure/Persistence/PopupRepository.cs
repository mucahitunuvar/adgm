using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class PopupRepository(WebsiteDbContext dbContext) : IPopupRepository
{
    public Task<Popup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Popups.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<PagedResult<Popup>> SearchAsync(
        PopupDisplayMode? displayMode, bool? isActive, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Popups.AsNoTracking().AsQueryable();

        if (displayMode is not null)
        {
            query = query.Where(p => p.DisplayMode == displayMode.Value);
        }

        if (isActive is not null)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        return query.OrderByDescending(p => p.Priority).ThenByDescending(p => p.CreatedAtUtc).ToPagedResultAsync(pagedRequest, cancellationToken);
    }

    public async Task<IReadOnlyList<Popup>> SearchVisibleAsync(
        LanguageCode languageCode, DateTime now, CancellationToken cancellationToken = default) =>
        await dbContext.Popups
            .AsNoTracking()
            .Where(PopupVisibility.IsVisibleAt(now))
            .Where(p => p.Translations.Any(t => t.LanguageCode == languageCode))
            .OrderByDescending(p => p.Priority)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Popup>> SearchByImageMediaIdAsync(Guid mediaAssetId, CancellationToken cancellationToken = default) =>
        await dbContext.Popups.AsNoTracking().Where(p => p.ImageMediaId == mediaAssetId).ToListAsync(cancellationToken);

    public Task<int> CountActiveAndNotExpiredAsync(DateTime now, Guid? excludeId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Popups.AsNoTracking().Where(PopupVisibility.IsActiveAndNotExpiredAt(now));

        if (excludeId is not null)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return query.CountAsync(cancellationToken);
    }

    public Task<DateTime?> GetEarliestUpcomingTransitionAsync(DateTime now, CancellationToken cancellationToken = default)
    {
        var active = dbContext.Popups.AsNoTracking().Where(p => p.IsActive);

        var upcomingTransitions = active.Where(p => p.PublishAtUtc > now).Select(p => p.PublishAtUtc!.Value)
            .Union(active.Where(p => p.UnpublishAtUtc > now).Select(p => p.UnpublishAtUtc!.Value));

        return upcomingTransitions.OrderBy(t => t).Select(t => (DateTime?)t).FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(Popup popup) => dbContext.Popups.Add(popup);

    public void Remove(Popup popup) => dbContext.Popups.Remove(popup);
}
