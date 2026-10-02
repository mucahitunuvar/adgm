using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// Faz 2 Görev 2 master prompt §2: a Slider's system key ("home-hero") - lowercase letters, digits and
// hyphens, immutable once created (there is no Rename/ChangeKey on Slider, mirroring ContentTypeKey).
// A future hero-slider block (Görev 4) references a Slider by its aggregate Id, not this Key - Key only
// exists so an admin can recognize which slider is which without opening it.
public sealed partial class SliderKey : ValueObject
{
    public const int MaxLength = 50;

    public string Value { get; }

    private SliderKey(string value)
    {
        Value = value;
    }

    public static Result<SliderKey> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<SliderKey>(Error.Validation("SliderKey.Required", "Slider key is required."));
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength || !KeyPattern().IsMatch(normalized))
        {
            return Result.Failure<SliderKey>(Error.Validation(
                "SliderKey.InvalidFormat", $"Slider key must match '[a-z0-9-]' and be at most {MaxLength} characters."));
        }

        return Result.Success(new SliderKey(normalized));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[a-z0-9-]+$")]
    private static partial Regex KeyPattern();
}
