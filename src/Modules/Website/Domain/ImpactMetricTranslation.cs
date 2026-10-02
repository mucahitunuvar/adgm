using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §8.2 (Faz 2 Görev 3). Period/Source are optional here - only the default language's values
// are required, and only once the metric is activated (ImpactMetric.Activate/SetTranslation enforce
// that cross-field, cross-language invariant; this type only validates its own field lengths, the same
// separation VideoTranslation's own remarks describe).
public sealed class ImpactMetricTranslation : Entity
{
    public const int MaxLabelLength = 100;
    public const int MaxUnitLength = 30;
    public const int MaxPeriodLength = 100;
    public const int MaxSourceLength = 200;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string Label { get; private set; } = string.Empty;

    public string? Unit { get; private set; }

    public string? Period { get; private set; }

    public string? Source { get; private set; }

    private ImpactMetricTranslation(Guid id, LanguageCode languageCode, string label, string? unit, string? period, string? source)
        : base(id)
    {
        LanguageCode = languageCode;
        Label = label;
        Unit = unit;
        Period = period;
        Source = source;
    }

    private ImpactMetricTranslation()
    {
    }

    public static Result<ImpactMetricTranslation> Create(LanguageCode languageCode, string? label, string? unit, string? period, string? source)
    {
        var labelResult = NormalizeLabel(label);
        if (labelResult.IsFailure)
        {
            return Result.Failure<ImpactMetricTranslation>(labelResult.Error);
        }

        var unitResult = NormalizeOptional(unit, MaxUnitLength, "ImpactMetricTranslation.UnitInvalid", "Unit");
        if (unitResult.IsFailure)
        {
            return Result.Failure<ImpactMetricTranslation>(unitResult.Error);
        }

        var periodResult = NormalizeOptional(period, MaxPeriodLength, "ImpactMetricTranslation.PeriodInvalid", "Period");
        if (periodResult.IsFailure)
        {
            return Result.Failure<ImpactMetricTranslation>(periodResult.Error);
        }

        var sourceResult = NormalizeOptional(source, MaxSourceLength, "ImpactMetricTranslation.SourceInvalid", "Source");
        if (sourceResult.IsFailure)
        {
            return Result.Failure<ImpactMetricTranslation>(sourceResult.Error);
        }

        return Result.Success(new ImpactMetricTranslation(
            Guid.NewGuid(), languageCode, labelResult.Value, unitResult.Value, periodResult.Value, sourceResult.Value));
    }

    internal Result Update(string? label, string? unit, string? period, string? source)
    {
        var labelResult = NormalizeLabel(label);
        if (labelResult.IsFailure)
        {
            return labelResult;
        }

        var unitResult = NormalizeOptional(unit, MaxUnitLength, "ImpactMetricTranslation.UnitInvalid", "Unit");
        if (unitResult.IsFailure)
        {
            return unitResult;
        }

        var periodResult = NormalizeOptional(period, MaxPeriodLength, "ImpactMetricTranslation.PeriodInvalid", "Period");
        if (periodResult.IsFailure)
        {
            return periodResult;
        }

        var sourceResult = NormalizeOptional(source, MaxSourceLength, "ImpactMetricTranslation.SourceInvalid", "Source");
        if (sourceResult.IsFailure)
        {
            return sourceResult;
        }

        Label = labelResult.Value;
        Unit = unitResult.Value;
        Period = periodResult.Value;
        Source = sourceResult.Value;

        return Result.Success();
    }

    private static Result<string> NormalizeLabel(string? label)
    {
        var normalized = (label ?? string.Empty).Trim();
        if (normalized.Length == 0 || normalized.Length > MaxLabelLength)
        {
            return Result.Failure<string>(Error.Validation(
                "ImpactMetricTranslation.LabelInvalid", $"Label is required and must be at most {MaxLabelLength} characters."));
        }

        return Result.Success(normalized);
    }

    private static Result<string?> NormalizeOptional(string? value, int maxLength, string errorCode, string fieldName)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return Result.Success<string?>(null);
        }

        if (trimmed.Length > maxLength)
        {
            return Result.Failure<string?>(Error.Validation(errorCode, $"{fieldName} must be at most {maxLength} characters."));
        }

        return Result.Success<string?>(trimmed);
    }
}
