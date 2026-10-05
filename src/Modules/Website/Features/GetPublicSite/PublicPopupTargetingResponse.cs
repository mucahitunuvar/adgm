namespace GenclikMerkezi.Modules.Website.Features.GetPublicSite;

// Faz 2 Görev 6 master prompt §6: Kind is the popup's original authored targeting kind (AllPages,
// HomeOnly, Contents or Paths); Paths is empty for AllPages/HomeOnly, the admin-authored paths as-is
// for Paths, and Contents' ids already resolved to this language's paths (invisible ones dropped) for
// Contents - so the frontend only ever needs to path-match against one flat list, whichever kind
// produced it.
public sealed record PublicPopupTargetingResponse(string Kind, IReadOnlyList<string> Paths);
