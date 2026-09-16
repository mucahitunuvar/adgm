using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.AdminGetUsers;

public sealed record AdminGetUsersQuery(
    string? Email,
    string? Role,
    string? Status,
    bool? IsLockedOut,
    bool? EmailConfirmed,
    int Page,
    int PageSize) : IRequest<Result<AdminGetUsersResponse>>;
