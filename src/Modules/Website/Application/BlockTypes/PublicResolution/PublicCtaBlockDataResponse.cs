namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.PublicResolution;

// Faz 2 Görev 5 master prompt §5.2/§5.2-hiding: cta's `data` - Eyebrow/Title are block-level texts
// (kept here, not in the response's own Texts field, since Buttons needs the same "drop the whole
// item when its link does not resolve" self-containment quick-links/feature-mosaic use).
public sealed record PublicCtaBlockDataResponse(string? Eyebrow, string Title, IReadOnlyList<PublicCtaButtonDataResponse> Buttons);
