namespace GenclikMerkezi.Modules.Identity.Features.AdminGetUsers;

public sealed record AdminUserListItemResponse(
    Guid UserId,
    string Email,
    string Role,
    string Status,
    bool EmailConfirmed,
    bool IsLockedOut,
    DateTime CreatedAtUtc);
