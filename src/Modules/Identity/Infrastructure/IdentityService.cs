using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Features.CreateStaffUser;
using GenclikMerkezi.Modules.Identity.Features.RegisterUser;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Identity.Infrastructure;

// The implementation behind IIdentityService (ADR-016 Decision 2, Option C). Depending on
// Features.RegisterUser (an inner layer) from Infrastructure (an outer layer) is the Clean
// Architecture dependency direction, not a violation of it (AGENTS.md §7) - MediatR is already a
// normal Infrastructure-layer dependency in this module (IdentityDbContext.SaveChangesAsync uses
// IPublisher for domain event dispatch the same way).
public sealed class IdentityService(
    IUserRepository userRepository,
    ISender sender,
    [FromKeyedServices(IdentityModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IIdentityService
{
    public async Task<IdentityUserProfile?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        return user is null
            ? null
            : new IdentityUserProfile(user.Id, user.Email.Value, user.FirstName, user.LastName, user.PhoneNumber);
    }

    public async Task<Result<Guid>> CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string? phoneNumber,
        string role,
        CancellationToken cancellationToken = default)
    {
        var command = new RegisterUserCommand(email, password, firstName, lastName, phoneNumber, role);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Result.Success(result.Value.UserId)
            : Result.Failure<Guid>(result.Error);
    }

    public async Task<Result<Guid>> CreateStaffUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string? phoneNumber,
        string role,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateStaffUserCommand(email, password, firstName, lastName, phoneNumber, role);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Result.Success(result.Value.UserId)
            : Result.Failure<Guid>(result.Error);
    }

    public async Task DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return;
        }

        user.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
