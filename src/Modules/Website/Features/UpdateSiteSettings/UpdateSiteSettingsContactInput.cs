namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettings;

public sealed record UpdateSiteSettingsContactInput(
    string? Address,
    string? Phone,
    string? Email,
    string? WhatsApp,
    string? MapEmbedUrl);
