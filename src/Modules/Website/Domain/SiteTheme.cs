using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §8.3/§13: visual identity tokens the frontend maps onto CSS variables so the module can be
// reused with a different brand identity without code changes. Logo/favicon are loose references to
// a MediaAsset (by id, resolved to a URL at read time), the same reasoning as every other
// cross-aggregate reference in this module - never a navigation, since MediaAsset lives in its own
// aggregate boundary.
public sealed partial class SiteTheme : ValueObject
{
    public const int MaxFontFamilyLength = 100;

    public Guid? LogoLightMediaAssetId { get; }

    public Guid? LogoDarkMediaAssetId { get; }

    public Guid? FaviconMediaAssetId { get; }

    public string PrimaryColorHex { get; } = string.Empty;

    public string SecondaryColorHex { get; } = string.Empty;

    public string FontFamily { get; } = string.Empty;

    private SiteTheme(
        Guid? logoLightMediaAssetId,
        Guid? logoDarkMediaAssetId,
        Guid? faviconMediaAssetId,
        string primaryColorHex,
        string secondaryColorHex,
        string fontFamily)
    {
        LogoLightMediaAssetId = logoLightMediaAssetId;
        LogoDarkMediaAssetId = logoDarkMediaAssetId;
        FaviconMediaAssetId = faviconMediaAssetId;
        PrimaryColorHex = primaryColorHex;
        SecondaryColorHex = secondaryColorHex;
        FontFamily = fontFamily;
    }

    public static Result<SiteTheme> Create(
        Guid? logoLightMediaAssetId,
        Guid? logoDarkMediaAssetId,
        Guid? faviconMediaAssetId,
        string? primaryColorHex,
        string? secondaryColorHex,
        string? fontFamily)
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

        return Result.Success(new SiteTheme(
            logoLightMediaAssetId, logoDarkMediaAssetId, faviconMediaAssetId,
            normalizedPrimary, normalizedSecondary, normalizedFontFamily));
    }

    public static SiteTheme CreateEmpty() => new(null, null, null, string.Empty, string.Empty, string.Empty);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return LogoLightMediaAssetId;
        yield return LogoDarkMediaAssetId;
        yield return FaviconMediaAssetId;
        yield return PrimaryColorHex;
        yield return SecondaryColorHex;
        yield return FontFamily;
    }

    [GeneratedRegex(@"^#[0-9A-Fa-f]{6}$")]
    private static partial Regex HexColorPattern();
}
