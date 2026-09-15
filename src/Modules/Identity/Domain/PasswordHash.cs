using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Identity.Domain;

public sealed class PasswordHash : ValueObject
{
    public string Value { get; }

    private PasswordHash(string value)
    {
        Value = value;
    }

    public static PasswordHash FromHashedValue(string hashedValue)
    {
        if (string.IsNullOrWhiteSpace(hashedValue))
        {
            throw new ArgumentException("Hashed password value cannot be empty.", nameof(hashedValue));
        }

        return new PasswordHash(hashedValue);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
