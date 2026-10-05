using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.1: a LegalDocument's system key ("kvkk-contact", "cookie-policy") - lowercase letters,
// digits and hyphens, immutable once created (there is no Rename/ChangeKey, mirroring SliderKey).
// Forms (Görev 3) and the cookie policy setting (Görev 7) reference a document by this Key, not its
// aggregate Id, so the key an admin assigns at creation is permanent.
public sealed partial class LegalDocumentKey : ValueObject
{
    public const int MaxLength = 50;

    public string Value { get; }

    private LegalDocumentKey(string value)
    {
        Value = value;
    }

    public static Result<LegalDocumentKey> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<LegalDocumentKey>(Error.Validation("LegalDocumentKey.Required", "Legal document key is required."));
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength || !KeyPattern().IsMatch(normalized))
        {
            return Result.Failure<LegalDocumentKey>(Error.Validation(
                "LegalDocumentKey.InvalidFormat", $"Legal document key must match '[a-z0-9-]' and be at most {MaxLength} characters."));
        }

        return Result.Success(new LegalDocumentKey(normalized));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[a-z0-9-]+$")]
    private static partial Regex KeyPattern();
}
