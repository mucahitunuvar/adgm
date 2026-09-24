namespace GenclikMerkezi.Contracts.Website;

// ADR-024 §1: one of the ports Website defines for project-specific wiring done by a Host-level
// adapter, never by Website itself (which must stay portable and depend on no other module).
// Unlike INotificationModuleContract.SendAsync, this is intentionally email-only and takes no
// UserId: Website's recipients (event registrants, form submitters, newsletter subscribers) are
// frequently not platform users at all, so there is nothing to attach an in-app notification to.
// This project's Host adapter forwards to INotificationModuleContract.SendEmailAsync; a different
// project could implement this port against a different provider entirely without Website's own
// code changing.
public interface IWebsiteEmailSender
{
    Task SendEmailAsync(string recipientEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
