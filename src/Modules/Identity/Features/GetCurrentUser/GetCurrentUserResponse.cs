namespace GenclikMerkezi.Modules.Identity.Features.GetCurrentUser;

public sealed record GetCurrentUserResponse(
    Guid UserId,
    string Email,
    string Role,
    string Status,
    DateTime CreatedAtUtc);
