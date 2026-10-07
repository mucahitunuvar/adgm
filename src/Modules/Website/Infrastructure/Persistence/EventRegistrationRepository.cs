using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class EventRegistrationRepository(WebsiteDbContext dbContext) : IEventRegistrationRepository
{
    private static readonly EventRegistrationStatus[] ActiveStatuses =
    [
        EventRegistrationStatus.PendingVerification, EventRegistrationStatus.Applied, EventRegistrationStatus.Confirmed,
        EventRegistrationStatus.Waitlisted,
    ];

    // Faz 4 Görev 4: the event-cancellation fan-out's own recipient set (§1) - deliberately excludes
    // PendingVerification (never held capacity, and §1 "kayıt Rejected olmaz" means it is left for
    // Görev 5's cleanup job regardless of the event's own cancellation) and Rejected/Cancelled (already
    // resolved, nothing to notify).
    private static readonly EventRegistrationStatus[] ActiveCancellationNoticeStatuses =
    [
        EventRegistrationStatus.Applied, EventRegistrationStatus.Confirmed, EventRegistrationStatus.Waitlisted,
    ];

    public Task<EventRegistration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.EventRegistrations.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<EventRegistration?> GetByVerificationTokenHashAsync(
        string verificationTokenHash, CancellationToken cancellationToken = default) =>
        dbContext.EventRegistrations.FirstOrDefaultAsync(r => r.VerificationTokenHash == verificationTokenHash, cancellationToken);

    public Task<EventRegistration?> GetByCancelTokenAsync(string cancelToken, CancellationToken cancellationToken = default) =>
        dbContext.EventRegistrations.FirstOrDefaultAsync(r => r.CancelToken == cancelToken, cancellationToken);

    public Task<EventRegistration?> GetActiveByContentItemIdAndEmailAsync(
        Guid contentItemId, string normalizedEmail, CancellationToken cancellationToken = default) =>
        dbContext.EventRegistrations.FirstOrDefaultAsync(
            r => r.ContentItemId == contentItemId && r.Email == normalizedEmail && ActiveStatuses.Contains(r.Status), cancellationToken);

    public Task<bool> HasRegistrationsAsync(Guid contentItemId, CancellationToken cancellationToken = default) =>
        dbContext.EventRegistrations.AnyAsync(r => r.ContentItemId == contentItemId, cancellationToken);

    public void Add(EventRegistration registration) => dbContext.EventRegistrations.Add(registration);

    public void Remove(EventRegistration registration) => dbContext.EventRegistrations.Remove(registration);

    public Task<PagedResult<EventRegistrationListItem>> SearchAsync(
        Guid contentItemId, EventRegistrationStatus? status, string? search, PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.EventRegistrations.AsNoTracking().Where(r => r.ContentItemId == contentItemId);

        if (status is not null)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLowerInvariant();
            query = query.Where(r =>
                r.FirstName.ToLower().Contains(normalizedSearch)
                || r.LastName.ToLower().Contains(normalizedSearch)
                || r.Email.Contains(normalizedSearch));
        }

        var projected = status == EventRegistrationStatus.Waitlisted
            ? query.OrderBy(r => r.WaitlistedAtUtc)
            : query.OrderBy(r => r.CreatedAtUtc);

        return projected
            .Select(r => new EventRegistrationListItem(
                r.Id, r.FirstName, r.LastName, r.Email, r.Phone, r.Status, r.CreatedAtUtc, r.VerifiedAtUtc,
                r.WaitlistedAtUtc, r.RowVersion))
            .ToPagedResultAsync(pagedRequest, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<EventRegistrationStatus, int>> GetStatusCountsAsync(
        Guid contentItemId, CancellationToken cancellationToken = default)
    {
        var counts = await dbContext.EventRegistrations.AsNoTracking()
            .Where(r => r.ContentItemId == contentItemId)
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(c => c.Status, c => c.Count);
    }

    public async Task<IReadOnlyList<EventRegistrationCancellationRecipient>> GetCancellationRecipientsAsync(
        Guid contentItemId, int skip, int take, CancellationToken cancellationToken = default) =>
        await dbContext.EventRegistrations.AsNoTracking()
            .Where(r => r.ContentItemId == contentItemId && ActiveCancellationNoticeStatuses.Contains(r.Status))
            .OrderBy(r => r.Id)
            .Skip(skip)
            .Take(take)
            .Select(r => new EventRegistrationCancellationRecipient(r.Email, r.LanguageCode))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<EventRegistration>> GetExpiredPendingVerificationAsync(
        DateTime createdBeforeUtc, int maxCount, CancellationToken cancellationToken = default) =>
        await dbContext.EventRegistrations
            .Where(r => r.Status == EventRegistrationStatus.PendingVerification && r.CreatedAtUtc <= createdBeforeUtc)
            .Take(maxCount)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<EventRegistration>> GetDueForAnonymizationAsync(
        IReadOnlyList<Guid> eventScheduleIds, int maxCount, CancellationToken cancellationToken = default)
    {
        if (eventScheduleIds.Count == 0)
        {
            return [];
        }

        return await dbContext.EventRegistrations
            .Where(r => r.AnonymizedAtUtc == null && eventScheduleIds.Contains(r.EventScheduleId))
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EventRegistrationExportItem>> GetForExportAsync(
        Guid contentItemId, EventRegistrationStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbContext.EventRegistrations.AsNoTracking().Where(r => r.ContentItemId == contentItemId);
        if (status is not null)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var registrations = await query.OrderBy(r => r.CreatedAtUtc).ToListAsync(cancellationToken);

        return registrations
            .Select(r => new EventRegistrationExportItem(
                r.FirstName, r.LastName, r.Email, r.Phone, r.Status, r.CreatedAtUtc,
                r.StatusHistory.FirstOrDefault(h => h.NewStatus == EventRegistrationStatus.Confirmed)?.OccurredAtUtc,
                r.LanguageCode.Value))
            .ToList();
    }
}
