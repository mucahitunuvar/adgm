namespace GenclikMerkezi.Contracts.IntegrationEvents;

// Carries the plaintext verification token so Notification can build the email's link -
// the token itself is only ever hashed at rest (Identity's EmailVerificationToken.TokenHash),
// so this is the one point it has to exist in the clear (AGENTS.md §27/§38: never log it).
public sealed record UserRegisteredIntegrationEvent(
    Guid UserId,
    string Email,
    string VerificationToken,
    DateTime ExpiresAtUtc,
    DateTime OccurredOnUtc);
