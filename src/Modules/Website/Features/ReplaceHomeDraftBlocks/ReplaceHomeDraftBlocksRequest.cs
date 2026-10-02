using GenclikMerkezi.Modules.Website.Application.BlockTypes;

namespace GenclikMerkezi.Modules.Website.Features.ReplaceHomeDraftBlocks;

public sealed record ReplaceHomeDraftBlocksRequest(byte[] RowVersion, IReadOnlyList<LayoutBlockInput> Blocks);
