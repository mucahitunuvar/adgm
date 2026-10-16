namespace GenclikMerkezi.Modules.Website.Features.GetPublicSite;

// Madde 36/38 (Faz 3 Koşu 2C/3B): the {key, version} pair for a LegalDocument a visitor is being
// asked to accept (newsletter subscription, event registration) - mirrors the policyKey/policyVersion
// pair PublicSiteCookieConsentResponse already carries for the cookie policy, but as its own nullable
// object: the whole field is null (not just its parts) when there is nothing to show.
public sealed record PublicSiteLegalNoticeResponse(string Key, int Version);
