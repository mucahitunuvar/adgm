using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §11.1 (Faz 4 Görev 1). A ContentItem's calendar/capacity information, deliberately a
// separate aggregate from ContentItem (1:1 via ContentItemId) rather than an owned entity: registration
// commands (Görev 3-4) touch the confirmed/waitlisted counters far more often than an editor touches
// the ContentItem itself, and sharing one RowVersion between the two would make every registration
// collide with the editor's own concurrent edit (and vice versa). RowVersion here is the same
// application-managed optimistic-concurrency token as every other Website aggregate (ADR-012):
// regenerated on every mutation, compared explicitly by the command handler - not a database-generated
// column, since Website runs on both SqlServer and Sqlite.
public sealed class EventSchedule : AggregateRoot
{
    public const int MaxOnlineLinkLength = 2048;
    public const int MaxCancellationReasonLength = 500;

    private readonly List<EventScheduleTranslation> _translations = [];

    public Guid ContentItemId { get; private set; }

    public DateTime StartsAtUtc { get; private set; }

    public DateTime EndsAtUtc { get; private set; }

    public EventFormat Format { get; private set; }

    public string? OnlineLink { get; private set; }

    public int? Capacity { get; private set; }

    public bool RegistrationEnabled { get; private set; }

    public DateTime? RegistrationOpensAtUtc { get; private set; }

    public DateTime? RegistrationClosesAtUtc { get; private set; }

    public int? MinAge { get; private set; }

    public int? MaxAge { get; private set; }

    public bool AutoConfirm { get; private set; }

    public bool WaitlistEnabled { get; private set; }

    public bool IsCancelled { get; private set; }

    public DateTime? CancelledAtUtc { get; private set; }

    public string? CancellationReason { get; private set; }

    // Only the capacity-holding statuses (Confirmed, Attended, NoShow - Görev 3/4) increment
    // ConfirmedCount; Applied and PendingVerification never do (§1 Faz 4 "kullanıcı kararları"). This
    // aggregate's own methods are the only place these counters change - no admin endpoint in this
    // Görev writes to them directly.
    public int ConfirmedCount { get; private set; }

    public int WaitlistedCount { get; private set; }

    public IReadOnlyList<EventScheduleTranslation> Translations => _translations.AsReadOnly();

    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    private EventSchedule(
        Guid id,
        Guid contentItemId,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        EventFormat format,
        string? onlineLink,
        int? capacity,
        bool registrationEnabled,
        DateTime? registrationOpensAtUtc,
        DateTime? registrationClosesAtUtc,
        int? minAge,
        int? maxAge,
        bool autoConfirm,
        bool waitlistEnabled,
        Guid createdByUserId,
        DateTime createdAtUtc)
        : base(id)
    {
        ContentItemId = contentItemId;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        Format = format;
        OnlineLink = onlineLink;
        Capacity = capacity;
        RegistrationEnabled = registrationEnabled;
        RegistrationOpensAtUtc = registrationOpensAtUtc;
        RegistrationClosesAtUtc = registrationClosesAtUtc;
        MinAge = minAge;
        MaxAge = maxAge;
        AutoConfirm = autoConfirm;
        WaitlistEnabled = waitlistEnabled;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    private EventSchedule()
    {
    }

    public static Result<EventSchedule> Create(
        Guid contentItemId,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        EventFormat format,
        string? onlineLink,
        int? capacity,
        bool registrationEnabled,
        DateTime? registrationOpensAtUtc,
        DateTime? registrationClosesAtUtc,
        int? minAge,
        int? maxAge,
        bool autoConfirm,
        bool waitlistEnabled,
        LanguageCode defaultLanguageCode,
        string? defaultVenueName,
        string? defaultVenueAddress,
        string? defaultFeeInfo,
        string? defaultInstructors,
        string? defaultProgramFlow,
        string? defaultAccessibilityNote,
        Guid createdByUserId,
        DateTime createdAtUtc)
    {
        var scheduleFieldsResult = ValidateScheduleFields(
            startsAtUtc, endsAtUtc, capacity, registrationClosesAtUtc, minAge, maxAge, currentConfirmedCount: 0);
        if (scheduleFieldsResult.IsFailure)
        {
            return Result.Failure<EventSchedule>(scheduleFieldsResult.Error);
        }

        var onlineLinkResult = NormalizeOnlineLink(onlineLink, format);
        if (onlineLinkResult.IsFailure)
        {
            return Result.Failure<EventSchedule>(onlineLinkResult.Error);
        }

        var translationResult = EventScheduleTranslation.Create(
            defaultLanguageCode, defaultVenueName, defaultVenueAddress, defaultFeeInfo, defaultInstructors, defaultProgramFlow,
            defaultAccessibilityNote);
        if (translationResult.IsFailure)
        {
            return Result.Failure<EventSchedule>(translationResult.Error);
        }

        var schedule = new EventSchedule(
            Guid.NewGuid(), contentItemId, startsAtUtc, endsAtUtc, format, onlineLinkResult.Value, capacity, registrationEnabled,
            registrationOpensAtUtc, registrationClosesAtUtc, minAge, maxAge, autoConfirm, waitlistEnabled, createdByUserId, createdAtUtc);
        schedule._translations.Add(translationResult.Value);

        return Result.Success(schedule);
    }

    // ADR-024 §11.1 "Başlangıç zamanı geçmiş etkinliğin kapasite ve kayıt ayarları değiştirilemez;
    // yalnızca metinler ve iptal güncellenebilir": once `now` has reached the CURRENT (pre-update)
    // StartsAtUtc, every schedule field below is locked - a call that resubmits the same values is a
    // harmless no-op (so a PUT that only changes translations keeps working after the event starts),
    // but any actual change is rejected outright rather than silently ignored.
    public Result UpdateSchedule(
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        EventFormat format,
        string? onlineLink,
        int? capacity,
        bool registrationEnabled,
        DateTime? registrationOpensAtUtc,
        DateTime? registrationClosesAtUtc,
        int? minAge,
        int? maxAge,
        bool autoConfirm,
        bool waitlistEnabled,
        DateTime now,
        Guid updatedByUserId,
        DateTime updatedAtUtc)
    {
        var onlineLinkResult = NormalizeOnlineLink(onlineLink, format);
        if (onlineLinkResult.IsFailure)
        {
            return onlineLinkResult;
        }

        if (now >= StartsAtUtc)
        {
            var unchanged = startsAtUtc == StartsAtUtc
                && endsAtUtc == EndsAtUtc
                && format == Format
                && onlineLinkResult.Value == OnlineLink
                && capacity == Capacity
                && registrationEnabled == RegistrationEnabled
                && registrationOpensAtUtc == RegistrationOpensAtUtc
                && registrationClosesAtUtc == RegistrationClosesAtUtc
                && minAge == MinAge
                && maxAge == MaxAge
                && autoConfirm == AutoConfirm
                && waitlistEnabled == WaitlistEnabled;

            return unchanged
                ? Result.Success()
                : Result.Failure(Error.Conflict(
                    "Event.CannotModifyScheduleAfterStart",
                    "Schedule, capacity and registration settings cannot be changed once the event has started; only texts and cancellation can."));
        }

        var scheduleFieldsResult = ValidateScheduleFields(
            startsAtUtc, endsAtUtc, capacity, registrationClosesAtUtc, minAge, maxAge, ConfirmedCount);
        if (scheduleFieldsResult.IsFailure)
        {
            return scheduleFieldsResult;
        }

        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        Format = format;
        OnlineLink = onlineLinkResult.Value;
        Capacity = capacity;
        RegistrationEnabled = registrationEnabled;
        RegistrationOpensAtUtc = registrationOpensAtUtc;
        RegistrationClosesAtUtc = registrationClosesAtUtc;
        MinAge = minAge;
        MaxAge = maxAge;
        AutoConfirm = autoConfirm;
        WaitlistEnabled = waitlistEnabled;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // Upserts the translation for languageCode, texts only - always allowed regardless of whether the
    // event has started (same "texts stay editable" carve-out UpdateSchedule's guard leaves open).
    public Result SetTranslation(
        LanguageCode languageCode,
        string? venueName,
        string? venueAddress,
        string? feeInfo,
        string? instructors,
        string? programFlow,
        string? accessibilityNote,
        Guid updatedByUserId,
        DateTime updatedAtUtc)
    {
        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is not null)
        {
            var updateResult = existing.Update(venueName, venueAddress, feeInfo, instructors, programFlow, accessibilityNote);
            if (updateResult.IsFailure)
            {
                return updateResult;
            }

            Touch(updatedByUserId, updatedAtUtc);
            return Result.Success();
        }

        var createResult = EventScheduleTranslation.Create(languageCode, venueName, venueAddress, feeInfo, instructors, programFlow, accessibilityNote);
        if (createResult.IsFailure)
        {
            return createResult;
        }

        _translations.Add(createResult.Value);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    public Result Cancel(string? reason, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var reasonResult = NormalizeCancellationReason(reason);
        if (reasonResult.IsFailure)
        {
            return reasonResult;
        }

        IsCancelled = true;
        CancelledAtUtc = updatedAtUtc;
        CancellationReason = reasonResult.Value;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    public Result Reactivate(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        IsCancelled = false;
        CancelledAtUtc = null;
        CancellationReason = null;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // ADR-024 §11.2 (Faz 4 Görev 3): the single place the "kontenjan kararı" counter math happens -
    // EventRegistrationStateResolver only decides whether registration is open/waitlisted/full at all
    // (Görev 2), this decides what a registration that passed that gate actually becomes. Mutates
    // ConfirmedCount/WaitlistedCount only on the branches that hold capacity (AutoConfirm -> Confirmed,
    // waitlist -> Waitlisted); an Applied outcome leaves both untouched, so a call that resolves to
    // Applied produces no EF change at all on this aggregate. Callers (CreateEventRegistration/
    // VerifyEventRegistration, via EventCapacityConcurrencyRetryExecutor) must re-invoke this against a
    // freshly reloaded instance on a RowVersion conflict - it is not itself retry-safe across stale
    // counters.
    public Result<EventCapacityDecision> ReserveCapacity()
    {
        var hasRoom = Capacity is null || ConfirmedCount < Capacity.Value;
        if (hasRoom)
        {
            if (!AutoConfirm)
            {
                return Result.Success(EventCapacityDecision.Applied);
            }

            ConfirmedCount++;
            BumpRowVersion();
            return Result.Success(EventCapacityDecision.Confirmed);
        }

        if (WaitlistEnabled)
        {
            WaitlistedCount++;
            BumpRowVersion();
            return Result.Success(EventCapacityDecision.Waitlisted);
        }

        return Result.Failure<EventCapacityDecision>(Error.Conflict(
            "Event.CapacityFull", "This event has reached its capacity and waitlisting is not enabled."));
    }

    // The counter-releasing counterpart to ReserveCapacity, called when a Confirmed/Waitlisted
    // registration is cancelled (participant link in this Görev, admin actions in Görev 4). Guarded
    // against going negative defensively - every real caller only ever releases a slot its own prior
    // ReserveCapacity call actually reserved.
    public void ReleaseConfirmedSlot()
    {
        if (ConfirmedCount > 0)
        {
            ConfirmedCount--;
            BumpRowVersion();
        }
    }

    public void ReleaseWaitlistSlot()
    {
        if (WaitlistedCount > 0)
        {
            WaitlistedCount--;
            BumpRowVersion();
        }
    }

    private static Result ValidateScheduleFields(
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        int? capacity,
        DateTime? registrationClosesAtUtc,
        int? minAge,
        int? maxAge,
        int currentConfirmedCount)
    {
        if (endsAtUtc <= startsAtUtc)
        {
            return Result.Failure(Error.Validation("EventSchedule.EndsBeforeStarts", "EndsAtUtc must be strictly after StartsAtUtc."));
        }

        if (capacity is not null && capacity.Value < 1)
        {
            return Result.Failure(Error.Validation("EventSchedule.CapacityInvalid", "Capacity must be at least 1 when set."));
        }

        if (capacity is not null && capacity.Value < currentConfirmedCount)
        {
            return Result.Failure(Error.Conflict(
                "Event.CapacityBelowConfirmed", $"Capacity cannot be reduced below the current confirmed count ({currentConfirmedCount})."));
        }

        if (registrationClosesAtUtc is not null && registrationClosesAtUtc.Value > startsAtUtc)
        {
            return Result.Failure(Error.Validation(
                "EventSchedule.RegistrationClosesAfterStart", "RegistrationClosesAtUtc must not be after StartsAtUtc."));
        }

        if (minAge is < 0 || maxAge is < 0)
        {
            return Result.Failure(Error.Validation("EventSchedule.AgeInvalid", "MinAge and MaxAge cannot be negative."));
        }

        if (minAge is not null && maxAge is not null && minAge.Value > maxAge.Value)
        {
            return Result.Failure(Error.Validation("EventSchedule.MinAgeGreaterThanMaxAge", "MinAge cannot be greater than MaxAge."));
        }

        return Result.Success();
    }

    private static Result<string?> NormalizeOnlineLink(string? onlineLink, EventFormat format)
    {
        var trimmed = onlineLink?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return format == EventFormat.InPerson
                ? Result.Success<string?>(null)
                : Result.Failure<string?>(Error.Validation(
                    "EventSchedule.OnlineLinkRequired", "OnlineLink is required for Online or Hybrid events."));
        }

        if (trimmed.Length > MaxOnlineLinkLength
            || !Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps)
        {
            return Result.Failure<string?>(Error.Validation(
                "EventSchedule.OnlineLinkInvalid", $"OnlineLink must be an absolute https URL of at most {MaxOnlineLinkLength} characters."));
        }

        return Result.Success<string?>(trimmed);
    }

    private static Result<string?> NormalizeCancellationReason(string? reason)
    {
        var trimmed = reason?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return Result.Success<string?>(null);
        }

        if (trimmed.Length > MaxCancellationReasonLength)
        {
            return Result.Failure<string?>(Error.Validation(
                "EventSchedule.CancellationReasonTooLong", $"CancellationReason must be at most {MaxCancellationReasonLength} characters."));
        }

        return Result.Success<string?>(trimmed);
    }

    private void Touch(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;
        BumpRowVersion();
    }

    // Registration-driven counter mutations (ReserveCapacity/ReleaseConfirmedSlot/ReleaseWaitlistSlot)
    // bump RowVersion without going through Touch: UpdatedByUserId/UpdatedAtUtc represent an editor's
    // last manual PUT, not registration churn a visitor triggers, mirroring NewsletterSubscriber's own
    // BumpRowVersion split from its admin-facing mutations.
    private void BumpRowVersion() => RowVersion = Guid.NewGuid().ToByteArray();
}
