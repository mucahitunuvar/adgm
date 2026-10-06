using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class EventRegistrationRepository(WebsiteDbContext dbContext) : IEventRegistrationRepository
{
    private static readonly EventRegistrationStatus[] ActiveStatuses =
    [
        EventRegistrationStatus.PendingVerification, EventRegistrationStatus.Applied, EventRegistrationStatus.Confirmed,
        EventRegistrationStatus.Waitlisted,
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
}
