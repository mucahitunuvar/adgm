using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §8.2 (Faz 2 Görev 3). Mirrors VideoTranslation's shape: Update is internal (only Partner,
// the aggregate root, calls it), Create/Update share the same normalization so both paths enforce the
// same invariants.
public sealed class PartnerTranslation : Entity
{
    public const int MaxNameLength = 150;
    public const int MaxDescriptionLength = 300;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    private PartnerTranslation(Guid id, LanguageCode languageCode, string name, string? description)
        : base(id)
    {
        LanguageCode = languageCode;
        Name = name;
        Description = description;
    }

    private PartnerTranslation()
    {
    }

    public static Result<PartnerTranslation> Create(LanguageCode languageCode, string? name, string? description)
    {
        var nameResult = NormalizeName(name);
        if (nameResult.IsFailure)
        {
            return Result.Failure<PartnerTranslation>(nameResult.Error);
        }

        var descriptionResult = NormalizeDescription(description);
        if (descriptionResult.IsFailure)
        {
            return Result.Failure<PartnerTranslation>(descriptionResult.Error);
        }

        return Result.Success(new PartnerTranslation(Guid.NewGuid(), languageCode, nameResult.Value, descriptionResult.Value));
    }

    internal Result Update(string? name, string? description)
    {
        var nameResult = NormalizeName(name);
        if (nameResult.IsFailure)
        {
            return nameResult;
        }

        var descriptionResult = NormalizeDescription(description);
        if (descriptionResult.IsFailure)
        {
            return descriptionResult;
        }

        Name = nameResult.Value;
        Description = descriptionResult.Value;

        return Result.Success();
    }

    private static Result<string> NormalizeName(string? name)
    {
        var normalized = (name ?? string.Empty).Trim();
        if (normalized.Length == 0 || normalized.Length > MaxNameLength)
        {
            return Result.Failure<string>(Error.Validation(
                "PartnerTranslation.NameInvalid", $"Name is required and must be at most {MaxNameLength} characters."));
        }

        return Result.Success(normalized);
    }

    private static Result<string?> NormalizeDescription(string? description)
    {
        var trimmed = description?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return Result.Success<string?>(null);
        }

        if (trimmed.Length > MaxDescriptionLength)
        {
            return Result.Failure<string?>(Error.Validation(
                "PartnerTranslation.DescriptionInvalid", $"Description must be at most {MaxDescriptionLength} characters."));
        }

        return Result.Success<string?>(trimmed);
    }
}
