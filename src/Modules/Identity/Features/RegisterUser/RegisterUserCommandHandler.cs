using GenclikMerkezi.Contracts.IntegrationEvents;
using GenclikMerkezi.Modules.Identity;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Identity.Features.RegisterUser;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IEmailVerificationTokenGenerator verificationTokenGenerator,
    IIntegrationEventPublisher integrationEventPublisher,
    [FromKeyedServices(IdentityModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    public async Task<Result<RegisterUserResponse>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);

        if (emailResult.IsFailure)
        {
            return Result.Failure<RegisterUserResponse>(emailResult.Error);
        }

        var email = emailResult.Value;

        var existingUser = await userRepository.GetByEmailAsync(email, cancellationToken);

        if (existingUser is not null)
        {
            return Result.Failure<RegisterUserResponse>(
                Error.Conflict("User.EmailAlreadyExists", "A user with this email already exists."));
        }

        var role = Enum.Parse<UserRole>(request.Role, ignoreCase: true);
        var passwordHash = PasswordHash.FromHashedValue(passwordHasher.Hash(request.Password));

        var user = User.Register(email, passwordHash, role);
        userRepository.Add(user);

        var plainVerificationToken = verificationTokenGenerator.GenerateToken();
        var verificationTokenHash = verificationTokenGenerator.Hash(plainVerificationToken);
        var verificationExpiresAtUtc = DateTime.UtcNow.Add(verificationTokenGenerator.Lifetime);
        user.IssueEmailVerificationToken(verificationTokenHash, verificationExpiresAtUtc);

        var integrationEvent = new UserRegisteredIntegrationEvent(
            user.Id,
            user.Email.Value,
            plainVerificationToken,
            verificationExpiresAtUtc,
            DateTime.UtcNow);

        await integrationEventPublisher.PublishTransactionalAsync(
            IntegrationEventTopics.UserRegistered,
            integrationEvent,
            () => unitOfWork.SaveChangesAsync(cancellationToken),
            cancellationToken);

        return Result.Success(new RegisterUserResponse(user.Id, user.Email.Value, user.Role.ToString()));
    }
}
