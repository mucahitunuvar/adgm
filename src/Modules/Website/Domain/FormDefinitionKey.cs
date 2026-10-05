using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.2 (Faz 3 Görev 3): a FormDefinition's system key - lowercase letters, digits and
// hyphens, immutable once created. Mirrors LegalDocumentKey exactly (ContentItem references a form by
// Id, not Key, but the key is still how an admin/operator identifies a form and how Görev 4's public
// GET /api/v1/public/forms/{key} endpoint will look it up).
public sealed partial class FormDefinitionKey : ValueObject
{
    public const int MaxLength = 50;

    public string Value { get; }

    private FormDefinitionKey(string value)
    {
        Value = value;
    }

    public static Result<FormDefinitionKey> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<FormDefinitionKey>(Error.Validation("FormDefinitionKey.Required", "Form key is required."));
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength || !KeyPattern().IsMatch(normalized))
        {
            return Result.Failure<FormDefinitionKey>(Error.Validation(
                "FormDefinitionKey.InvalidFormat", $"Form key must match '[a-z0-9-]' and be at most {MaxLength} characters."));
        }

        return Result.Success(new FormDefinitionKey(normalized));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[a-z0-9-]+$")]
    private static partial Regex KeyPattern();
}
