namespace GenclikMerkezi.Modules.Website.Features.GetPublicSite;

public sealed record PublicSiteThemeResponse(
    string? LogoLightUrl, string? LogoDarkUrl, string? FaviconUrl, string PrimaryColorHex, string SecondaryColorHex, string FontFamily);
