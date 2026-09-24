namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsTheme;

public sealed record UpdateSiteSettingsThemeRequest(byte[] RowVersion, string? PrimaryColorHex, string? SecondaryColorHex, string? FontFamily);
