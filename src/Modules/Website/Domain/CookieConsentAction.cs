namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §13 (Faz 3 Görev 7): which banner action the visitor took. Custom means the visitor opened
// the preferences panel and picked a subset of categories (neither "accept all" nor "reject all").
public enum CookieConsentAction
{
    AcceptAll,
    RejectAll,
    Custom,
}
