namespace GenclikMerkezi.Modules.Identity.Features.RegisterUser;

public sealed record RegisterUserResponse(Guid UserId, string Email, string Role);
