using GenclikMerkezi.Modules.Identity.Domain;

namespace GenclikMerkezi.UnitTests.Identity.Domain;

public class PasswordResetTokenTests
{
    private static User CreateUser() =>
        User.Register(Email.Create("aday@example.com").Value, PasswordHash.FromHashedValue("hash"), "Test", "User", null, UserRole.Candidate);

    [Fact]
    public void IsActive_WhenNeverExpiredOrUsed_ReturnsTrue()
    {
        var token = CreateUser().IssuePasswordResetToken("hash", DateTime.UtcNow.AddMinutes(30));

        Assert.False(token.IsExpired);
        Assert.False(token.IsUsed);
        Assert.True(token.IsActive);
    }

    [Fact]
    public void IsActive_WhenExpiresAtUtcIsInThePast_ReturnsFalse()
    {
        var token = CreateUser().IssuePasswordResetToken("hash", DateTime.UtcNow.AddSeconds(-1));

        Assert.True(token.IsExpired);
        Assert.False(token.IsActive);
    }

    [Fact]
    public void MarkUsed_SetsUsedAtUtc()
    {
        var token = CreateUser().IssuePasswordResetToken("hash", DateTime.UtcNow.AddMinutes(30));

        token.MarkUsed();

        Assert.NotNull(token.UsedAtUtc);
        Assert.True(token.IsUsed);
        Assert.False(token.IsActive);
    }

    [Fact]
    public void MarkUsed_CalledTwice_IsIdempotentAndKeepsFirstUsedAtUtc()
    {
        var token = CreateUser().IssuePasswordResetToken("hash", DateTime.UtcNow.AddMinutes(30));

        token.MarkUsed();
        var firstUsedAtUtc = token.UsedAtUtc;

        token.MarkUsed();

        Assert.Equal(firstUsedAtUtc, token.UsedAtUtc);
    }
}
