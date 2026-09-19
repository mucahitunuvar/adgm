namespace GenclikMerkezi.Contracts.Notification;

// The published, in-process contract other modules depend on to trigger a best-effort notification
// (same pattern as IIdentityService / ICareerAdvisorModuleContract - ADR-016 Decision 2, Option C).
// Implemented in Notification.Infrastructure, registered once at the Host composition root.
// Deliberately synchronous, not Outbox-based: only IdentityDbContext is the CAP transactional
// anchor (ADR-014's amendment / ADR-018), so no other module can publish through its own Outbox -
// callers invoke this directly, after their own transaction has already committed (ADR-022 §3/§6).
// Sends both an email and an in-app notification; failures are recorded, not thrown - this is a
// best-effort side effect, not a source of critical data consistency.
public interface INotificationModuleContract
{
    Task SendAsync(
        Guid userId, string recipientEmail, string subject, string message, CancellationToken cancellationToken = default);

    // Görev 8: aynı içerik birden çok alıcıya - tek tek SendAsync çağırmak yerine tek bir
    // transaction/commit sınırında toplu işlenir.
    Task SendBulkAsync(
        IEnumerable<NotificationRecipient> recipients, string subject, string message, CancellationToken cancellationToken = default);
}
