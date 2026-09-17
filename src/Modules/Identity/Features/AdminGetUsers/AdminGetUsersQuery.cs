using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.AdminGetUsers;

public sealed record AdminGetUsersQuery(
    string? Email,
    string? Role,
    string? Status,
    bool? IsLockedOut,
    bool? EmailConfirmed) : PagedRequest, IRequest<Result<AdminGetUsersResponse>>;
