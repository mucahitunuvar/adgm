using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<Result<GetCurrentUserResponse>>;
