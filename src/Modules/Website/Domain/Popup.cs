using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §9 (Faz 2 Görev 6). A simple aggregate mirroring Partner/ImpactMetric's own shape
// (AGENTS.md §10: RowVersion is the same application-managed optimistic-concurrency token every other
// aggregate uses). Cross-aggregate invariants this type cannot check by itself - whether
// Targeting.ContentItemIds still exist, whether a Targeting.Paths entry collides with a configured
// site language code, and "en fazla 20 aktif ve süresi dolmamış pop-up" (needs a database-wide count)
// - are the Application-layer command handlers' responsibility, the same split ContentType's own
// RoutePrefix-uniqueness check already uses.
public sealed class Popup : AggregateRoot
{
    public const int MinDelaySeconds = 0;
    public const int MaxDelaySeconds = 60;
    public const int MinFrequencyDays = 1;
    public const int MaxFrequencyDays = 365;
    public const int MinPriority = 0;
    public const int MaxPriority = 100;

    private readonly List<PopupTranslation> _translations = [];

    public PopupDisplayMode DisplayMode { get; private set; }

    public Guid? ImageMediaId { get; private set; }

    public LinkTarget LinkTarget { get; private set; } = LinkTarget.CreateEmpty();

    public PopupTargeting Targeting { get; private set; } = PopupTargeting.CreateAllPages();

    public PopupDeviceTarget DeviceTarget { get; private set; }

    public DateTime? PublishAtUtc { get; private set; }

    public DateTime? UnpublishAtUtc { get; private set; }

    public bool IsActive { get; private set; }

    public int DelaySeconds { get; private set; }

    public PopupFrequency Frequency { get; private set; }

    public int? FrequencyDays { get; private set; }

    public bool Dismissible { get; private set; }

    public int Priority { get; private set; }

    public IReadOnlyList<PopupTranslation> Translations => _translations.AsReadOnly();

    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    private Popup(
        Guid id, PopupDisplayMode displayMode, Guid? imageMediaId, LinkTarget linkTarget, PopupTargeting targeting,
        PopupDeviceTarget deviceTarget, DateTime? publishAtUtc, DateTime? unpublishAtUtc, int delaySeconds, PopupFrequency frequency,
        int? frequencyDays, bool dismissible, int priority, Guid createdByUserId, DateTime createdAtUtc)
        : base(id)
    {
        DisplayMode = displayMode;
        ImageMediaId = imageMediaId;
        LinkTarget = linkTarget;
        Targeting = targeting;
        DeviceTarget = deviceTarget;
        PublishAtUtc = publishAtUtc;
        UnpublishAtUtc = unpublishAtUtc;
        DelaySeconds = delaySeconds;
        Frequency = frequency;
        FrequencyDays = frequencyDays;
        Dismissible = dismissible;
        Priority = priority;
        IsActive = true;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    private Popup()
    {
    }

    public static Result<Popup> Create(
        PopupDisplayMode displayMode,
        Guid? imageMediaId,
        LinkTarget linkTarget,
        PopupTargeting targeting,
        PopupDeviceTarget deviceTarget,
        DateTime? publishAtUtc,
        DateTime? unpublishAtUtc,
        int delaySeconds,
        PopupFrequency frequency,
        int? frequencyDays,
        bool dismissible,
        int priority,
        LanguageCode defaultLanguageCode,
        string? defaultLanguageTitle,
        string? defaultLanguageBody,
        string? defaultLanguageButtonLabel,
        Guid createdByUserId,
        DateTime createdAtUtc)
    {
        var modeResult = ValidateModeRules(displayMode, imageMediaId, ref delaySeconds, ref dismissible);
        if (modeResult.IsFailure)
        {
            return Result.Failure<Popup>(modeResult.Error);
        }

        var frequencyResult = ValidateFrequency(frequency, frequencyDays);
        if (frequencyResult.IsFailure)
        {
            return Result.Failure<Popup>(frequencyResult.Error);
        }

        var scheduleResult = ValidateSchedule(publishAtUtc, unpublishAtUtc);
        if (scheduleResult.IsFailure)
        {
            return Result.Failure<Popup>(scheduleResult.Error);
        }

        var priorityResult = ValidatePriority(priority);
        if (priorityResult.IsFailure)
        {
            return Result.Failure<Popup>(priorityResult.Error);
        }

        var translationResult = PopupTranslation.Create(
            defaultLanguageCode, displayMode, defaultLanguageTitle, defaultLanguageBody, defaultLanguageButtonLabel);
        if (translationResult.IsFailure)
        {
            return Result.Failure<Popup>(translationResult.Error);
        }

        var buttonLinkResult = ValidateButtonRequiresLink([translationResult.Value], linkTarget);
        if (buttonLinkResult.IsFailure)
        {
            return Result.Failure<Popup>(buttonLinkResult.Error);
        }

        var popup = new Popup(
            Guid.NewGuid(), displayMode, imageMediaId, linkTarget, targeting, deviceTarget, publishAtUtc, unpublishAtUtc, delaySeconds,
            frequency, frequencyDays, dismissible, priority, createdByUserId, createdAtUtc);
        popup._translations.Add(translationResult.Value);

        return Result.Success(popup);
    }

    public Result Update(
        PopupDisplayMode displayMode,
        Guid? imageMediaId,
        LinkTarget linkTarget,
        PopupTargeting targeting,
        PopupDeviceTarget deviceTarget,
        DateTime? publishAtUtc,
        DateTime? unpublishAtUtc,
        int delaySeconds,
        PopupFrequency frequency,
        int? frequencyDays,
        bool dismissible,
        int priority,
        Guid updatedByUserId,
        DateTime updatedAtUtc)
    {
        var modeResult = ValidateModeRules(displayMode, imageMediaId, ref delaySeconds, ref dismissible);
        if (modeResult.IsFailure)
        {
            return modeResult;
        }

        var frequencyResult = ValidateFrequency(frequency, frequencyDays);
        if (frequencyResult.IsFailure)
        {
            return frequencyResult;
        }

        var scheduleResult = ValidateSchedule(publishAtUtc, unpublishAtUtc);
        if (scheduleResult.IsFailure)
        {
            return scheduleResult;
        }

        var priorityResult = ValidatePriority(priority);
        if (priorityResult.IsFailure)
        {
            return priorityResult;
        }

        // Re-validate every existing translation against the NEW display mode (e.g. a Modal-only
        // Title requirement starts applying the moment DisplayMode flips from Banner to Modal).
        foreach (var translation in _translations)
        {
            var revalidation = translation.Update(displayMode, translation.Title, translation.Body, translation.ButtonLabel);
            if (revalidation.IsFailure)
            {
                return revalidation;
            }
        }

        var buttonLinkResult = ValidateButtonRequiresLink(_translations, linkTarget);
        if (buttonLinkResult.IsFailure)
        {
            return buttonLinkResult;
        }

        DisplayMode = displayMode;
        ImageMediaId = imageMediaId;
        LinkTarget = linkTarget;
        Targeting = targeting;
        DeviceTarget = deviceTarget;
        PublishAtUtc = publishAtUtc;
        UnpublishAtUtc = unpublishAtUtc;
        DelaySeconds = delaySeconds;
        Frequency = frequency;
        FrequencyDays = frequencyDays;
        Dismissible = dismissible;
        Priority = priority;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // Upserts the translation for languageCode - one call per language the caller wants to set.
    public Result SetTranslation(
        LanguageCode languageCode, string? title, string? body, string? buttonLabel, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is not null)
        {
            var updateResult = existing.Update(DisplayMode, title, body, buttonLabel);
            if (updateResult.IsFailure)
            {
                return updateResult;
            }

            var linkCheck = ValidateButtonRequiresLink(_translations, LinkTarget);
            if (linkCheck.IsFailure)
            {
                return linkCheck;
            }

            Touch(updatedByUserId, updatedAtUtc);
            return Result.Success();
        }

        var createResult = PopupTranslation.Create(languageCode, DisplayMode, title, body, buttonLabel);
        if (createResult.IsFailure)
        {
            return createResult;
        }

        if (!string.IsNullOrEmpty(createResult.Value.ButtonLabel) && LinkTarget.IsEmpty)
        {
            return Result.Failure(Error.Validation(
                "Popup.ButtonLabelRequiresLink", "A popup with a button label must also have a link target."));
        }

        _translations.Add(createResult.Value);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // The default-language translation can never be removed (Application layer resolves which
    // language is default and passes it in - Popup itself has no SiteLanguage access).
    public Result RemoveTranslation(LanguageCode languageCode, LanguageCode defaultLanguageCode, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (languageCode == defaultLanguageCode)
        {
            return Result.Failure(Error.Conflict(
                "Popup.CannotDeleteDefaultTranslation", "The default language's translation cannot be deleted."));
        }

        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is null)
        {
            return Result.Failure(Error.NotFound("Popup.TranslationNotFound", $"No translation exists for language '{languageCode}'."));
        }

        _translations.Remove(existing);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    public Result Activate(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        IsActive = true;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    public Result Deactivate(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        IsActive = false;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // §6: Banner never has an image or a delay; Modal is always dismissible regardless of what was
    // asked for. delaySeconds/dismissible are normalized in place (ref) rather than returning a
    // tuple, since every caller immediately assigns both back onto itself anyway.
    private static Result ValidateModeRules(PopupDisplayMode displayMode, Guid? imageMediaId, ref int delaySeconds, ref bool dismissible)
    {
        if (displayMode == PopupDisplayMode.Banner)
        {
            if (imageMediaId is not null)
            {
                return Result.Failure(Error.Validation("Popup.BannerCannotHaveImage", "A banner popup cannot have an image."));
            }

            if (delaySeconds != 0)
            {
                return Result.Failure(Error.Validation("Popup.BannerCannotHaveDelay", "A banner popup's delay must be 0."));
            }
        }
        else
        {
            dismissible = true;

            if (delaySeconds < MinDelaySeconds || delaySeconds > MaxDelaySeconds)
            {
                return Result.Failure(Error.Validation(
                    "Popup.DelaySecondsInvalid", $"DelaySeconds must be between {MinDelaySeconds} and {MaxDelaySeconds}."));
            }
        }

        return Result.Success();
    }

    private static Result ValidateFrequency(PopupFrequency frequency, int? frequencyDays)
    {
        if (frequency == PopupFrequency.EveryNDays)
        {
            if (frequencyDays is null || frequencyDays < MinFrequencyDays || frequencyDays > MaxFrequencyDays)
            {
                return Result.Failure(Error.Validation(
                    "Popup.FrequencyDaysRequired",
                    $"FrequencyDays is required and must be between {MinFrequencyDays} and {MaxFrequencyDays} for 'EveryNDays'."));
            }
        }
        else if (frequencyDays is not null)
        {
            return Result.Failure(Error.Validation(
                "Popup.FrequencyDaysNotAllowed", "FrequencyDays can only be set when Frequency is 'EveryNDays'."));
        }

        return Result.Success();
    }

    private static Result ValidateSchedule(DateTime? publishAtUtc, DateTime? unpublishAtUtc)
    {
        if (publishAtUtc is not null && unpublishAtUtc is not null && unpublishAtUtc <= publishAtUtc)
        {
            return Result.Failure(Error.Validation("Popup.UnpublishMustBeAfterPublish", "UnpublishAtUtc must be after PublishAtUtc."));
        }

        return Result.Success();
    }

    private static Result ValidatePriority(int priority)
    {
        if (priority < MinPriority || priority > MaxPriority)
        {
            return Result.Failure(Error.Validation(
                "Popup.PriorityInvalid", $"Priority must be between {MinPriority} and {MaxPriority}."));
        }

        return Result.Success();
    }

    // §6 "ButtonLabel varsa LinkTarget zorunlu" - checked across every translation since ButtonLabel
    // is per-language but LinkTarget belongs to the popup as a whole (mirrors Slide's own rule).
    private static Result ValidateButtonRequiresLink(IReadOnlyList<PopupTranslation> translations, LinkTarget linkTarget)
    {
        if (translations.Any(t => !string.IsNullOrEmpty(t.ButtonLabel)) && linkTarget.IsEmpty)
        {
            return Result.Failure(Error.Validation(
                "Popup.ButtonLabelRequiresLink", "A popup with a button label must also have a link target."));
        }

        return Result.Success();
    }

    private void Touch(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;
        RowVersion = Guid.NewGuid().ToByteArray();
    }
}
