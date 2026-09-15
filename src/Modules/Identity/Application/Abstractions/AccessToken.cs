namespace GenclikMerkezi.Modules.Identity.Application.Abstractions;

public sealed record AccessToken(string Token, DateTime ExpiresAtUtc);
