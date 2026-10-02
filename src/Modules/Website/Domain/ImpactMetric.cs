using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §8.2 (Faz 2 Görev 3): "doğrulanmamış rakam yayınlanmaz" - a newly created metric always
// starts inactive (mirrors ContentItem's own draft-then-publish workflow, not Video/Partner's
// "active by default"), and Activate requires the default language's Period and Source to be filled.
// Once active, those two default-language fields can never be emptied again (SetTranslation enforces
// this), so a public metric can never silently lose its verification trail.
public sealed partial class ImpactMetric : AggregateRoot
{
    public const int MaxIconKeyLength = 50;
    public const int ValueDecimalPlaces = 2;

    private readonly List<ImpactMetricTranslation> _translations = [];

    public decimal Value { get; private set; }

    public string? IconKey { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyList<ImpactMetricTranslation> Translations => _translations.AsReadOnly();

    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    private ImpactMetric(Guid id, decimal value, string? iconKey, int sortOrder, Guid createdByUserId, DateTime createdAtUtc)
        : base(id)
    {
        Value = value;
        IconKey = iconKey;
        SortOrder = sortOrder;
        IsActive = false;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    private ImpactMetric()
    {
    }

    public static Result<ImpactMetric> Create(
        decimal value,
        string? iconKey,
        int sortOrder,
        LanguageCode defaultLanguageCode,
        string? defaultLanguageLabel,
        string? defaultLanguageUnit,
        string? defaultLanguagePeriod,
        string? defaultLanguageSource,
        Guid createdByUserId,
        DateTime createdAtUtc)
    {
        var valueResult = NormalizeValue(value);
        if (valueResult.IsFailure)
        {
            return Result.Failure<ImpactMetric>(valueResult.Error);
        }

        var iconKeyResult = NormalizeIconKey(iconKey);
        if (iconKeyResult.IsFailure)
        {
            return Result.Failure<ImpactMetric>(iconKeyResult.Error);
        }

        var translationResult = ImpactMetricTranslation.Create(
            defaultLanguageCode, defaultLanguageLabel, defaultLanguageUnit, defaultLanguagePeriod, defaultLanguageSource);
        if (translationResult.IsFailure)
        {
            return Result.Failure<ImpactMetric>(translationResult.Error);
        }

        var metric = new ImpactMetric(Guid.NewGuid(), valueResult.Value, iconKeyResult.Value, sortOrder, createdByUserId, createdAtUtc);
        metric._translations.Add(translationResult.Value);

        return Result.Success(metric);
    }

    public Result Update(decimal value, string? iconKey, int sortOrder, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var valueResult = NormalizeValue(value);
        if (valueResult.IsFailure)
        {
            return valueResult;
        }

        var iconKeyResult = NormalizeIconKey(iconKey);
        if (iconKeyResult.IsFailure)
        {
            return iconKeyResult;
        }

        Value = valueResult.Value;
        IconKey = iconKeyResult.Value;
        SortOrder = sortOrder;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // Upserts the translation for languageCode - one call per language the caller wants to set.
    // While the metric is active, the default language's Period/Source can never be cleared again
    // (ADR-024 §8.2: "Aktifken bu alanlar boşaltılamaz").
    public Result SetTranslation(
        LanguageCode languageCode,
        LanguageCode defaultLanguageCode,
        string? label,
        string? unit,
        string? period,
        string? source,
        Guid updatedByUserId,
        DateTime updatedAtUtc)
    {
        if (IsActive && languageCode == defaultLanguageCode
            && (string.IsNullOrWhiteSpace(period) || string.IsNullOrWhiteSpace(source)))
        {
            return Result.Failure(Error.Conflict(
                "ImpactMetric.CannotClearPeriodOrSourceWhileActive",
                "Period and Source cannot be cleared for the default language while the metric is active."));
        }

        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is not null)
        {
            var updateResult = existing.Update(label, unit, period, source);
            if (updateResult.IsFailure)
            {
                return updateResult;
            }

            Touch(updatedByUserId, updatedAtUtc);
            return Result.Success();
        }

        var createResult = ImpactMetricTranslation.Create(languageCode, label, unit, period, source);
        if (createResult.IsFailure)
        {
            return createResult;
        }

        _translations.Add(createResult.Value);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // The default-language translation can never be removed (Application layer resolves which
    // language is default and passes it in - mirrors Video.RemoveTranslation).
    public Result RemoveTranslation(LanguageCode languageCode, LanguageCode defaultLanguageCode, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (languageCode == defaultLanguageCode)
        {
            return Result.Failure(Error.Conflict(
                "ImpactMetric.CannotDeleteDefaultTranslation", "The default language's translation cannot be deleted."));
        }

        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is null)
        {
            return Result.Failure(Error.NotFound("ImpactMetric.TranslationNotFound", $"No translation exists for language '{languageCode}'."));
        }

        _translations.Remove(existing);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // ADR-024 §8.2: "Bir gösterge aktif edilebilmesi için varsayılan dilde Period ve Source dolu
    // olmalıdır (doğrulanmamış rakam yayınlanmaz)".
    public Result Activate(LanguageCode defaultLanguageCode, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var defaultTranslation = _translations.FirstOrDefault(t => t.LanguageCode == defaultLanguageCode);
        if (defaultTranslation is null || string.IsNullOrWhiteSpace(defaultTranslation.Period) || string.IsNullOrWhiteSpace(defaultTranslation.Source))
        {
            return Result.Failure(Error.Validation(
                "ImpactMetric.ActivationRequiresPeriodAndSource",
                "The default language's Period and Source must be filled before this metric can be activated."));
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

    private static Result<decimal> NormalizeValue(decimal value)
    {
        if (value < 0 || decimal.Round(value, ValueDecimalPlaces, MidpointRounding.AwayFromZero) != value)
        {
            return Result.Failure<decimal>(Error.Validation(
                "ImpactMetric.ValueInvalid", $"Value must be zero or greater, with at most {ValueDecimalPlaces} decimal places."));
        }

        return Result.Success(value);
    }

    private static Result<string?> NormalizeIconKey(string? iconKey)
    {
        var normalized = string.IsNullOrWhiteSpace(iconKey) ? null : iconKey.Trim();
        if (normalized is not null && (normalized.Length > MaxIconKeyLength || !IconKeyPattern().IsMatch(normalized)))
        {
            return Result.Failure<string?>(Error.Validation(
                "ImpactMetric.IconKeyInvalid", $"Icon key must match '[a-z0-9-]' and be at most {MaxIconKeyLength} characters."));
        }

        return Result.Success(normalized);
    }

    private void Touch(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;
        RowVersion = Guid.NewGuid().ToByteArray();
    }

    [GeneratedRegex("^[a-z0-9-]+$")]
    private static partial Regex IconKeyPattern();
}
