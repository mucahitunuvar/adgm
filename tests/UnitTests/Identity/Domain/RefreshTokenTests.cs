using GenclikMerkezi.Modules.Identity.Domain;

namespace GenclikMerkezi.UnitTests.Identity.Domain;

public class RefreshTokenTests
{
    private static User CreateUser() =>
        User.Register(Email.Create("aday@example.com").Value, PasswordHash.FromHashedValue("hash"), UserRole.Candidate);

    [Fact]
    public void IsActive_WhenNeverExpiredOrRevoked_ReturnsTrue()
    {
        var token = CreateUser().IssueRefreshToken("hash", DateTime.UtcNow.AddDays(1));

        Assert.False(token.IsExpired);
        Assert.False(token.IsRevoked);
        Assert.True(token.IsActive);
    }

    [Fact]
    public void IsActive_WhenExpiresAtUtcIsInThePast_ReturnsFalse()
    {
        var token = CreateUser().IssueRefreshToken("hash", DateTime.UtcNow.AddSeconds(-1));

        Assert.True(token.IsExpired);
        Assert.False(token.IsActive);
    }

    [Fact]
    public void Revoke_SetsRevokedAtUtcAndReplacedByTokenHash()
    {
        var token = CreateUser().IssueRefreshToken("hash", DateTime.UtcNow.AddDays(1));

        token.Revoke("replacement-hash");

        Assert.NotNull(token.RevokedAtUtc);
        Assert.Equal("replacement-hash", token.ReplacedByTokenHash);
        Assert.True(token.IsRevoked);
        Assert.False(token.IsActive);
    }

    [Fact]
    public void Revoke_CalledTwice_IsIdempotentAndKeepsFirstRevocationData()
    {
        var token = CreateUser().IssueRefreshToken("hash", DateTime.UtcNow.AddDays(1));

        token.Revoke("first-replacement");
        var firstRevokedAtUtc = token.RevokedAtUtc;

        token.Revoke("second-replacement");

        Assert.Equal(firstRevokedAtUtc, token.RevokedAtUtc);
        Assert.Equal("first-replacement", token.ReplacedByTokenHash);
    }
}
