namespace GenclikMerkezi.Modules.Identity.Features.AdminGetUserById;

public sealed record AdminUserDetailResponse(
    Guid UserId,
    string Email,
    string Role,
    string Status,
    bool EmailConfirmed,
    bool IsLockedOut,
    DateTime? LockedUntilUtc,
    int FailedLoginAttemptCount,
    DateTime CreatedAtUtc);
