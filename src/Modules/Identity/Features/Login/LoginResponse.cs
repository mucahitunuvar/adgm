namespace GenclikMerkezi.Modules.Identity.Features.Login;

public sealed record LoginResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    bool EmailConfirmed);
