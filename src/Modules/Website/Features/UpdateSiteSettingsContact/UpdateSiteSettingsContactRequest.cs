namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsContact;

public sealed record UpdateSiteSettingsContactRequest(
    byte[] RowVersion,
    string? Address,
    string? Phone,
    string? Email,
    string? WhatsApp,
    string? MapEmbedUrl,
    IReadOnlyList<UpdateSiteSettingsSocialLinkInput> SocialLinks);
