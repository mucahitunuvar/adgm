using GenclikMerkezi.Modules.Website.Application.BlockTypes.PublicResolution;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicHome;

public sealed record PublicHomeResponse(IReadOnlyList<PublicLayoutBlockResponse> Blocks, PublicHomeSeoResponse Seo);
