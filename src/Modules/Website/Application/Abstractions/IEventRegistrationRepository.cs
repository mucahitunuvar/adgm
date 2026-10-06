using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IEventRegistrationRepository
{
    Task<EventRegistration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // VerifyEventRegistration's lookup - looking up by the hash (never the raw token, which this
    // aggregate never stores) of whatever the verification link presented. A tampered token simply
    // hashes to a value no row has, which is indistinguishable from "expired" at this layer - exactly
    // the "tek tip hata" ADR-024 §11.2 asks for.
    Task<EventRegistration?> GetByVerificationTokenHashAsync(string verificationTokenHash, CancellationToken cancellationToken = default);

    // CancelEventRegistration's lookup - mirrors INewsletterSubscriberRepository.GetByUnsubscribeTokenAsync
    // exactly (direct lookup, no signature/expiry check).
    Task<EventRegistration?> GetByCancelTokenAsync(string cancelToken, CancellationToken cancellationToken = default);

    // §1 "Yinelenen kayıt" guard - the one active (IsActive) registration for this email against this
    // event, if any. Email must already be normalized (EventRegistration.NormalizeEmail) by the caller.
    Task<EventRegistration?> GetActiveByContentItemIdAndEmailAsync(
        Guid contentItemId, string normalizedEmail, CancellationToken cancellationToken = default);

    // IEventRegistrationUsageChecker's real implementation (replacing Görev 1's NoOp) - any row at
    // all, regardless of status: even a Cancelled/Rejected registration is retained personal data
    // (Görev 5's retention job, not a delete), so "kayıtlı etkinliğin içeriği kalıcı silinemez" blocks
    // on its existence too, not just on an active one.
    Task<bool> HasRegistrationsAsync(Guid contentItemId, CancellationToken cancellationToken = default);

    void Add(EventRegistration registration);

    void Remove(EventRegistration registration);

    // Faz 4 Görev 4: GetEventRegistrations' paged/filtered/sorted list - a projection (see
    // EventRegistrationListItem's own remarks), not the full aggregate. Sort is CreatedAtUtc ascending
    // unless status is Waitlisted, in which case it is WaitlistedAtUtc ascending (§1 "yedek listede
    // WaitlistedAtUtc artan").
    Task<PagedResult<EventRegistrationListItem>> SearchAsync(
        Guid contentItemId, EventRegistrationStatus? status, string? search, PagedRequest pagedRequest,
        CancellationToken cancellationToken = default);

    // The list's top-of-response counter summary (Applied/Confirmed/Waitlisted) - Confirmed here means
    // Status == Confirmed specifically (not EventSchedule.ConfirmedCount, which also counts
    // Attended/NoShow); RemainingSpots is computed by the caller from EventSchedule directly.
    Task<IReadOnlyDictionary<EventRegistrationStatus, int>> GetStatusCountsAsync(
        Guid contentItemId, CancellationToken cancellationToken = default);

    // The event-cancellation fan-out's own paged source - only the statuses §1 says must be notified
    // (Applied/Confirmed/Waitlisted), ordered by Id for a stable skip/take across
    // EventCancellationNotifier's inline first batch and ProcessEventCancellationNotificationsJob's
    // overflow continuations.
    Task<IReadOnlyList<EventRegistrationCancellationRecipient>> GetCancellationRecipientsAsync(
        Guid contentItemId, int skip, int take, CancellationToken cancellationToken = default);
}
