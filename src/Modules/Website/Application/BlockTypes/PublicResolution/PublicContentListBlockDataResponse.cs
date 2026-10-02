namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.PublicResolution;

// Faz 2 Görev 5 master prompt §5.2: content-list's `data` - the resolved items plus the content type's
// own listing page path ("türün liste sayfası yolu"), null when the type has no listing page.
public sealed record PublicContentListBlockDataResponse(IReadOnlyList<PublicContentListBlockItemResponse> Items, string? MoreHref);
