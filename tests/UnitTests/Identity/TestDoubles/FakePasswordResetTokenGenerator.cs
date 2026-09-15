using GenclikMerkezi.Modules.Identity.Application.Abstractions;

namespace GenclikMerkezi.UnitTests.Identity.TestDoubles;

public sealed class FakePasswordResetTokenGenerator : IPasswordResetTokenGenerator
{
    public TimeSpan Lifetime { get; init; } = TimeSpan.FromMinutes(30);

    public string GenerateToken() => $"plain-reset-token-{Guid.NewGuid()}";

    public string Hash(string token) => $"reset-hash:{token}";
}
