namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettings;

public sealed record UpdateSiteSettingsThemeInput(
    Guid? LogoLightMediaAssetId,
    Guid? LogoDarkMediaAssetId,
    Guid? FaviconMediaAssetId,
    string? PrimaryColorHex,
    string? SecondaryColorHex,
    string? FontFamily);
