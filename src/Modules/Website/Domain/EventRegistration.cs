using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §11.2 (Faz 4 Görev 3): a single participant's registration for one EventSchedule - a
// separate aggregate from EventSchedule (only an EventScheduleId/ContentItemId reference between
// them, same split ADR-024 §11.1 already applies to EventSchedule vs ContentItem), since a
// registration command mutates this aggregate and EventSchedule's counters together in one Unit of
// Work rather than one aggregate owning the other.
//
// Anonymous registrants never pass CAPTCHA/guard checks a second time once verified - this aggregate
// instead carries its own 24-hour, hashed, single-purpose verification token (never a Data
// Protection-signed one like NewsletterSubscriber's confirmation link): the capacity decision a
// verification click triggers must read this row's own expiry, not re-derive it from a stateless
// token, and "kayıt Rejected olmaz, silinme job'ına bırakılır" (§1) requires the row itself to still
// exist, unresolved, until Görev 5's cleanup job removes it - a signed-but-stateless token has nothing
// to leave behind for that job to find. CancelToken, by contrast, mirrors
// NewsletterSubscriber.UnsubscribeToken exactly (plain random token, verified by direct DB lookup,
// never expires) for the same reason that one documents: a one-click cancellation link must keep
// working for as long as the registration exists.
public sealed partial class EventRegistration : AggregateRoot
{
    public const int MaxFirstNameLength = 100;
    public const int MaxLastNameLength = 100;
    public const int MaxEmailLength = 256;
    public const int MaxPhoneLength = 30;
    public const int MaxTokenLength = 100;

    public static readonly TimeSpan VerificationTokenLifetime = TimeSpan.FromHours(24);
    public static readonly TimeSpan VerificationEmailResendCooldown = TimeSpan.FromMinutes(10);

    private readonly List<EventRegistrationStatusHistoryEntry> _statusHistory = [];

    // Tracks the single history entry (if any) ApplyCapacityDecision added that has not yet survived
    // a successful SaveChangesAsync - never mapped/persisted (see EventRegistrationConfiguration).
    // EventCapacityConcurrencyRetryExecutor may call ApplyCapacityDecision several times against the
    // same in-memory instance across a RowVersion-conflict retry loop (ADR-024 §11.2 "en fazla 3 kez
    // yeniden deneme"); without this, a failed attempt's speculative status/history mutation would
    // still be sitting on the aggregate when the next attempt's SaveChangesAsync finally succeeds,
    // double-recording (or recording the wrong) decision.
    private EventRegistrationStatusHistoryEntry? _uncommittedDecisionEntry;

    public Guid EventScheduleId { get; private set; }

    public Guid ContentItemId { get; private set; }

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string? Phone { get; private set; }

    public Guid? UserId { get; private set; }

    public LanguageCode LanguageCode { get; private set; } = null!;

    public EventRegistrationStatus Status { get; private set; }

    public LegalDocumentKey AcceptedPrivacyNoticeKey { get; private set; } = null!;

    public int AcceptedPrivacyNoticeVersion { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? VerifiedAtUtc { get; private set; }

    public DateTime? StatusChangedAtUtc { get; private set; }

    public DateTime? WaitlistedAtUtc { get; private set; }

    public DateTime? CancelledAtUtc { get; private set; }

    public EventRegistrationCancelledBy? CancelledBy { get; private set; }

    public string? VerificationTokenHash { get; private set; }

    public DateTime? VerificationTokenExpiresAtUtc { get; private set; }

    public DateTime? LastVerificationEmailSentAtUtc { get; private set; }

    public string CancelToken { get; private set; } = string.Empty;

    public DateTime? AnonymizedAtUtc { get; private set; }

    public IReadOnlyList<EventRegistrationStatusHistoryEntry> StatusHistory => _statusHistory.AsReadOnly();

    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();

    // §1 "Yinelenen kayıt: aynı etkinliğe aynı e-postayla aktif ... ikinci kayıt oluşturulmaz" - the
    // statuses that still count as "an open registration attempt" for that duplicate check.
    public bool IsActive =>
        Status is EventRegistrationStatus.PendingVerification or EventRegistrationStatus.Applied
            or EventRegistrationStatus.Confirmed or EventRegistrationStatus.Waitlisted;

    private EventRegistration(
        Guid id, Guid eventScheduleId, Guid contentItemId, string firstName, string lastName, string email, string? phone,
        Guid? userId, LanguageCode languageCode, LegalDocumentKey acceptedPrivacyNoticeKey, int acceptedPrivacyNoticeVersion,
        string? verificationTokenHash, DateTime? verificationTokenExpiresAtUtc, string cancelToken, DateTime createdAtUtc)
        : base(id)
    {
        EventScheduleId = eventScheduleId;
        ContentItemId = contentItemId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        UserId = userId;
        LanguageCode = languageCode;
        Status = EventRegistrationStatus.PendingVerification;
        AcceptedPrivacyNoticeKey = acceptedPrivacyNoticeKey;
        AcceptedPrivacyNoticeVersion = acceptedPrivacyNoticeVersion;
        VerificationTokenHash = verificationTokenHash;
        VerificationTokenExpiresAtUtc = verificationTokenExpiresAtUtc;
        CancelToken = cancelToken;
        CreatedAtUtc = createdAtUtc;
        StatusChangedAtUtc = createdAtUtc;

        _statusHistory.Add(EventRegistrationStatusHistoryEntry.Create(null, EventRegistrationStatus.PendingVerification, "System", createdAtUtc));
    }

    private EventRegistration()
    {
    }

    // requiresVerification callers (the command handler) pass verificationTokenHash/
    // verificationTokenExpiresAtUtc as null for an already-logged-in, email-confirmed registrant: the
    // row still starts as PendingVerification (ApplyCapacityDecision is then called in the very same
    // request, before anything is persisted - see CreateEventRegistrationCommandHandler), simply
    // without ever emailing a verification link for it.
    public static Result<EventRegistration> Create(
        Guid eventScheduleId,
        Guid contentItemId,
        string? firstName,
        string? lastName,
        string? email,
        string? phone,
        Guid? userId,
        LanguageCode languageCode,
        LegalDocumentKey acceptedPrivacyNoticeKey,
        int acceptedPrivacyNoticeVersion,
        string? verificationTokenHash,
        DateTime? verificationTokenExpiresAtUtc,
        string cancelToken,
        DateTime createdAtUtc)
    {
        var firstNameResult = NormalizeRequiredText(firstName, MaxFirstNameLength, "FirstName");
        if (firstNameResult.IsFailure)
        {
            return Result.Failure<EventRegistration>(firstNameResult.Error);
        }

        var lastNameResult = NormalizeRequiredText(lastName, MaxLastNameLength, "LastName");
        if (lastNameResult.IsFailure)
        {
            return Result.Failure<EventRegistration>(lastNameResult.Error);
        }

        var emailResult = NormalizeEmail(email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<EventRegistration>(emailResult.Error);
        }

        var trimmedPhone = phone?.Trim();
        if (trimmedPhone is { Length: > MaxPhoneLength })
        {
            return Result.Failure<EventRegistration>(Error.Validation(
                "EventRegistration.PhoneTooLong", $"Phone must be at most {MaxPhoneLength} characters."));
        }

        if (string.IsNullOrWhiteSpace(cancelToken))
        {
            return Result.Failure<EventRegistration>(Error.Validation(
                "EventRegistration.CancelTokenRequired", "Cancel token is required."));
        }

        return Result.Success(new EventRegistration(
            Guid.NewGuid(), eventScheduleId, contentItemId, firstNameResult.Value, lastNameResult.Value, emailResult.Value,
            string.IsNullOrEmpty(trimmedPhone) ? null : trimmedPhone, userId, languageCode, acceptedPrivacyNoticeKey,
            acceptedPrivacyNoticeVersion, verificationTokenHash, verificationTokenExpiresAtUtc, cancelToken, createdAtUtc));
    }

    // Trim + lowercase, the same normalization every duplicate/lookup query must use to find this row
    // again - mirrors NewsletterSubscriber.NormalizeEmail exactly, exposed so the command handler can
    // normalize a candidate email before querying, without constructing an aggregate first.
    public static Result<string> NormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<string>(Error.Validation("EventRegistration.EmailRequired", "Email is required."));
        }

        var normalized = email.Trim().ToLowerInvariant();
        if (normalized.Length > MaxEmailLength || !EmailPattern().IsMatch(normalized))
        {
            return Result.Failure<string>(Error.Validation("EventRegistration.EmailInvalid", "Email is invalid."));
        }

        return Result.Success(normalized);
    }

    // §1 "Kontenjan kararı" - called once by the command handler for an already-verified registrant
    // (immediately after Create, before the first SaveChangesAsync), and once per verification click
    // for a previously-anonymous one. Safe to call more than once against the same in-memory instance
    // (see _uncommittedDecisionEntry's remarks) - each call first undoes whatever the previous,
    // never-persisted call decided, so only the final call's outcome is ever actually saved.
    public Result ApplyCapacityDecision(EventCapacityDecision decision, string changedBy, DateTime now)
    {
        if (_uncommittedDecisionEntry is not null)
        {
            _statusHistory.Remove(_uncommittedDecisionEntry);
            _uncommittedDecisionEntry = null;
            Status = EventRegistrationStatus.PendingVerification;
            VerifiedAtUtc = null;
            WaitlistedAtUtc = null;
        }

        if (Status != EventRegistrationStatus.PendingVerification)
        {
            return Result.Failure(Error.Conflict(
                "EventRegistration.NotPendingVerification", "This registration is not awaiting a capacity decision."));
        }

        var newStatus = decision switch
        {
            EventCapacityDecision.Confirmed => EventRegistrationStatus.Confirmed,
            EventCapacityDecision.Applied => EventRegistrationStatus.Applied,
            EventCapacityDecision.Waitlisted => EventRegistrationStatus.Waitlisted,
            _ => throw new ArgumentOutOfRangeException(nameof(decision), decision, "Unrecognized capacity decision."),
        };

        var entry = EventRegistrationStatusHistoryEntry.Create(Status, newStatus, changedBy, now);
        Status = newStatus;
        VerifiedAtUtc = now;
        StatusChangedAtUtc = now;
        if (newStatus == EventRegistrationStatus.Waitlisted)
        {
            WaitlistedAtUtc = now;
        }

        _statusHistory.Add(entry);
        _uncommittedDecisionEntry = entry;
        BumpRowVersion();

        return Result.Success();
    }

    // Marks the current (never-persisted) capacity decision as final - called once, right after the
    // SaveChangesAsync that actually persisted it succeeds, so a later unrelated call to
    // ApplyCapacityDecision (there is none in this Görev, but Görev 4's waitlist promotion reuses this
    // same method) does not undo an already-committed decision.
    public void ConfirmCapacityDecisionCommitted() => _uncommittedDecisionEntry = null;

    // §1 "Yedekten otomatik terfi yoktur" / "kayıt sızdırmaz": resending (or first sending) the
    // verification email always mints a fresh token - this aggregate never stores the raw token
    // itself (only its hash), so there is nothing else to resend. Callers gate this on
    // CanSendVerificationEmail first.
    public Result ReissueVerificationToken(string verificationTokenHash, DateTime verificationTokenExpiresAtUtc, DateTime sentAtUtc)
    {
        if (Status != EventRegistrationStatus.PendingVerification)
        {
            return Result.Failure(Error.Conflict(
                "EventRegistration.NotPendingVerification", "This registration is not awaiting verification."));
        }

        VerificationTokenHash = verificationTokenHash;
        VerificationTokenExpiresAtUtc = verificationTokenExpiresAtUtc;
        LastVerificationEmailSentAtUtc = sentAtUtc;
        BumpRowVersion();

        return Result.Success();
    }

    public bool CanSendVerificationEmail(DateTime now) =>
        LastVerificationEmailSentAtUtc is null || now - LastVerificationEmailSentAtUtc >= VerificationEmailResendCooldown;

    // Records the very first verification email (sent for the token Create already issued) -
    // ReissueVerificationToken covers every later resend, which also mints a fresh token.
    public void RecordVerificationEmailSent(DateTime sentAtUtc)
    {
        LastVerificationEmailSentAtUtc = sentAtUtc;
        BumpRowVersion();
    }

    // Participant-initiated cancellation (this Görev) and Görev 4's admin cancellation both call this
    // with their own CancelledBy value. Idempotent on purpose - a one-click cancellation link may be
    // opened more than once (§1 "Aynı token tekrar kullanılırsa idempotent").
    public Result<EventRegistrationCancelOutcome> Cancel(EventRegistrationCancelledBy cancelledBy, DateTime now)
    {
        if (Status == EventRegistrationStatus.Cancelled)
        {
            return Result.Success(EventRegistrationCancelOutcome.AlreadyCancelled);
        }

        if (Status is not (EventRegistrationStatus.PendingVerification or EventRegistrationStatus.Applied
            or EventRegistrationStatus.Confirmed or EventRegistrationStatus.Waitlisted))
        {
            return Result.Failure<EventRegistrationCancelOutcome>(Error.Conflict(
                "EventRegistration.CannotCancel", "This registration cannot be cancelled from its current status."));
        }

        var outcome = Status switch
        {
            EventRegistrationStatus.Confirmed => EventRegistrationCancelOutcome.ReleasedConfirmedSlot,
            EventRegistrationStatus.Waitlisted => EventRegistrationCancelOutcome.ReleasedWaitlistSlot,
            _ => EventRegistrationCancelOutcome.ReleasedNoSlot,
        };

        var entry = EventRegistrationStatusHistoryEntry.Create(Status, EventRegistrationStatus.Cancelled, cancelledBy.ToString(), now);
        Status = EventRegistrationStatus.Cancelled;
        CancelledAtUtc = now;
        CancelledBy = cancelledBy;
        StatusChangedAtUtc = now;
        _statusHistory.Add(entry);
        BumpRowVersion();

        return Result.Success(outcome);
    }

    private static Result<string> NormalizeRequiredText(string? value, int maxLength, string fieldName)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            return Result.Failure<string>(Error.Validation($"EventRegistration.{fieldName}Required", $"{fieldName} is required."));
        }

        if (trimmed.Length > maxLength)
        {
            return Result.Failure<string>(Error.Validation(
                $"EventRegistration.{fieldName}TooLong", $"{fieldName} must be at most {maxLength} characters."));
        }

        return Result.Success(trimmed);
    }

    private void BumpRowVersion() => RowVersion = Guid.NewGuid().ToByteArray();

    // Mirrors NewsletterSubscriber/ContactInfo's own email format check.
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();
}
