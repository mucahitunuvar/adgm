using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.AdminGetUsers;

public sealed class AdminGetUsersQueryHandler(IUserRepository userRepository)
    : IRequestHandler<AdminGetUsersQuery, Result<AdminGetUsersResponse>>
{
    public async Task<Result<AdminGetUsersResponse>> Handle(AdminGetUsersQuery request, CancellationToken cancellationToken)
    {
        var role = request.Role is null ? (UserRole?)null : Enum.Parse<UserRole>(request.Role, ignoreCase: true);
        var status = request.Status is null ? (UserStatus?)null : Enum.Parse<UserStatus>(request.Status, ignoreCase: true);

        var filter = new UserSearchFilter(
            request.Email,
            role,
            status,
            request.IsLockedOut,
            request.EmailConfirmed,
            request.Page,
            request.PageSize);

        var pagedUsers = await userRepository.SearchAsync(filter, cancellationToken);

        var items = pagedUsers.Items
            .Select(u => new AdminUserListItemResponse(
                u.Id,
                u.Email,
                u.Role.ToString(),
                u.Status.ToString(),
                u.EmailConfirmed,
                u.IsLockedOut,
                u.CreatedAtUtc))
            .ToList();

        return Result.Success(new AdminGetUsersResponse(
            items,
            pagedUsers.TotalCount,
            pagedUsers.Page,
            pagedUsers.PageSize,
            pagedUsers.TotalPages));
    }
}
