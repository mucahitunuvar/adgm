namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsCookieConsent;

public sealed record UpdateSiteSettingsCookieConsentRequest(
    byte[] RowVersion,
    string? CookiePolicyKey,
    IReadOnlyList<UpdateSiteSettingsCookieConsentTranslationInput> Translations);
