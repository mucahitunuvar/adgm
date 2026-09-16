using GenclikMerkezi.Modules.Identity.Domain;

namespace GenclikMerkezi.Modules.Identity.Application.Abstractions;

// Projection shape for admin listing - deliberately not the full User aggregate (AGENTS.md §40:
// avoid loading entire aggregates for simple read projections).
public sealed record UserSummary(
    Guid Id,
    string Email,
    UserRole Role,
    UserStatus Status,
    bool EmailConfirmed,
    bool IsLockedOut,
    DateTime CreatedAtUtc);
