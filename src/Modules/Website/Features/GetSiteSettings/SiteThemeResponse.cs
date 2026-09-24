namespace GenclikMerkezi.Modules.Website.Features.GetSiteSettings;

public sealed record SiteThemeResponse(
    Guid? LogoLightMediaAssetId,
    string? LogoLightUrl,
    Guid? LogoDarkMediaAssetId,
    string? LogoDarkUrl,
    Guid? FaviconMediaAssetId,
    string? FaviconUrl,
    string PrimaryColorHex,
    string SecondaryColorHex,
    string FontFamily);
