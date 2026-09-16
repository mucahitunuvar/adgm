using GenclikMerkezi.Modules.Identity.Application.Abstractions;

namespace GenclikMerkezi.UnitTests.Identity.TestDoubles;

public sealed class FakeEmailVerificationTokenGenerator : IEmailVerificationTokenGenerator
{
    public TimeSpan Lifetime { get; init; } = TimeSpan.FromHours(48);

    public string GenerateToken() => $"plain-verification-token-{Guid.NewGuid()}";

    public string Hash(string token) => $"verification-hash:{token}";
}
