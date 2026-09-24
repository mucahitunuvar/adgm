namespace GenclikMerkezi.Modules.Website.Features.GetPublicSiteSettings;

public sealed record PublicSiteThemeResponse(
    string? LogoLightUrl, string? LogoDarkUrl, string? FaviconUrl, string PrimaryColorHex, string SecondaryColorHex, string FontFamily);
