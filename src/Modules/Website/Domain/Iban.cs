using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §13: identifies a bank account for the (currently inactive) "Destek Ol" donation page.
// Validated against the full ISO 13616 mod-97 checksum (Görev 6 fix) - the shape-only regex alone
// accepted any string of the right length/character-class, including simple typos (transposed or
// off-by-one digits) that the checksum exists specifically to catch. No package is needed: the
// algorithm is the standard "move the first 4 characters to the end, map letters to A=10..Z=35,
// interpret as a decimal integer, remainder mod 97 must be 1" - computed digit-by-digit below so no
// number larger than a normal int is ever needed, even for the longest (34-character) IBAN.
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

        if (!HasValidChecksum(normalized))
        {
            return Result.Failure<Iban>(Error.Validation("Iban.InvalidChecksum", "IBAN checksum is invalid."));
        }

        return Result.Success(new Iban(normalized));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    private static bool HasValidChecksum(string normalized)
    {
        var rearranged = normalized[4..] + normalized[..4];
        var remainder = 0;

        foreach (var ch in rearranged)
        {
            var numericValue = char.IsDigit(ch) ? ch - '0' : ch - 'A' + 10;

            // Letters (A=10..Z=35) contribute two digits to the running remainder, digits contribute
            // one - each digit shifts the accumulated remainder one decimal place, same as processing
            // the full rearranged number one character at a time would, without ever materializing a
            // number too large for a normal int/long.
            if (numericValue >= 10)
            {
                remainder = ((remainder * 10) + (numericValue / 10)) % 97;
            }

            remainder = ((remainder * 10) + (numericValue % 10)) % 97;
        }

        return remainder == 1;
    }

    [GeneratedRegex(@"^[A-Z]{2}[0-9]{2}[A-Z0-9]{11,30}$")]
    private static partial Regex IbanPattern();
}
