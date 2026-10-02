using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentLayout;

public sealed record GetContentLayoutQuery(Guid ContentItemId) : IRequest<Result<GetContentLayoutResponse>>;
