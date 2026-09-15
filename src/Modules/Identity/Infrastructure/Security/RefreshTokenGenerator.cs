using System.Security.Cryptography;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.Modules.Identity.Infrastructure.Security;

public sealed class RefreshTokenGenerator(IOptions<JwtSettings> jwtSettings) : IRefreshTokenGenerator
{
    public TimeSpan Lifetime { get; } = TimeSpan.FromDays(jwtSettings.Value.RefreshTokenExpirationDays);

    public string GenerateToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public string Hash(string token)
    {
        var hashBytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes);
    }
}
