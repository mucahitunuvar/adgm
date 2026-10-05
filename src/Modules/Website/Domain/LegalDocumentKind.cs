namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.1: what kind of legal text this document represents. PrivacyNotice and ExplicitConsent
// are shown as separate checkboxes on a form (never merged into one - ADR-024 §12.1 "Aydınlatma ile
// açık rıza aynı onay kutusunda birleştirilmez"); CookiePolicy backs the cookie consent banner
// (Görev 7); TermsOfUse and Other are for the remaining standalone legal pages (e.g. "terms").
public enum LegalDocumentKind
{
    PrivacyNotice,
    ExplicitConsent,
    CookiePolicy,
    TermsOfUse,
    Other,
}
