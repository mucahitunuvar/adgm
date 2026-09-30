namespace GenclikMerkezi.Modules.Website.Features.GetPublicSite;

// Faz 2 Görev 1 master prompt §1/§1.3: the frontend builds its own mobile menu from Header - no
// separate Mobile field.
public sealed record PublicMenusResponse(
    IReadOnlyList<PublicMenuItemResponse> Header,
    IReadOnlyList<PublicMenuItemResponse> Utility,
    IReadOnlyList<PublicMenuItemResponse> Footer);
