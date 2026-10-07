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

    // Faz 4 Görev 5: CleanupExpiredPendingEventRegistrationsJob's own query - §1 "kayıt Rejected olmaz,
    // silinme job'ına bırakılır". PendingVerification never holds capacity, so the job that removes
    // these rows outright never needs to touch EventSchedule's counters.
    Task<IReadOnlyList<EventRegistration>> GetExpiredPendingVerificationAsync(
        DateTime createdBeforeUtc, int maxCount, CancellationToken cancellationToken = default);

    // Faz 4 Görev 5: AnonymizeExpiredEventRegistrationsJob's own query - every non-anonymized
    // registration against one of the already-ended-long-enough-ago schedules the job's own
    // IEventScheduleRepository.GetIdsEndedBeforeAsync call found (not filtered by Status - §1 applies
    // the retention window to every registration of that event, regardless of outcome).
    Task<IReadOnlyList<EventRegistration>> GetDueForAnonymizationAsync(
        IReadOnlyList<Guid> eventScheduleIds, int maxCount, CancellationToken cancellationToken = default);

    // Faz 4 Görev 5: the participant CSV export's own row source - unlike SearchAsync (paged admin
    // list), returns every matching row in one go. ConfirmedAtUtc is derived from StatusHistory's own
    // Confirmed entry (there is no dedicated column for it) rather than VerifiedAtUtc (set for every
    // capacity decision, not only Confirmed) or StatusChangedAtUtc (overwritten by every later
    // transition, e.g. Attended).
    Task<IReadOnlyList<EventRegistrationExportItem>> GetForExportAsync(
        Guid contentItemId, EventRegistrationStatus? status, CancellationToken cancellationToken = default);
}
