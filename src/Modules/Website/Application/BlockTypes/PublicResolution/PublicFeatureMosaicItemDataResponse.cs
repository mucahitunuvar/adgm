namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.PublicResolution;

// Faz 2 Görev 5 master prompt §5.2/§5.2-hiding: feature-mosaic's per-item `data`, self-contained like
// quick-links' for the same "item removed, not just its href, when its link does not resolve" reason.
public sealed record PublicFeatureMosaicItemDataResponse(string? Eyebrow, string Title, PublicBlockImageResponse? Image, string Href);
