using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakePopupRepository : IPopupRepository
{
    private readonly List<Popup> _popups = [];

    public void Seed(Popup popup) => _popups.Add(popup);

    public Task<Popup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_popups.FirstOrDefault(p => p.Id == id));

    public Task<PagedResult<Popup>> SearchAsync(
        PopupDisplayMode? displayMode, bool? isActive, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var query = _popups.AsEnumerable();
        if (displayMode is not null)
        {
            query = query.Where(p => p.DisplayMode == displayMode.Value);
        }

        if (isActive is not null)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        var items = query.OrderByDescending(p => p.Priority).ToList();
        return Task.FromResult(new PagedResult<Popup>(items, items.Count, pagedRequest.Page, pagedRequest.PageSize));
    }

    public Task<IReadOnlyList<Popup>> SearchVisibleAsync(LanguageCode languageCode, DateTime now, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Popup> visible = _popups
            .Where(p => PopupVisibility.Evaluate(p, now) && p.Translations.Any(t => t.LanguageCode == languageCode))
            .OrderByDescending(p => p.Priority)
            .ToList();
        return Task.FromResult(visible);
    }

    public Task<IReadOnlyList<Popup>> SearchByImageMediaIdAsync(Guid mediaAssetId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Popup>>(_popups.Where(p => p.ImageMediaId == mediaAssetId).ToList());

    public Task<int> CountActiveAndNotExpiredAsync(DateTime now, Guid? excludeId, CancellationToken cancellationToken = default)
    {
        var count = _popups.Count(p => p.Id != excludeId && PopupVisibility.IsActiveAndNotExpired(p, now));
        return Task.FromResult(count);
    }

    public Task<DateTime?> GetEarliestUpcomingTransitionAsync(DateTime now, CancellationToken cancellationToken = default)
    {
        var upcoming = _popups
            .Where(p => p.IsActive)
            .SelectMany(p => new DateTime?[] { p.PublishAtUtc, p.UnpublishAtUtc })
            .Where(t => t > now)
            .OrderBy(t => t)
            .FirstOrDefault();

        return Task.FromResult(upcoming);
    }

    public void Add(Popup popup) => _popups.Add(popup);

    public void Remove(Popup popup) => _popups.RemoveAll(p => p.Id == popup.Id);
}
