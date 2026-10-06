using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Contracts.Website;

namespace GenclikMerkezi.Api.Website;

// Host-level adapter (ADR-024 §11.2, Faz 4 Görev 3): implements Website's
// IPlatformUserEmailConfirmationLookup port by forwarding to Identity's public contract - mirrors
// NotificationWebsiteEmailSender's own shape exactly. Website itself never references
// IIdentityService, or any other module - only the Host (the composition root, not itself a
// "module" under ModuleBoundaryTests/WebsiteContractsBoundaryTests) is allowed to know about both
// sides of this wiring.
public sealed class IdentityPlatformUserEmailConfirmationLookup(IIdentityService identityService)
    : IPlatformUserEmailConfirmationLookup
{
    public async Task<bool> IsEmailConfirmedAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await identityService.GetUserProfileAsync(userId, cancellationToken);
        return profile is { EmailConfirmed: true };
    }
}
