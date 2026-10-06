using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class EventScheduleRepository(WebsiteDbContext dbContext) : IEventScheduleRepository
{
    public Task<EventSchedule?> GetByContentItemIdAsync(Guid contentItemId, CancellationToken cancellationToken = default) =>
        dbContext.EventSchedules.FirstOrDefaultAsync(es => es.ContentItemId == contentItemId, cancellationToken);

    public Task<bool> ExistsForContentTypeIdAsync(Guid contentTypeId, CancellationToken cancellationToken = default)
    {
        var contentItemIds = dbContext.ContentItems.Where(ci => ci.ContentTypeId == contentTypeId).Select(ci => ci.Id);
        return dbContext.EventSchedules.AnyAsync(es => contentItemIds.Contains(es.ContentItemId), cancellationToken);
    }

    public async Task<PagedResult<PublicEventListItemCandidate>> SearchPublicAsync(
        Guid? contentTypeId,
        EventFormat? format,
        DateTime? from,
        DateTime? to,
        EventTimeWindow window,
        DateTime now,
        LanguageCode languageCode,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        var activeEventTypeIds = dbContext.ContentTypes.Where(ct => ct.IsActive && ct.SupportsEvent).Select(ct => ct.Id);

        var visibleContentItems = dbContext.ContentItems.AsNoTracking()
            .Where(ContentItemVisibility.IsVisibleAt(now))
            .Where(ci => activeEventTypeIds.Contains(ci.ContentTypeId))
            .Where(ci => ci.Translations.Any(t => t.LanguageCode == languageCode));

        if (contentTypeId is not null)
        {
            visibleContentItems = visibleContentItems.Where(ci => ci.ContentTypeId == contentTypeId.Value);
        }

        var joined = dbContext.EventSchedules.AsNoTracking()
            .Join(visibleContentItems, es => es.ContentItemId, ci => ci.Id, (es, ci) => new { es, ci });

        if (format is not null)
        {
            joined = joined.Where(x => x.es.Format == format.Value);
        }

        if (from is not null)
        {
            joined = joined.Where(x => x.es.StartsAtUtc >= from.Value);
        }

        if (to is not null)
        {
            joined = joined.Where(x => x.es.StartsAtUtc <= to.Value);
        }

        joined = window switch
        {
            EventTimeWindow.Upcoming => joined.Where(x => x.es.EndsAtUtc >= now),
            EventTimeWindow.Past => joined.Where(x => x.es.EndsAtUtc < now),
            _ => joined,
        };

        var projected = joined.Select(x => new
        {
            x.ci.Id,
            Title = x.ci.Translations.Where(t => t.LanguageCode == languageCode).Select(t => t.Title).First(),
            FullPath = x.ci.Translations.Where(t => t.LanguageCode == languageCode).Select(t => t.FullPath).First(),
            x.ci.CoverImageMediaId,
            x.es.StartsAtUtc,
            x.es.EndsAtUtc,
            x.es.Format,
            VenueName = x.es.Translations.Where(t => t.LanguageCode == languageCode).Select(t => t.VenueName).FirstOrDefault() ?? string.Empty,
            x.es.IsCancelled,
        });

        projected = window == EventTimeWindow.Past
            ? projected.OrderByDescending(x => x.StartsAtUtc)
            : projected.OrderBy(x => x.StartsAtUtc);

        var paged = await projected.ToPagedResultAsync(pagedRequest, cancellationToken);

        var items = paged.Items
            .Select(x => new PublicEventListItemCandidate(
                x.Id, x.Title, x.FullPath, x.CoverImageMediaId, x.StartsAtUtc, x.EndsAtUtc, x.Format, x.VenueName, x.IsCancelled))
            .ToList();

        return new PagedResult<PublicEventListItemCandidate>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }

    public async Task<EventRegistrationStateInputs?> GetRegistrationStateInputsByContentItemIdAsync(
        Guid contentItemId, CancellationToken cancellationToken = default)
    {
        var row = await dbContext.EventSchedules.AsNoTracking()
            .Where(es => es.ContentItemId == contentItemId)
            .Select(es => new
            {
                es.IsCancelled,
                es.RegistrationEnabled,
                es.RegistrationOpensAtUtc,
                es.RegistrationClosesAtUtc,
                es.StartsAtUtc,
                es.Capacity,
                es.ConfirmedCount,
                es.WaitlistEnabled,
            })
            .FirstOrDefaultAsync(cancellationToken);

        return row is null
            ? null
            : new EventRegistrationStateInputs(
                row.IsCancelled, row.RegistrationEnabled, row.RegistrationOpensAtUtc, row.RegistrationClosesAtUtc, row.StartsAtUtc,
                row.Capacity, row.ConfirmedCount, row.WaitlistEnabled);
    }

    public async Task<IReadOnlyDictionary<Guid, EventRegistrationStateInputs>> GetRegistrationStateInputsByContentItemIdsAsync(
        IReadOnlyList<Guid> contentItemIds, CancellationToken cancellationToken = default)
    {
        if (contentItemIds.Count == 0)
        {
            return new Dictionary<Guid, EventRegistrationStateInputs>();
        }

        var rows = await dbContext.EventSchedules.AsNoTracking()
            .Where(es => contentItemIds.Contains(es.ContentItemId))
            .Select(es => new
            {
                es.ContentItemId,
                es.IsCancelled,
                es.RegistrationEnabled,
                es.RegistrationOpensAtUtc,
                es.RegistrationClosesAtUtc,
                es.StartsAtUtc,
                es.Capacity,
                es.ConfirmedCount,
                es.WaitlistEnabled,
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(
            r => r.ContentItemId,
            r => new EventRegistrationStateInputs(
                r.IsCancelled, r.RegistrationEnabled, r.RegistrationOpensAtUtc, r.RegistrationClosesAtUtc, r.StartsAtUtc, r.Capacity,
                r.ConfirmedCount, r.WaitlistEnabled));
    }

    public void Add(EventSchedule eventSchedule) => dbContext.EventSchedules.Add(eventSchedule);

    public void Remove(EventSchedule eventSchedule) => dbContext.EventSchedules.Remove(eventSchedule);
}
