namespace GenclikMerkezi.Contracts.Identity;

// Projection shape returned by IIdentityService.GetUserProfileAsync - deliberately not the full
// User aggregate (AGENTS.md §40: avoid loading entire aggregates for simple read projections).
public sealed record IdentityUserProfile(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber);
