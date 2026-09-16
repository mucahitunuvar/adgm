using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository)
    : IRequestHandler<GetCurrentUserQuery, Result<GetCurrentUserResponse>>
{
    public async Task<Result<GetCurrentUserResponse>> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUserService.UserId is not { } userId)
        {
            return Result.Failure<GetCurrentUserResponse>(
                Error.Unauthorized("Auth.NotAuthenticated", "No authenticated user found."));
        }

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<GetCurrentUserResponse>(
                Error.NotFound("User.NotFound", "The current user could not be found."));
        }

        return Result.Success(new GetCurrentUserResponse(
            user.Id,
            user.Email.Value,
            user.Role.ToString(),
            user.Status.ToString(),
            user.CreatedAtUtc,
            user.EmailConfirmed));
    }
}
