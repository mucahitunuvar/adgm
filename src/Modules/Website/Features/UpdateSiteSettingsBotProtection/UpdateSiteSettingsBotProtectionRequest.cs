namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsBotProtection;

public sealed record UpdateSiteSettingsBotProtectionRequest(byte[] RowVersion, bool BotProtectionEnabled, string? TurnstileSiteKey);
