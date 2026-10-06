namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §13 (Faz 3 Görev 7): the only four shapes a ThirdPartyScript may take - raw HTML/JavaScript
// is never accepted from the panel (AGENTS.md §27/§52: never trust client-/admin-supplied script as
// safe), so every script is one of these typed providers instead.
public enum ThirdPartyScriptProviderKind
{
    GoogleAnalytics4,
    GoogleTagManager,
    MetaPixel,
    ExternalScript,
}
