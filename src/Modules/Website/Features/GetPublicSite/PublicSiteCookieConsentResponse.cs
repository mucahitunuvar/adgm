namespace GenclikMerkezi.Modules.Website.Features.GetPublicSite;

// ADR-024 §13 (Faz 3 Görev 7): PolicyVersion is null when CookiePolicyKey is not yet configured, or the
// configured document has no effective version - the frontend then knows it cannot yet post a
// CreateCookieConsentRecord (that endpoint 404s - "CookieConsent.NotAvailable" - until one exists).
// "Politika sürümü değişince frontend yeniden onay ister" (§13/ARCHITECTURE.md §8.11): the frontend
// compares PolicyVersion against whatever it has stored from the visitor's last consent.
public sealed record PublicSiteCookieConsentResponse(
    string BannerTitle,
    string BannerText,
    IReadOnlyList<PublicSiteCookieCategoryResponse> Categories,
    string? PolicyKey,
    int? PolicyVersion);
