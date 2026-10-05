namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §14 (Faz 3 Görev 6): the double opt-in state machine. PendingConfirmation only ever becomes
// Active (the confirmation link) or is removed by the retention job after 7 days; Active can only
// become Unsubscribed; Unsubscribed can be brought back to PendingConfirmation by a fresh subscribe
// request for the same email (the "kayıt sızdırmaz" resubscribe flow - SubscribeToNewsletter never
// reveals which branch it took). Stored as a string column, so adding members later needs no migration.
public enum NewsletterSubscriberStatus
{
    PendingConfirmation,
    Active,
    Unsubscribed,
}
