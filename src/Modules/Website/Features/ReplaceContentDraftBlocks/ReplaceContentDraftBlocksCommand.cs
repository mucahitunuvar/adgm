using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ReplaceContentDraftBlocks;

public sealed record ReplaceContentDraftBlocksCommand(Guid ContentItemId, byte[] RowVersion, IReadOnlyList<LayoutBlockInput> Blocks)
    : IRequest<Result>;
