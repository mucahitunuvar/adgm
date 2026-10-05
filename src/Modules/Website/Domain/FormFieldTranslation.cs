using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.2 (Faz 3 Görev 3): a FormField's per-language display text - Label is always required,
// Placeholder/HelpText are optional hints.
public sealed class FormFieldTranslation : Entity
{
    public const int MaxLabelLength = 200;
    public const int MaxPlaceholderLength = 200;
    public const int MaxHelpTextLength = 500;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string Label { get; private set; } = string.Empty;

    public string? Placeholder { get; private set; }

    public string? HelpText { get; private set; }

    private FormFieldTranslation(Guid id, LanguageCode languageCode, string label, string? placeholder, string? helpText)
        : base(id)
    {
        LanguageCode = languageCode;
        Label = label;
        Placeholder = placeholder;
        HelpText = helpText;
    }

    private FormFieldTranslation()
    {
    }

    public static Result<FormFieldTranslation> Create(LanguageCode languageCode, string? label, string? placeholder, string? helpText)
    {
        var normalizedLabel = (label ?? string.Empty).Trim();
        if (normalizedLabel.Length == 0 || normalizedLabel.Length > MaxLabelLength)
        {
            return Result.Failure<FormFieldTranslation>(Error.Validation(
                "FormFieldTranslation.LabelInvalid", $"Label is required and must be at most {MaxLabelLength} characters."));
        }

        var normalizedPlaceholder = NormalizeOptional(placeholder);
        if (normalizedPlaceholder is not null && normalizedPlaceholder.Length > MaxPlaceholderLength)
        {
            return Result.Failure<FormFieldTranslation>(Error.Validation(
                "FormFieldTranslation.PlaceholderTooLong", $"Placeholder must be at most {MaxPlaceholderLength} characters."));
        }

        var normalizedHelpText = NormalizeOptional(helpText);
        if (normalizedHelpText is not null && normalizedHelpText.Length > MaxHelpTextLength)
        {
            return Result.Failure<FormFieldTranslation>(Error.Validation(
                "FormFieldTranslation.HelpTextTooLong", $"Help text must be at most {MaxHelpTextLength} characters."));
        }

        return Result.Success(new FormFieldTranslation(Guid.NewGuid(), languageCode, normalizedLabel, normalizedPlaceholder, normalizedHelpText));
    }

    private static string? NormalizeOptional(string? value)
    {
        if (value is null)
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }
}
