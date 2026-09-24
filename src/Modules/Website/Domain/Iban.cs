using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §13: identifies a bank account for the (currently inactive) "Destek Ol" donation page.
// Format-only validation (ISO 13616 shape - country code + check digits + BBAN), not a full mod-97
// checksum: enough to reject obvious typos/garbage without adding a dependency for Faz 0.
public sealed partial class Iban : ValueObject
{
    public const int MaxLength = 34;

    public string Value { get; }

    private Iban(string value)
    {
        Value = value;
    }

    public static Result<Iban> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Iban>(Error.Validation("Iban.Required", "IBAN is required."));
        }

        var normalized = value.Replace(" ", string.Empty).ToUpperInvariant();

        if (!IbanPattern().IsMatch(normalized))
        {
            return Result.Failure<Iban>(Error.Validation("Iban.InvalidFormat", "IBAN format is invalid."));
        }

        return Result.Success(new Iban(normalized));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[A-Z]{2}[0-9]{2}[A-Z0-9]{11,30}$")]
    private static partial Regex IbanPattern();
}
