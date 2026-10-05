using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.2 (Faz 3 Görev 3): a panel-managed, typed public form - entirely built from the admin
// panel, never seeded. RowVersion is the same application-managed optimistic-concurrency token every
// other Website aggregate uses.
public sealed partial class FormDefinition : AggregateRoot
{
    public const int MinRetentionDays = 30;
    public const int MaxRetentionDays = 3650;
    public const int DefaultRetentionDays = 730;
    public const int MaxNotificationEmails = 10;
    public const int MaxExplicitConsents = 3;
    public const int MaxFields = 30;
    public const int MaxFileFields = 3;

    private readonly List<string> _notificationEmails = [];
    private readonly List<FormExplicitConsentRequirement> _explicitConsents = [];
    private readonly List<FormField> _fields = [];
    private readonly List<FormDefinitionTranslation> _translations = [];

    public FormDefinitionKey Key { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public int RetentionDays { get; private set; } = DefaultRetentionDays;

    public IReadOnlyList<string> NotificationEmails => _notificationEmails.AsReadOnly();

    public LegalDocumentKey PrivacyNoticeKey { get; private set; } = null!;

    public IReadOnlyList<FormExplicitConsentRequirement> ExplicitConsents => _explicitConsents.AsReadOnly();

    public int DefinitionVersion { get; private set; }

    public IReadOnlyList<FormField> Fields => _fields.AsReadOnly();

    public IReadOnlyList<FormDefinitionTranslation> Translations => _translations.AsReadOnly();

    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    private FormDefinition(Guid id, FormDefinitionKey key, Guid createdByUserId, DateTime createdAtUtc)
        : base(id)
    {
        Key = key;
        IsActive = false;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    private FormDefinition()
    {
    }

    public static Result<FormDefinition> Create(
        string? key,
        int retentionDays,
        IReadOnlyList<string> notificationEmails,
        LegalDocumentKey privacyNoticeKey,
        IReadOnlyList<FormExplicitConsentRequirement> explicitConsents,
        LanguageCode defaultLanguageCode,
        string? defaultLanguageTitle,
        string? defaultLanguageDescription,
        string? defaultLanguageSuccessMessage,
        string? defaultLanguageSubmitButtonLabel,
        Guid createdByUserId,
        DateTime createdAtUtc)
    {
        var keyResult = FormDefinitionKey.Create(key);
        if (keyResult.IsFailure)
        {
            return Result.Failure<FormDefinition>(keyResult.Error);
        }

        var coreResult = ValidateCore(retentionDays, notificationEmails, explicitConsents);
        if (coreResult.IsFailure)
        {
            return Result.Failure<FormDefinition>(coreResult.Error);
        }

        var translationResult = FormDefinitionTranslation.Create(
            defaultLanguageCode, defaultLanguageTitle, defaultLanguageDescription, defaultLanguageSuccessMessage,
            defaultLanguageSubmitButtonLabel);
        if (translationResult.IsFailure)
        {
            return Result.Failure<FormDefinition>(translationResult.Error);
        }

        var form = new FormDefinition(Guid.NewGuid(), keyResult.Value, createdByUserId, createdAtUtc)
        {
            RetentionDays = retentionDays,
            PrivacyNoticeKey = privacyNoticeKey,
        };
        form._notificationEmails.AddRange(NormalizeEmails(notificationEmails));
        form._explicitConsents.AddRange(explicitConsents);
        form._translations.Add(translationResult.Value);

        return Result.Success(form);
    }

    // §12.2 "temel alanların güncellenmesi": RetentionDays, NotificationEmails, PrivacyNoticeKey and
    // ExplicitConsents all change together through the same PUT - Key is immutable, Translations and
    // Fields each have their own dedicated mutators, IsActive is Activate/Deactivate's job.
    public Result Update(
        int retentionDays,
        IReadOnlyList<string> notificationEmails,
        LegalDocumentKey privacyNoticeKey,
        IReadOnlyList<FormExplicitConsentRequirement> explicitConsents,
        Guid updatedByUserId,
        DateTime updatedAtUtc)
    {
        var coreResult = ValidateCore(retentionDays, notificationEmails, explicitConsents);
        if (coreResult.IsFailure)
        {
            return coreResult;
        }

        RetentionDays = retentionDays;
        PrivacyNoticeKey = privacyNoticeKey;
        _notificationEmails.Clear();
        _notificationEmails.AddRange(NormalizeEmails(notificationEmails));
        _explicitConsents.Clear();
        _explicitConsents.AddRange(explicitConsents);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // Upserts the translation for languageCode - one call per language the caller wants to set.
    public Result SetTranslation(
        LanguageCode languageCode, string? title, string? description, string? successMessage, string? submitButtonLabel,
        Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is not null)
        {
            var updateResult = existing.Update(title, description, successMessage, submitButtonLabel);
            if (updateResult.IsFailure)
            {
                return updateResult;
            }

            Touch(updatedByUserId, updatedAtUtc);
            return Result.Success();
        }

        var createResult = FormDefinitionTranslation.Create(languageCode, title, description, successMessage, submitButtonLabel);
        if (createResult.IsFailure)
        {
            return createResult;
        }

        _translations.Add(createResult.Value);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    public Result RemoveTranslation(LanguageCode languageCode, LanguageCode defaultLanguageCode, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (languageCode == defaultLanguageCode)
        {
            return Result.Failure(Error.Conflict(
                "FormDefinition.CannotDeleteDefaultTranslation", "The default language's translation cannot be deleted."));
        }

        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is null)
        {
            return Result.Failure(Error.NotFound("FormDefinition.TranslationNotFound", $"No translation exists for language '{languageCode}'."));
        }

        _translations.Remove(existing);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // Whole-list replace (PUT .../fields), same shape as ContentItem.SetGallery - each field change
    // (add/remove/reorder/reconfigure) bumps DefinitionVersion so a FormSubmission (Görev 4) can snapshot
    // which field definitions it was answered against ("gönderim anında alanların o anki tanımı
    // başvuruya kopyalanır").
    public Result SetFields(IReadOnlyList<FormField> fields, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (fields.Count > MaxFields)
        {
            return Result.Failure(Error.Validation("FormDefinition.TooManyFields", $"At most {MaxFields} fields can be configured."));
        }

        if (fields.GroupBy(f => f.Key).Any(g => g.Count() > 1))
        {
            return Result.Failure(Error.Validation("FormDefinition.DuplicateFieldKey", "Field keys must be unique within a form."));
        }

        if (fields.Count(f => f.Type == FormFieldType.File) > MaxFileFields)
        {
            return Result.Failure(Error.Validation(
                "FormDefinition.TooManyFileFields", $"At most {MaxFileFields} File fields can be configured."));
        }

        _fields.Clear();
        _fields.AddRange(fields);
        DefinitionVersion++;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // §12.2 "Form ancak bağlı aydınlatma metninin (ve zorunlu açık rızaların) yürürlükte bir sürümü
    // varsa aktif edilebilir": legalRequirementsSatisfied is resolved by the caller (the Application-
    // layer command handler, which alone has LegalDocument repository access) - FormDefinition itself
    // only enforces that activation is refused without it.
    public Result Activate(bool legalRequirementsSatisfied, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (!legalRequirementsSatisfied)
        {
            return Result.Failure(Error.Conflict(
                "FormDefinition.LegalRequirementsNotMet",
                "The form's privacy notice and required explicit consents must each have an effective version before activation."));
        }

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

    private static Result ValidateCore(
        int retentionDays, IReadOnlyList<string> notificationEmails, IReadOnlyList<FormExplicitConsentRequirement> explicitConsents)
    {
        if (retentionDays < MinRetentionDays || retentionDays > MaxRetentionDays)
        {
            return Result.Failure(Error.Validation(
                "FormDefinition.RetentionDaysInvalid", $"Retention days must be between {MinRetentionDays} and {MaxRetentionDays}."));
        }

        if (notificationEmails.Count > MaxNotificationEmails)
        {
            return Result.Failure(Error.Validation(
                "FormDefinition.TooManyNotificationEmails", $"At most {MaxNotificationEmails} notification emails are allowed."));
        }

        if (notificationEmails.Any(e => string.IsNullOrWhiteSpace(e) || !EmailPattern().IsMatch(e.Trim())))
        {
            return Result.Failure(Error.Validation("FormDefinition.InvalidNotificationEmail", "One or more notification emails are invalid."));
        }

        if (explicitConsents.Count > MaxExplicitConsents)
        {
            return Result.Failure(Error.Validation(
                "FormDefinition.TooManyExplicitConsents", $"At most {MaxExplicitConsents} explicit consents are allowed."));
        }

        if (explicitConsents.GroupBy(c => c.LegalDocumentKey).Any(g => g.Count() > 1))
        {
            return Result.Failure(Error.Validation(
                "FormDefinition.DuplicateExplicitConsent", "The same legal document cannot be required as explicit consent twice."));
        }

        return Result.Success();
    }

    private static IEnumerable<string> NormalizeEmails(IReadOnlyList<string> emails) => emails.Select(e => e.Trim());

    private void Touch(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;
        RowVersion = Guid.NewGuid().ToByteArray();
    }

    // Mirrors ContactInfo's own email format check.
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();
}
