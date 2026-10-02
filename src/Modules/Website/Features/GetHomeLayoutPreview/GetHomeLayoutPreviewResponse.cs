using GenclikMerkezi.Modules.Website.Application.BlockTypes.PublicResolution;

namespace GenclikMerkezi.Modules.Website.Features.GetHomeLayoutPreview;

// Faz 2 Görev 5 master prompt §5.1: "public yanıtla aynı biçimde, taslak bloklarla" - same block shape
// as the public home endpoint, built from the draft list instead of the published one.
public sealed record GetHomeLayoutPreviewResponse(IReadOnlyList<PublicLayoutBlockResponse> Blocks);
