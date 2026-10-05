using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.2 (Faz 3 Görev 3): a FormDefinition's per-language panel/public text - Title and
// Description are shown above the form, SuccessMessage after a successful submission, SubmitButtonLabel
// on the submit button. Description is sanitized rich text (IHtmlContentSanitizer runs in the handler
// before this ever sees the string, same split as LegalDocumentVersionTranslation.Body) and may be
// empty for a simple form; Title/SuccessMessage/SubmitButtonLabel are short, always-required UI text.
public sealed class FormDefinitionTranslation : Entity
{
    public const int MaxTitleLength = 200;
    public const int MaxSuccessMessageLength = 1000;
    public const int MaxSubmitButtonLabelLength = 100;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string SuccessMessage { get; private set; } = string.Empty;

    public string SubmitButtonLabel { get; private set; } = string.Empty;

    private FormDefinitionTranslation(
        Guid id, LanguageCode languageCode, string title, string description, string successMessage, string submitButtonLabel)
        : base(id)
    {
        LanguageCode = languageCode;
        Title = title;
        Description = description;
        SuccessMessage = successMessage;
        SubmitButtonLabel = submitButtonLabel;
    }

    private FormDefinitionTranslation()
    {
    }

    public static Result<FormDefinitionTranslation> Create(
        LanguageCode languageCode, string? title, string? description, string? successMessage, string? submitButtonLabel)
    {
        var validationResult = Validate(title, description, successMessage, submitButtonLabel);
        if (validationResult.IsFailure)
        {
            return Result.Failure<FormDefinitionTranslation>(validationResult.Error);
        }

        var (normalizedTitle, normalizedDescription, normalizedSuccessMessage, normalizedSubmitButtonLabel) = validationResult.Value;

        return Result.Success(new FormDefinitionTranslation(
            Guid.NewGuid(), languageCode, normalizedTitle, normalizedDescription, normalizedSuccessMessage, normalizedSubmitButtonLabel));
    }

    internal Result Update(string? title, string? description, string? successMessage, string? submitButtonLabel)
    {
        var validationResult = Validate(title, description, successMessage, submitButtonLabel);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        var (normalizedTitle, normalizedDescription, normalizedSuccessMessage, normalizedSubmitButtonLabel) = validationResult.Value;

        Title = normalizedTitle;
        Description = normalizedDescription;
        SuccessMessage = normalizedSuccessMessage;
        SubmitButtonLabel = normalizedSubmitButtonLabel;

        return Result.Success();
    }

    private static Result<(string Title, string Description, string SuccessMessage, string SubmitButtonLabel)> Validate(
        string? title, string? description, string? successMessage, string? submitButtonLabel)
    {
        var normalizedTitle = (title ?? string.Empty).Trim();
        if (normalizedTitle.Length == 0 || normalizedTitle.Length > MaxTitleLength)
        {
            return Result.Failure<(string, string, string, string)>(Error.Validation(
                "FormDefinitionTranslation.TitleInvalid", $"Title is required and must be at most {MaxTitleLength} characters."));
        }

        var normalizedDescription = (description ?? string.Empty).Trim();

        var normalizedSuccessMessage = (successMessage ?? string.Empty).Trim();
        if (normalizedSuccessMessage.Length == 0 || normalizedSuccessMessage.Length > MaxSuccessMessageLength)
        {
            return Result.Failure<(string, string, string, string)>(Error.Validation(
                "FormDefinitionTranslation.SuccessMessageInvalid",
                $"Success message is required and must be at most {MaxSuccessMessageLength} characters."));
        }

        var normalizedSubmitButtonLabel = (submitButtonLabel ?? string.Empty).Trim();
        if (normalizedSubmitButtonLabel.Length == 0 || normalizedSubmitButtonLabel.Length > MaxSubmitButtonLabelLength)
        {
            return Result.Failure<(string, string, string, string)>(Error.Validation(
                "FormDefinitionTranslation.SubmitButtonLabelInvalid",
                $"Submit button label is required and must be at most {MaxSubmitButtonLabelLength} characters."));
        }

        return Result.Success((normalizedTitle, normalizedDescription, normalizedSuccessMessage, normalizedSubmitButtonLabel));
    }
}
