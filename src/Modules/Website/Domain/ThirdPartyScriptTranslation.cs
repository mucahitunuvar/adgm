using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §13 (Faz 3 Görev 7): the visitor-facing Name and Purpose shown in the cookie preferences
// panel. Mirrors PartnerTranslation's shape: Update is internal (only ThirdPartyScript, the aggregate
// root, calls it). Unlike PartnerTranslation.Description, Purpose is required - a script a visitor is
// asked to consent to must always explain what it does.
public sealed class ThirdPartyScriptTranslation : Entity
{
    public const int MaxNameLength = 150;
    public const int MaxPurposeLength = 300;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public string Purpose { get; private set; } = string.Empty;

    private ThirdPartyScriptTranslation(Guid id, LanguageCode languageCode, string name, string purpose)
        : base(id)
    {
        LanguageCode = languageCode;
        Name = name;
        Purpose = purpose;
    }

    private ThirdPartyScriptTranslation()
    {
    }

    public static Result<ThirdPartyScriptTranslation> Create(LanguageCode languageCode, string? name, string? purpose)
    {
        var nameResult = NormalizeName(name);
        if (nameResult.IsFailure)
        {
            return Result.Failure<ThirdPartyScriptTranslation>(nameResult.Error);
        }

        var purposeResult = NormalizePurpose(purpose);
        if (purposeResult.IsFailure)
        {
            return Result.Failure<ThirdPartyScriptTranslation>(purposeResult.Error);
        }

        return Result.Success(new ThirdPartyScriptTranslation(Guid.NewGuid(), languageCode, nameResult.Value, purposeResult.Value));
    }

    internal Result Update(string? name, string? purpose)
    {
        var nameResult = NormalizeName(name);
        if (nameResult.IsFailure)
        {
            return nameResult;
        }

        var purposeResult = NormalizePurpose(purpose);
        if (purposeResult.IsFailure)
        {
            return purposeResult;
        }

        Name = nameResult.Value;
        Purpose = purposeResult.Value;

        return Result.Success();
    }

    private static Result<string> NormalizeName(string? name)
    {
        var normalized = (name ?? string.Empty).Trim();
        if (normalized.Length == 0 || normalized.Length > MaxNameLength)
        {
            return Result.Failure<string>(Error.Validation(
                "ThirdPartyScriptTranslation.NameInvalid", $"Name is required and must be at most {MaxNameLength} characters."));
        }

        return Result.Success(normalized);
    }

    private static Result<string> NormalizePurpose(string? purpose)
    {
        var normalized = (purpose ?? string.Empty).Trim();
        if (normalized.Length == 0 || normalized.Length > MaxPurposeLength)
        {
            return Result.Failure<string>(Error.Validation(
                "ThirdPartyScriptTranslation.PurposeInvalid", $"Purpose is required and must be at most {MaxPurposeLength} characters."));
        }

        return Result.Success(normalized);
    }
}
