using GenclikMerkezi.Modules.Identity.Application.Abstractions;

namespace GenclikMerkezi.UnitTests.Identity.TestDoubles;

public sealed class FakeRefreshTokenGenerator : IRefreshTokenGenerator
{
    public TimeSpan Lifetime { get; init; } = TimeSpan.FromDays(7);

    public string GenerateToken() => $"plain-token-{Guid.NewGuid()}";

    public string Hash(string token) => $"hash:{token}";
}
