using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4.1 (Faz 1a Görev 2): the system key ("news", "event") - lowercase, hyphen-separated,
// immutable once a ContentType is created (there is no Rename/ChangeKey on ContentType). No leading,
// trailing or doubled hyphen, matching every seed key in the master prompt's table exactly.
public sealed partial class ContentTypeKey : ValueObject
{
    public const int MaxLength = 50;

    public string Value { get; }

    private ContentTypeKey(string value)
    {
        Value = value;
    }

    public static Result<ContentTypeKey> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<ContentTypeKey>(Error.Validation("ContentTypeKey.Required", "Content type key is required."));
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength || !KeyPattern().IsMatch(normalized))
        {
            return Result.Failure<ContentTypeKey>(Error.Validation(
                "ContentTypeKey.InvalidFormat",
                "Content type key must be lowercase letters, digits and single hyphens (e.g. 'news', 'success-story')."));
        }

        return Result.Success(new ContentTypeKey(normalized));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex KeyPattern();
}
