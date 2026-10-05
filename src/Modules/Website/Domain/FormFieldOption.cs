using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.2 (Faz 3 Görev 3): one choice of a Select/MultiSelect FormField - Key is the value
// actually stored on a submitted FormSubmission's answer (Görev 4), Translations carry the per-language
// display label. List order (declared assignment order, mirrors FormField.Options on the owning
// FormField) is the display order - there is no separate SortOrder.
public sealed partial class FormFieldOption : Entity
{
    public const int MaxKeyLength = 50;

    private readonly List<FormFieldOptionTranslation> _translations = [];

    public string Key { get; private set; } = string.Empty;

    public IReadOnlyList<FormFieldOptionTranslation> Translations => _translations.AsReadOnly();

    private FormFieldOption(Guid id, string key)
        : base(id)
    {
        Key = key;
    }

    private FormFieldOption()
    {
    }

    public static Result<FormFieldOption> Create(string? key, IReadOnlyList<FormFieldOptionTranslation> translations)
    {
        var keyResult = NormalizeKey(key);
        if (keyResult.IsFailure)
        {
            return Result.Failure<FormFieldOption>(keyResult.Error);
        }

        var option = new FormFieldOption(Guid.NewGuid(), keyResult.Value);
        option._translations.AddRange(translations);

        return Result.Success(option);
    }

    private static Result<string> NormalizeKey(string? key)
    {
        var normalized = (key ?? string.Empty).Trim().ToLowerInvariant();

        if (normalized.Length == 0 || normalized.Length > MaxKeyLength || !KeyPattern().IsMatch(normalized))
        {
            return Result.Failure<string>(Error.Validation(
                "FormFieldOption.KeyInvalid", $"Option key must match '[a-z0-9_]' and be at most {MaxKeyLength} characters."));
        }

        return Result.Success(normalized);
    }

    [GeneratedRegex("^[a-z0-9_]+$")]
    private static partial Regex KeyPattern();
}
