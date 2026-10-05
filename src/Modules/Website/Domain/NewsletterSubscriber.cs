using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §14 (Faz 3 Görev 6): a double opt-in newsletter subscription. Everything that determines
// WHETHER a subscribe request is valid (newsletter enabled, the configured privacy notice having an
// effective version, which version was accepted) is cross-aggregate (needs the live SiteSettings/
// LegalDocument) and is therefore the Application-layer command handler's job, the same split
// FormSubmission's own remarks describe - this aggregate only enforces what it can check on its own.
public sealed partial class NewsletterSubscriber : AggregateRoot
{
    public const int MaxEmailLength = 256;

    // §14 "PendingConfirmation ise onay e-postası (en fazla saatte bir) yeniden gönderilir" - applies
    // to a resend while still pending, never to the very first send (ConfirmationEmailLastSentAtUtc is
    // null then).
    public static readonly TimeSpan ConfirmationResendCooldown = TimeSpan.FromHours(1);

    public string Email { get; private set; } = string.Empty;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public NewsletterSubscriberStatus Status { get; private set; }

    public DateTime SubscribedAtUtc { get; private set; }

    public DateTime? ConfirmedAtUtc { get; private set; }

    public DateTime? UnsubscribedAtUtc { get; private set; }

    public LegalDocumentKey AcceptedPrivacyNoticeKey { get; private set; } = null!;

    public int AcceptedPrivacyNoticeVersion { get; private set; }

    // §14 "aboneye özgü rastgele bir UnsubscribeToken değerini taşır ve veritabanında doğrulanır" -
    // deliberately NOT a Data Protection-signed token (unlike the confirmation link): a one-click
    // unsubscribe link must keep working forever, including across a future Data Protection key
    // rotation, so it is verified by direct lookup instead of by signature/expiry.
    public string UnsubscribeToken { get; private set; } = string.Empty;

    public DateTime? ConfirmationEmailLastSentAtUtc { get; private set; }

    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();

    private NewsletterSubscriber(
        Guid id, string email, LanguageCode languageCode, LegalDocumentKey acceptedPrivacyNoticeKey, int acceptedPrivacyNoticeVersion,
        string unsubscribeToken, DateTime subscribedAtUtc)
        : base(id)
    {
        Email = email;
        LanguageCode = languageCode;
        Status = NewsletterSubscriberStatus.PendingConfirmation;
        SubscribedAtUtc = subscribedAtUtc;
        AcceptedPrivacyNoticeKey = acceptedPrivacyNoticeKey;
        AcceptedPrivacyNoticeVersion = acceptedPrivacyNoticeVersion;
        UnsubscribeToken = unsubscribeToken;
    }

    private NewsletterSubscriber()
    {
    }

    public static Result<NewsletterSubscriber> Create(
        string? email,
        LanguageCode languageCode,
        LegalDocumentKey acceptedPrivacyNoticeKey,
        int acceptedPrivacyNoticeVersion,
        string unsubscribeToken,
        DateTime subscribedAtUtc)
    {
        var emailResult = NormalizeEmail(email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<NewsletterSubscriber>(emailResult.Error);
        }

        if (string.IsNullOrWhiteSpace(unsubscribeToken))
        {
            return Result.Failure<NewsletterSubscriber>(Error.Validation(
                "NewsletterSubscriber.UnsubscribeTokenRequired", "Unsubscribe token is required."));
        }

        return Result.Success(new NewsletterSubscriber(
            Guid.NewGuid(), emailResult.Value, languageCode, acceptedPrivacyNoticeKey, acceptedPrivacyNoticeVersion, unsubscribeToken,
            subscribedAtUtc));
    }

    // Trim + lowercase, the same normalization every lookup (GetByEmailAsync) must use to find this
    // row again - exposed so the command handler can normalize a candidate email before querying,
    // without constructing an aggregate first.
    public static Result<string> NormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<string>(Error.Validation("NewsletterSubscriber.EmailRequired", "Email is required."));
        }

        var normalized = email.Trim().ToLowerInvariant();
        if (normalized.Length > MaxEmailLength || !EmailPattern().IsMatch(normalized))
        {
            return Result.Failure<string>(Error.Validation("NewsletterSubscriber.EmailInvalid", "Email is invalid."));
        }

        return Result.Success(normalized);
    }

    public Result Confirm(DateTime confirmedAtUtc)
    {
        if (Status != NewsletterSubscriberStatus.PendingConfirmation)
        {
            return Result.Failure(Error.Conflict(
                "NewsletterSubscriber.NotPendingConfirmation", "This subscription is not awaiting confirmation."));
        }

        Status = NewsletterSubscriberStatus.Active;
        ConfirmedAtUtc = confirmedAtUtc;
        BumpRowVersion();

        return Result.Success();
    }

    // Idempotent on purpose - a one-click unsubscribe link may be opened more than once (email
    // preview scanners, a double click), and the second click must not surface an error.
    public Result Unsubscribe(DateTime unsubscribedAtUtc)
    {
        if (Status == NewsletterSubscriberStatus.Unsubscribed)
        {
            return Result.Success();
        }

        Status = NewsletterSubscriberStatus.Unsubscribed;
        UnsubscribedAtUtc = unsubscribedAtUtc;
        BumpRowVersion();

        return Result.Success();
    }

    // §14 "Unsubscribed ise yeniden PendingConfirmation'a alınır ve onay e-postası gönderilir" - the
    // row keeps its identity (and UnsubscribeToken) across the resubscribe, re-accepting whichever
    // privacy notice version/language the new request carries.
    public void Resubscribe(
        LegalDocumentKey acceptedPrivacyNoticeKey, int acceptedPrivacyNoticeVersion, LanguageCode languageCode, DateTime subscribedAtUtc)
    {
        Status = NewsletterSubscriberStatus.PendingConfirmation;
        SubscribedAtUtc = subscribedAtUtc;
        ConfirmedAtUtc = null;
        UnsubscribedAtUtc = null;
        LanguageCode = languageCode;
        AcceptedPrivacyNoticeKey = acceptedPrivacyNoticeKey;
        AcceptedPrivacyNoticeVersion = acceptedPrivacyNoticeVersion;
        BumpRowVersion();
    }

    public bool CanSendConfirmationEmail(DateTime now) =>
        ConfirmationEmailLastSentAtUtc is null || now - ConfirmationEmailLastSentAtUtc >= ConfirmationResendCooldown;

    public void RecordConfirmationEmailSent(DateTime sentAtUtc)
    {
        ConfirmationEmailLastSentAtUtc = sentAtUtc;
        BumpRowVersion();
    }

    private void BumpRowVersion() => RowVersion = Guid.NewGuid().ToByteArray();

    // Mirrors ContactInfo/FormDefinition's own email format check.
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();
}
