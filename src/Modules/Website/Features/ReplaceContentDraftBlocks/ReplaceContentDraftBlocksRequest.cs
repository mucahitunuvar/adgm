using GenclikMerkezi.Modules.Website.Application.BlockTypes;

namespace GenclikMerkezi.Modules.Website.Features.ReplaceContentDraftBlocks;

public sealed record ReplaceContentDraftBlocksRequest(byte[] RowVersion, IReadOnlyList<LayoutBlockInput> Blocks);
