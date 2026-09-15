using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Identity.Domain;

public sealed partial class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Email>(Error.Validation("Email.Required", "Email is required."));
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > 256 || !EmailRegex().IsMatch(normalized))
        {
            return Result.Failure<Email>(Error.Validation("Email.InvalidFormat", "Email format is invalid."));
        }

        return Result.Success(new Email(normalized));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
