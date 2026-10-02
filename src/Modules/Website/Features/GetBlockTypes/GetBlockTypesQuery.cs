using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetBlockTypes;

public sealed record GetBlockTypesQuery : IRequest<Result<IReadOnlyList<BlockTypeResponse>>>;
