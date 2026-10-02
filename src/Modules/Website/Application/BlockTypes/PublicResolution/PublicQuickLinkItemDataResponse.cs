namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.PublicResolution;

// Faz 2 Görev 5 master prompt §5.2/§5.2-hiding: quick-links' per-item `data` - settings' IconKey,
// texts' Label/Description and the resolved Href merged into one self-contained item, since an item
// whose link does not resolve is dropped entirely ("öğe çıkarılır"), not just its href.
public sealed record PublicQuickLinkItemDataResponse(string IconKey, string Label, string? Description, string Href);
