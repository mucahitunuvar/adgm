using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §8.3/§13 (Görev 6): color/font tokens the frontend maps onto CSS variables so the module
// can be reused with a different brand identity without code changes. Logo/favicon/OG-image are a
// separate concept - SiteSettings' "identity" group (UpdateIdentity) - not part of the theme.
public sealed partial class SiteTheme : ValueObject
{
    public const int MaxFontFamilyLength = 100;

    public string PrimaryColorHex { get; } = string.Empty;

    public string SecondaryColorHex { get; } = string.Empty;

    public string FontFamily { get; } = string.Empty;

    private SiteTheme(string primaryColorHex, string secondaryColorHex, string fontFamily)
    {
        PrimaryColorHex = primaryColorHex;
        SecondaryColorHex = secondaryColorHex;
        FontFamily = fontFamily;
    }

    public static Result<SiteTheme> Create(string? primaryColorHex, string? secondaryColorHex, string? fontFamily)
    {
        var normalizedPrimary = (primaryColorHex ?? string.Empty).Trim();
        var normalizedSecondary = (secondaryColorHex ?? string.Empty).Trim();
        var normalizedFontFamily = (fontFamily ?? string.Empty).Trim();

        if (normalizedPrimary.Length > 0 && !HexColorPattern().IsMatch(normalizedPrimary))
        {
            return Result.Failure<SiteTheme>(Error.Validation(
                "SiteTheme.InvalidPrimaryColor", "Primary color must be a 6-digit hex code (e.g. '#1A2B3C')."));
        }

        if (normalizedSecondary.Length > 0 && !HexColorPattern().IsMatch(normalizedSecondary))
        {
            return Result.Failure<SiteTheme>(Error.Validation(
                "SiteTheme.InvalidSecondaryColor", "Secondary color must be a 6-digit hex code (e.g. '#1A2B3C')."));
        }

        if (normalizedFontFamily.Length > MaxFontFamilyLength)
        {
            return Result.Failure<SiteTheme>(Error.Validation(
                "SiteTheme.FontFamilyTooLong", $"Font family must be at most {MaxFontFamilyLength} characters."));
        }

        return Result.Success(new SiteTheme(normalizedPrimary, normalizedSecondary, normalizedFontFamily));
    }

    public static SiteTheme CreateEmpty() => new(string.Empty, string.Empty, string.Empty);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return PrimaryColorHex;
        yield return SecondaryColorHex;
        yield return FontFamily;
    }

    [GeneratedRegex(@"^#[0-9A-Fa-f]{6}$")]
    private static partial Regex HexColorPattern();
}
