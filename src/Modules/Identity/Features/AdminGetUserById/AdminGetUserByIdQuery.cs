using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.AdminGetUserById;

public sealed record AdminGetUserByIdQuery(Guid UserId) : IRequest<Result<AdminUserDetailResponse>>;
