using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.AdminGetUserById;

public sealed class AdminGetUserByIdQueryHandler(IUserRepository userRepository)
    : IRequestHandler<AdminGetUserByIdQuery, Result<AdminUserDetailResponse>>
{
    public async Task<Result<AdminUserDetailResponse>> Handle(
        AdminGetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<AdminUserDetailResponse>(
                Error.NotFound("User.NotFound", "The specified user could not be found."));
        }

        return Result.Success(new AdminUserDetailResponse(
            user.Id,
            user.Email.Value,
            user.Role.ToString(),
            user.Status.ToString(),
            user.EmailConfirmed,
            user.IsLockedOut,
            user.LockedUntilUtc,
            user.FailedLoginAttemptCount,
            user.CreatedAtUtc));
    }
}
