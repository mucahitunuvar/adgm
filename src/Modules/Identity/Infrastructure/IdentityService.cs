using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;

namespace GenclikMerkezi.Modules.Identity.Infrastructure;

// The implementation behind IIdentityService (ADR-016 Decision 2, Option C).
public sealed class IdentityService(IUserRepository userRepository) : IIdentityService
{
    public async Task<IdentityUserProfile?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        return user is null
            ? null
            : new IdentityUserProfile(user.Id, user.Email.Value, user.FirstName, user.LastName, user.PhoneNumber);
    }
}
