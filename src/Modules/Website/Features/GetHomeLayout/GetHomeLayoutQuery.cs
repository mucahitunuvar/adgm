using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetHomeLayout;

public sealed record GetHomeLayoutQuery : IRequest<Result<GetHomeLayoutResponse>>;
