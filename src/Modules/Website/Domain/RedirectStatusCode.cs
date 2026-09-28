namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §15. Every automatic redirect (Faz 1a Görev 4) is MovedPermanently - Found is only ever
// chosen by an admin creating a manual redirect (Görev 5).
public enum RedirectStatusCode
{
    MovedPermanently = 301,
    Found = 302,
}
