using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ReplaceHomeDraftBlocks;

public sealed record ReplaceHomeDraftBlocksCommand(byte[] RowVersion, IReadOnlyList<LayoutBlockInput> Blocks) : IRequest<Result>;
