namespace GenclikMerkezi.Modules.Identity.Features.RegisterUser;

public sealed record RegisterUserRequest(string Email, string Password, string Role);
