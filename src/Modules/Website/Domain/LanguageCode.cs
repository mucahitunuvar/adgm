using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §3: BCP-47 style code (tr, en, pt-br), always lowercase-normalized, immutable once
// a SiteLanguage is created (SiteLanguage never exposes a way to change its Code afterwards).
public sealed partial class LanguageCode : ValueObject
{
    public string Value { get; }

    private LanguageCode(string value)
    {
        Value = value;
    }

    public static Result<LanguageCode> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<LanguageCode>(Error.Validation("LanguageCode.Required", "Language code is required."));
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > 35 || !LanguageCodePattern().IsMatch(normalized))
        {
            return Result.Failure<LanguageCode>(Error.Validation(
                "LanguageCode.InvalidFormat",
                "Language code must be a valid BCP-47 code (e.g. 'tr', 'en', 'pt-br')."));
        }

        return Result.Success(new LanguageCode(normalized));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[a-z]{2,3}(-[a-z0-9]{2,8})*$")]
    private static partial Regex LanguageCodePattern();
}
