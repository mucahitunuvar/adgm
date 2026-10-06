using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeEventRegistrationRepository : IEventRegistrationRepository
{
    private static readonly EventRegistrationStatus[] ActiveStatuses =
    [
        EventRegistrationStatus.PendingVerification, EventRegistrationStatus.Applied, EventRegistrationStatus.Confirmed,
        EventRegistrationStatus.Waitlisted,
    ];

    private static readonly EventRegistrationStatus[] ActiveCancellationNoticeStatuses =
    [
        EventRegistrationStatus.Applied, EventRegistrationStatus.Confirmed, EventRegistrationStatus.Waitlisted,
    ];

    private readonly List<EventRegistration> _registrations = [];

    public void Seed(EventRegistration registration) => _registrations.Add(registration);

    public Task<EventRegistration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_registrations.FirstOrDefault(r => r.Id == id));

    public Task<EventRegistration?> GetByVerificationTokenHashAsync(
        string verificationTokenHash, CancellationToken cancellationToken = default) =>
        Task.FromResult(_registrations.FirstOrDefault(r => r.VerificationTokenHash == verificationTokenHash));

    public Task<EventRegistration?> GetByCancelTokenAsync(string cancelToken, CancellationToken cancellationToken = default) =>
        Task.FromResult(_registrations.FirstOrDefault(r => r.CancelToken == cancelToken));

    public Task<EventRegistration?> GetActiveByContentItemIdAndEmailAsync(
        Guid contentItemId, string normalizedEmail, CancellationToken cancellationToken = default) =>
        Task.FromResult(_registrations.FirstOrDefault(
            r => r.ContentItemId == contentItemId && r.Email == normalizedEmail && ActiveStatuses.Contains(r.Status)));

    public Task<bool> HasRegistrationsAsync(Guid contentItemId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_registrations.Any(r => r.ContentItemId == contentItemId));

    public void Add(EventRegistration registration) => _registrations.Add(registration);

    public void Remove(EventRegistration registration) => _registrations.Remove(registration);

    public Task<PagedResult<EventRegistrationListItem>> SearchAsync(
        Guid contentItemId, EventRegistrationStatus? status, string? search, PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        var query = _registrations.Where(r => r.ContentItemId == contentItemId);
        if (status is not null)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(r =>
                r.FirstName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase)
                || r.LastName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase)
                || r.Email.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
        }

        var ordered = status == EventRegistrationStatus.Waitlisted
            ? query.OrderBy(r => r.WaitlistedAtUtc)
            : query.OrderBy(r => r.CreatedAtUtc);

        var items = ordered
            .Select(r => new EventRegistrationListItem(
                r.Id, r.FirstName, r.LastName, r.Email, r.Phone, r.Status, r.CreatedAtUtc, r.VerifiedAtUtc, r.WaitlistedAtUtc, r.RowVersion))
            .ToList();

        return Task.FromResult(new PagedResult<EventRegistrationListItem>(items, items.Count, pagedRequest.Page, pagedRequest.PageSize));
    }

    public Task<IReadOnlyDictionary<EventRegistrationStatus, int>> GetStatusCountsAsync(
        Guid contentItemId, CancellationToken cancellationToken = default)
    {
        IReadOnlyDictionary<EventRegistrationStatus, int> counts = _registrations
            .Where(r => r.ContentItemId == contentItemId)
            .GroupBy(r => r.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        return Task.FromResult(counts);
    }

    public Task<IReadOnlyList<EventRegistrationCancellationRecipient>> GetCancellationRecipientsAsync(
        Guid contentItemId, int skip, int take, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EventRegistrationCancellationRecipient> recipients = _registrations
            .Where(r => r.ContentItemId == contentItemId && ActiveCancellationNoticeStatuses.Contains(r.Status))
            .OrderBy(r => r.Id)
            .Skip(skip)
            .Take(take)
            .Select(r => new EventRegistrationCancellationRecipient(r.Email, r.LanguageCode))
            .ToList();

        return Task.FromResult(recipients);
    }
}
