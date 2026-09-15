using System.Security.Cryptography;
using System.Text;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.Modules.Identity.Infrastructure.Security;

public sealed class PasswordResetTokenGenerator(IOptions<PasswordResetSettings> settings) : IPasswordResetTokenGenerator
{
    public TimeSpan Lifetime { get; } = TimeSpan.FromMinutes(settings.Value.TokenExpirationMinutes);

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
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes);
    }
}
