using System.Net;
using GenclikMerkezi.Contracts.Website;
using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.Extensions.Configuration;

namespace GenclikMerkezi.Modules.Website.Application.Events;

// ADR-024 §11.2 (Faz 4 Görev 3): "E-postalar (commit sonrası, dile göre)" - every EventRegistration
// email in one place, keyed by LanguageCode.Value ("tr" falls through to the Turkish branch,
// everything else - including "en" - to the English one, the simplest "dile göre" mechanism that
// fits this Görev; a resource-file/translation-table system would be premature for two languages and
// four fixed templates). Mirrors SubscribeToNewsletterCommandHandler's own inline-HTML-string
// approach (no shared email template engine exists in this module yet) but factored into its own
// class since, unlike the newsletter's single template, four statuses * two languages would otherwise
// duplicate the same link-building logic across two command handlers (Create/Verify).
public sealed class EventRegistrationNotifier(IWebsiteEmailSender websiteEmailSender, IConfiguration configuration)
{
    public Task SendVerificationEmailAsync(
        EventRegistration registration, string eventTitle, string rawVerificationToken, CancellationToken cancellationToken)
    {
        var verifyUrl = BuildSiteUrl($"/events/registrations/verify?token={Uri.EscapeDataString(rawVerificationToken)}");
        var isTurkish = IsTurkish(registration.LanguageCode);

        var subject = isTurkish ? $"Kaydınızı onaylayın: {eventTitle}" : $"Confirm your registration: {eventTitle}";
        var body = isTurkish
            ? $"<p>'{Encode(eventTitle)}' etkinliğine kaydınızı tamamlamak için aşağıdaki bağlantıya tıklayın. Bağlantı 24 saat geçerlidir.</p>"
                + $"<p><a href=\"{verifyUrl}\">Kaydımı onayla</a></p>"
            : $"<p>Click the link below to complete your registration for '{Encode(eventTitle)}'. The link is valid for 24 hours.</p>"
                + $"<p><a href=\"{verifyUrl}\">Confirm my registration</a></p>";

        return websiteEmailSender.SendEmailAsync(registration.Email, subject, body, cancellationToken);
    }

    // §1 "mevcut kayıt PendingVerification ise doğrulama e-postası yeniden gönderilir" and "diğer aktif
    // durumlarda e-posta adresine 'zaten kayıtlısınız' bildirimi ve iptal linki gönderilir" - the
    // latter case, for an email that already has an Applied/Confirmed/Waitlisted registration.
    public Task SendAlreadyRegisteredNoticeAsync(EventRegistration registration, string eventTitle, CancellationToken cancellationToken)
    {
        var cancelUrl = BuildSiteUrl($"/events/registrations/cancel?token={Uri.EscapeDataString(registration.CancelToken)}");
        var isTurkish = IsTurkish(registration.LanguageCode);

        var subject = isTurkish ? $"Zaten kayıtlısınız: {eventTitle}" : $"You are already registered: {eventTitle}";
        var body = isTurkish
            ? $"<p>Bu e-posta adresi '{Encode(eventTitle)}' etkinliği için zaten kayıtlı. Kaydınızdan vazgeçmek isterseniz aşağıdaki bağlantıyı kullanabilirsiniz.</p>"
                + $"<p><a href=\"{cancelUrl}\">Kaydımı iptal et</a></p>"
            : $"<p>This email address is already registered for '{Encode(eventTitle)}'. If you want to withdraw, use the link below.</p>"
                + $"<p><a href=\"{cancelUrl}\">Cancel my registration</a></p>";

        return websiteEmailSender.SendEmailAsync(registration.Email, subject, body, cancellationToken);
    }

    // §1 "Applied ('başvurunuz alındı'); Confirmed (..., online link yalnızca Online/Hybrid etkinlikte,
    // public .ics linki, iptal linki); Waitlisted ('yedek listedesiniz' + iptal linki)" - dispatches on
    // registration.Status, which must already be Applied/Confirmed/Waitlisted (the capacity decision's
    // outcome) by the time this is called.
    public Task SendCapacityDecisionEmailAsync(
        EventRegistration registration, EventSchedule eventSchedule, string eventTitle, CancellationToken cancellationToken)
    {
        var cancelUrl = BuildSiteUrl($"/events/registrations/cancel?token={Uri.EscapeDataString(registration.CancelToken)}");
        var isTurkish = IsTurkish(registration.LanguageCode);

        var (subject, body) = registration.Status switch
        {
            EventRegistrationStatus.Confirmed => BuildConfirmedEmail(registration, eventSchedule, eventTitle, cancelUrl, isTurkish),
            EventRegistrationStatus.Waitlisted => BuildWaitlistedEmail(eventTitle, cancelUrl, isTurkish),
            _ => BuildAppliedEmail(eventTitle, cancelUrl, isTurkish),
        };

        return websiteEmailSender.SendEmailAsync(registration.Email, subject, body, cancellationToken);
    }

    private (string Subject, string Body) BuildAppliedEmail(string eventTitle, string cancelUrl, bool isTurkish)
    {
        var subject = isTurkish ? $"Başvurunuz alındı: {eventTitle}" : $"Your application was received: {eventTitle}";
        var body = isTurkish
            ? $"<p>'{Encode(eventTitle)}' etkinliğine başvurunuz alındı. Başvurunuz incelendikten sonra size bilgi verilecektir.</p>"
                + $"<p><a href=\"{cancelUrl}\">Başvurumu iptal et</a></p>"
            : $"<p>Your application for '{Encode(eventTitle)}' has been received. You will be notified once it has been reviewed.</p>"
                + $"<p><a href=\"{cancelUrl}\">Cancel my application</a></p>";

        return (subject, body);
    }

    private (string Subject, string Body) BuildWaitlistedEmail(string eventTitle, string cancelUrl, bool isTurkish)
    {
        var subject = isTurkish ? $"Yedek listedesiniz: {eventTitle}" : $"You are on the waitlist: {eventTitle}";
        var body = isTurkish
            ? $"<p>'{Encode(eventTitle)}' etkinliği için yedek listeye alındınız. Kontenjan açılırsa size bilgi verilecektir.</p>"
                + $"<p><a href=\"{cancelUrl}\">Kaydımı iptal et</a></p>"
            : $"<p>You have been placed on the waitlist for '{Encode(eventTitle)}'. You will be notified if a spot opens up.</p>"
                + $"<p><a href=\"{cancelUrl}\">Cancel my registration</a></p>";

        return (subject, body);
    }

    private (string Subject, string Body) BuildConfirmedEmail(
        EventRegistration registration, EventSchedule eventSchedule, string eventTitle, string cancelUrl, bool isTurkish)
    {
        var icsUrl = BuildSiteUrl($"/api/v1/public/events/{registration.ContentItemId}/calendar.ics?lang={registration.LanguageCode.Value}");
        var onlineLinkHtml = eventSchedule.Format == EventFormat.InPerson || string.IsNullOrEmpty(eventSchedule.OnlineLink)
            ? string.Empty
            : isTurkish
                ? $"<p>Çevrimiçi katılım bağlantısı: <a href=\"{eventSchedule.OnlineLink}\">{Encode(eventSchedule.OnlineLink)}</a></p>"
                : $"<p>Online participation link: <a href=\"{eventSchedule.OnlineLink}\">{Encode(eventSchedule.OnlineLink)}</a></p>";

        var subject = isTurkish ? $"Kaydınız onaylandı: {eventTitle}" : $"Your registration is confirmed: {eventTitle}";
        var body = isTurkish
            ? $"<p>'{Encode(eventTitle)}' etkinliğine kaydınız onaylandı.</p>"
                + onlineLinkHtml
                + $"<p><a href=\"{icsUrl}\">Takvime ekle (.ics)</a></p>"
                + $"<p><a href=\"{cancelUrl}\">Kaydımı iptal et</a></p>"
            : $"<p>Your registration for '{Encode(eventTitle)}' is confirmed.</p>"
                + onlineLinkHtml
                + $"<p><a href=\"{icsUrl}\">Add to calendar (.ics)</a></p>"
                + $"<p><a href=\"{cancelUrl}\">Cancel my registration</a></p>";

        return (subject, body);
    }

    // Faz 4 Görev 4: admin "reject" - §1 "reason opsiyonel, kişiye iletilmez", so the rejection reason
    // never appears here, only the fact itself.
    public Task SendRejectionNoticeAsync(EventRegistration registration, string eventTitle, CancellationToken cancellationToken)
    {
        var isTurkish = IsTurkish(registration.LanguageCode);

        var subject = isTurkish ? $"Başvurunuz kabul edilmedi: {eventTitle}" : $"Your application was not accepted: {eventTitle}";
        var body = isTurkish
            ? $"<p>'{Encode(eventTitle)}' etkinliğine başvurunuz kabul edilmedi.</p>"
            : $"<p>Your application for '{Encode(eventTitle)}' was not accepted.</p>";

        return websiteEmailSender.SendEmailAsync(registration.Email, subject, body, cancellationToken);
    }

    // Faz 4 Görev 4: admin-initiated cancellation (CancelledBy = Admin) - a Confirmed/Waitlisted/Applied
    // registration the admin withdrew, distinct from the participant's own one-click cancellation (which
    // needs no notice, since the participant already knows they triggered it).
    public Task SendAdminCancellationNoticeAsync(EventRegistration registration, string eventTitle, CancellationToken cancellationToken)
    {
        var isTurkish = IsTurkish(registration.LanguageCode);

        var subject = isTurkish ? $"Kaydınız iptal edildi: {eventTitle}" : $"Your registration was cancelled: {eventTitle}";
        var body = isTurkish
            ? $"<p>'{Encode(eventTitle)}' etkinliğine kaydınız yönetici tarafından iptal edildi.</p>"
            : $"<p>Your registration for '{Encode(eventTitle)}' was cancelled by the organizer.</p>";

        return websiteEmailSender.SendEmailAsync(registration.Email, subject, body, cancellationToken);
    }

    // Faz 4 Görev 4: the event-cancellation fan-out (IEventCancellationNotifier's real implementation) -
    // takes a raw email/language rather than an EventRegistration since it is driven by
    // EventRegistrationCancellationRecipient, a lightweight projection over a potentially large
    // recipient set (§1 "sebep metni dahil; kişiye özel veri yok").
    public Task SendEventCancellationNoticeAsync(
        string email, LanguageCode languageCode, string eventTitle, string? reason, CancellationToken cancellationToken)
    {
        var isTurkish = IsTurkish(languageCode);
        var reasonHtml = string.IsNullOrWhiteSpace(reason)
            ? string.Empty
            : isTurkish ? $"<p>Sebep: {Encode(reason)}</p>" : $"<p>Reason: {Encode(reason)}</p>";

        var subject = isTurkish ? $"Etkinlik iptal edildi: {eventTitle}" : $"Event cancelled: {eventTitle}";
        var body = isTurkish
            ? $"<p>'{Encode(eventTitle)}' etkinliği iptal edildi.</p>{reasonHtml}"
            : $"<p>'{Encode(eventTitle)}' has been cancelled.</p>{reasonHtml}";

        return websiteEmailSender.SendEmailAsync(email, subject, body, cancellationToken);
    }

    private static bool IsTurkish(LanguageCode languageCode) => string.Equals(languageCode.Value, "tr", StringComparison.Ordinal);

    private static string Encode(string value) => WebUtility.HtmlEncode(value);

    // Reuses Website:PublicSiteBaseUrl for the .ics link too (same origin: this deployment's frontend
    // reverse-proxies /api/* to this backend) rather than introducing a second, API-specific base-url
    // setting purely for this one link - see the Görev 3 completion report's assumptions.
    private string BuildSiteUrl(string pathAndQuery)
    {
        var publicSiteBaseUrl = (configuration["Website:PublicSiteBaseUrl"] ?? string.Empty).TrimEnd('/');
        return $"{publicSiteBaseUrl}{pathAndQuery}";
    }
}
