namespace GenclikMerkezi.Contracts.IntegrationEvents;

// Fired when an already-registered, not-yet-verified user asks for a new verification link - kept
// separate from UserRegisteredIntegrationEvent so that event keeps meaning "a user registered".
// Carries the plaintext verification token for the same reason UserRegisteredIntegrationEvent does:
// only ever hashed at rest (Identity's EmailVerificationToken.TokenHash), so this is the one point
// it has to exist in the clear (AGENTS.md §27/§38: never log it).
public sealed record EmailVerificationRequestedIntegrationEvent(
    Guid UserId,
    string Email,
    string VerificationToken,
    DateTime ExpiresAtUtc,
    DateTime OccurredOnUtc);
