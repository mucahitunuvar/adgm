namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §15 (Faz 1a Görev 6): the five outcomes public route resolution can produce, in the order
// they are attempted (Home, then Detail, then Listing, then Redirect, then NotFound).
public enum RouteResolutionKind
{
    Home,
    Detail,
    Listing,
    Redirect,
    NotFound,
}
