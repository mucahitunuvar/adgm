using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentItemById;

public sealed record GetContentItemByIdQuery(Guid Id) : IRequest<Result<ContentItemDetailResponse>>;
