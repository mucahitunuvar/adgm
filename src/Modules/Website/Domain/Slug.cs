using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §15 (Görev 3): URL-safe identifier Faz 1's ContentItem will carry. Built here, ahead of
// ContentItem itself, as a standalone domain building block with no endpoint of its own yet - see
// the Faz 0 master prompt's Görev 3. The same normalization serves both "generate a slug from a
// title" and "normalize a slug the admin typed by hand" - there is no separate code path for either.
public sealed partial class Slug : ValueObject
{
    public const int MaxLength = 200;

    // Explicit Turkish letter -> unaccented ASCII mapping. 'İ' (U+0130, capital dotted I) needs
    // special handling because .NET's culture-invariant lowercasing of it produces "i" plus a
    // stray combining dot (U+0307), not plain "i" - the classic "Turkish I problem". Everything not
    // in this table falls through to Unicode NFD normalization + stripping combining marks below,
    // which handles ordinary Latin accents (é, ñ, ...).
    private static readonly Dictionary<char, char> TurkishCharacterMap = new()
    {
        ['ç'] = 'c', ['Ç'] = 'c',
        ['ğ'] = 'g', ['Ğ'] = 'g',
        ['ı'] = 'i', ['I'] = 'i',
        ['İ'] = 'i',
        ['ö'] = 'o', ['Ö'] = 'o',
        ['ş'] = 's', ['Ş'] = 's',
        ['ü'] = 'u', ['Ü'] = 'u',
    };

    public string Value { get; }

    private Slug(string value)
    {
        Value = value;
    }

    public static Result<Slug> Create(string? value)
    {
        var normalized = Normalize(value);

        if (normalized.Length == 0)
        {
            return Result.Failure<Slug>(Error.Validation("Slug.Empty", "Slug cannot be empty after normalization."));
        }

        return Result.Success(new Slug(normalized));
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var mapped = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            mapped.Append(TurkishCharacterMap.TryGetValue(ch, out var replacement) ? replacement : ch);
        }

        var decomposed = mapped.ToString().Normalize(NormalizationForm.FormD);
        var withoutMarks = new StringBuilder(decomposed.Length);
        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                withoutMarks.Append(ch);
            }
        }

        var lowered = withoutMarks.ToString().ToLowerInvariant();
        var hyphenated = NonAlphanumericRunPattern().Replace(lowered, "-").Trim('-');

        return hyphenated.Length > MaxLength ? TruncateAtWordBoundary(hyphenated) : hyphenated;
    }

    // Cuts at the nearest hyphen at or before MaxLength rather than mid-word. A single "word" longer
    // than MaxLength (no hyphen to cut at) falls back to a hard cut - better than returning nothing.
    private static string TruncateAtWordBoundary(string value)
    {
        var truncated = value[..MaxLength];
        var lastHyphen = truncated.LastIndexOf('-');

        return lastHyphen > 0 ? truncated[..lastHyphen] : truncated;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumericRunPattern();
}
