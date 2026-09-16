namespace GenclikMerkezi.Contracts.IntegrationEvents;

// Carries the plaintext reset token so Notification can build the email's link - only ever
// hashed at rest (Identity's PasswordResetToken.TokenHash), so this is the one point it has to
// exist in the clear (AGENTS.md §27/§38: never log it).
public sealed record PasswordResetRequestedIntegrationEvent(
    Guid UserId,
    string Email,
    string ResetToken,
    DateTime ExpiresAtUtc,
    DateTime OccurredOnUtc);
