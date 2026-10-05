using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.2 (Faz 3 Görev 3): one typed input of a FormDefinition. Which of MinLength/MaxLength,
// Options, DateMin/DateMax, AllowedFileTypes/MaxSizeMb apply depends on Type - exactly one group may be
// set, enforced here so a FormField can never carry constraints that do not apply to its own Type (same
// "not supported for this configuration" shape as ContentItem.CheckDetailImageAllowed). Whole-form
// invariants (at most 30 fields, unique Key within the form, at most 3 File fields) are
// FormDefinition.SetFields' job - a single field cannot check them.
public sealed partial class FormField : Entity
{
    public const int MaxKeyLength = 50;
    public const int MaxTextLength = 4000;
    public const int MinOptionCount = 2;
    public const int MaxOptionCount = 50;
    public const int MinFileSizeMb = 1;
    public const int MaxFileSizeMb = 10;

    private readonly List<FormFieldOption> _options = [];
    private readonly List<FormFieldAllowedFileType> _allowedFileTypes = [];
    private readonly List<FormFieldTranslation> _translations = [];

    public string Key { get; private set; } = string.Empty;

    public FormFieldType Type { get; private set; }

    public bool IsRequired { get; private set; }

    public int SortOrder { get; private set; }

    public int? MinLength { get; private set; }

    public int? MaxLength { get; private set; }

    public IReadOnlyList<FormFieldOption> Options => _options.AsReadOnly();

    public DateTime? DateMin { get; private set; }

    public DateTime? DateMax { get; private set; }

    public IReadOnlyList<FormFieldAllowedFileType> AllowedFileTypes => _allowedFileTypes.AsReadOnly();

    public int? MaxSizeMb { get; private set; }

    public IReadOnlyList<FormFieldTranslation> Translations => _translations.AsReadOnly();

    private FormField(
        Guid id, string key, FormFieldType type, bool isRequired, int sortOrder, int? minLength, int? maxLength, DateTime? dateMin,
        DateTime? dateMax, int? maxSizeMb)
        : base(id)
    {
        Key = key;
        Type = type;
        IsRequired = isRequired;
        SortOrder = sortOrder;
        MinLength = minLength;
        MaxLength = maxLength;
        DateMin = dateMin;
        DateMax = dateMax;
        MaxSizeMb = maxSizeMb;
    }

    private FormField()
    {
    }

    public static Result<FormField> Create(
        string? key,
        FormFieldType type,
        bool isRequired,
        int sortOrder,
        int? minLength,
        int? maxLength,
        IReadOnlyList<FormFieldOption> options,
        DateTime? dateMin,
        DateTime? dateMax,
        IReadOnlyList<FormFieldAllowedFileType> allowedFileTypes,
        int? maxSizeMb,
        IReadOnlyList<FormFieldTranslation> translations)
    {
        var keyResult = NormalizeKey(key);
        if (keyResult.IsFailure)
        {
            return Result.Failure<FormField>(keyResult.Error);
        }

        var textLengthResult = ValidateTextLength(type, minLength, maxLength);
        if (textLengthResult.IsFailure)
        {
            return Result.Failure<FormField>(textLengthResult.Error);
        }

        var optionsResult = ValidateOptions(type, options);
        if (optionsResult.IsFailure)
        {
            return Result.Failure<FormField>(optionsResult.Error);
        }

        var dateRangeResult = ValidateDateRange(type, dateMin, dateMax);
        if (dateRangeResult.IsFailure)
        {
            return Result.Failure<FormField>(dateRangeResult.Error);
        }

        var fileResult = ValidateFileConstraints(type, allowedFileTypes, maxSizeMb);
        if (fileResult.IsFailure)
        {
            return Result.Failure<FormField>(fileResult.Error);
        }

        var field = new FormField(Guid.NewGuid(), keyResult.Value, type, isRequired, sortOrder, minLength, maxLength, dateMin, dateMax, maxSizeMb);
        field._options.AddRange(options);
        field._allowedFileTypes.AddRange(allowedFileTypes.Distinct());
        field._translations.AddRange(translations);

        return Result.Success(field);
    }

    private static Result<string> NormalizeKey(string? key)
    {
        var normalized = (key ?? string.Empty).Trim().ToLowerInvariant();

        if (normalized.Length == 0 || normalized.Length > MaxKeyLength || !KeyPattern().IsMatch(normalized))
        {
            return Result.Failure<string>(Error.Validation(
                "FormField.KeyInvalid", $"Field key must match '[a-z0-9_]' and be at most {MaxKeyLength} characters."));
        }

        return Result.Success(normalized);
    }

    private static Result ValidateTextLength(FormFieldType type, int? minLength, int? maxLength)
    {
        if (type is not (FormFieldType.Text or FormFieldType.Textarea))
        {
            return minLength is not null || maxLength is not null
                ? Result.Failure(Error.Validation(
                    "FormField.LengthNotSupported", "MinLength/MaxLength only apply to Text and Textarea fields."))
                : Result.Success();
        }

        if (minLength is < 0 || maxLength is < 0 || maxLength > MaxTextLength)
        {
            return Result.Failure(Error.Validation(
                "FormField.LengthInvalid", $"MinLength/MaxLength must be between 0 and {MaxTextLength}."));
        }

        if (minLength is not null && maxLength is not null && minLength > maxLength)
        {
            return Result.Failure(Error.Validation("FormField.MinLengthGreaterThanMaxLength", "MinLength cannot exceed MaxLength."));
        }

        return Result.Success();
    }

    private static Result ValidateOptions(FormFieldType type, IReadOnlyList<FormFieldOption> options)
    {
        if (type is not (FormFieldType.Select or FormFieldType.MultiSelect))
        {
            return options.Count > 0
                ? Result.Failure(Error.Validation("FormField.OptionsNotSupported", "Options only apply to Select and MultiSelect fields."))
                : Result.Success();
        }

        if (options.Count is < MinOptionCount or > MaxOptionCount)
        {
            return Result.Failure(Error.Validation(
                "FormField.OptionCountInvalid", $"Select/MultiSelect fields must have between {MinOptionCount} and {MaxOptionCount} options."));
        }

        if (options.GroupBy(o => o.Key).Any(g => g.Count() > 1))
        {
            return Result.Failure(Error.Validation("FormField.DuplicateOptionKey", "Option keys must be unique within a field."));
        }

        return Result.Success();
    }

    private static Result ValidateDateRange(FormFieldType type, DateTime? dateMin, DateTime? dateMax)
    {
        if (type != FormFieldType.Date)
        {
            return dateMin is not null || dateMax is not null
                ? Result.Failure(Error.Validation("FormField.DateRangeNotSupported", "DateMin/DateMax only apply to Date fields."))
                : Result.Success();
        }

        if (dateMin is not null && dateMax is not null && dateMin > dateMax)
        {
            return Result.Failure(Error.Validation("FormField.DateMinGreaterThanDateMax", "DateMin cannot be after DateMax."));
        }

        return Result.Success();
    }

    private static Result ValidateFileConstraints(FormFieldType type, IReadOnlyList<FormFieldAllowedFileType> allowedFileTypes, int? maxSizeMb)
    {
        if (type != FormFieldType.File)
        {
            return allowedFileTypes.Count > 0 || maxSizeMb is not null
                ? Result.Failure(Error.Validation(
                    "FormField.FileConstraintsNotSupported", "AllowedFileTypes/MaxSizeMb only apply to File fields."))
                : Result.Success();
        }

        if (allowedFileTypes.Count == 0)
        {
            return Result.Failure(Error.Validation(
                "FormField.AllowedFileTypesRequired", "At least one allowed file type is required for a File field."));
        }

        if (maxSizeMb is null || maxSizeMb < MinFileSizeMb || maxSizeMb > MaxFileSizeMb)
        {
            return Result.Failure(Error.Validation(
                "FormField.MaxSizeMbInvalid", $"MaxSizeMb is required and must be between {MinFileSizeMb} and {MaxFileSizeMb}."));
        }

        return Result.Success();
    }

    [GeneratedRegex("^[a-z0-9_]+$")]
    private static partial Regex KeyPattern();
}
