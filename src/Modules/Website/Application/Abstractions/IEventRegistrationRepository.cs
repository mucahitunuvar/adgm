using GenclikMerkezi.Modules.Website.Domain;

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
}
