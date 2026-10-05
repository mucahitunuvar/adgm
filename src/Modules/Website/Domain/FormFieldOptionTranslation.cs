using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.2 (Faz 3 Görev 3): a Select/MultiSelect FormFieldOption's per-language display label.
public sealed class FormFieldOptionTranslation : Entity
{
    public const int MaxLabelLength = 200;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string Label { get; private set; } = string.Empty;

    private FormFieldOptionTranslation(Guid id, LanguageCode languageCode, string label)
        : base(id)
    {
        LanguageCode = languageCode;
        Label = label;
    }

    private FormFieldOptionTranslation()
    {
    }

    public static Result<FormFieldOptionTranslation> Create(LanguageCode languageCode, string? label)
    {
        var normalized = (label ?? string.Empty).Trim();
        if (normalized.Length == 0 || normalized.Length > MaxLabelLength)
        {
            return Result.Failure<FormFieldOptionTranslation>(Error.Validation(
                "FormFieldOptionTranslation.LabelInvalid", $"Label is required and must be at most {MaxLabelLength} characters."));
        }

        return Result.Success(new FormFieldOptionTranslation(Guid.NewGuid(), languageCode, normalized));
    }
}
