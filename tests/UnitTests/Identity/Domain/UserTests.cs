using GenclikMerkezi.Modules.Identity.Domain;

namespace GenclikMerkezi.UnitTests.Identity.Domain;

public class UserTests
{
    private static Email CreateEmail(string value = "aday@example.com") => Email.Create(value).Value;

    private static PasswordHash CreatePasswordHash(string value = "hashed-value") =>
        PasswordHash.FromHashedValue(value);

    [Fact]
    public void Register_SetsExpectedDefaultsAndRaisesDomainEvent()
    {
        var email = CreateEmail();
        var passwordHash = CreatePasswordHash();

        var user = User.Register(email, passwordHash, UserRole.Candidate);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(email, user.Email);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Equal(UserRole.Candidate, user.Role);
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.Empty(user.RefreshTokens);

        var domainEvent = Assert.Single(user.DomainEvents);
        var userRegistered = Assert.IsType<UserRegisteredDomainEvent>(domainEvent);
        Assert.Equal(user.Id, userRegistered.UserId);
        Assert.Equal(email.Value, userRegistered.Email);
        Assert.Equal(UserRole.Candidate, userRegistered.Role);
    }

    [Fact]
    public void IssueRefreshToken_AddsTokenToCollectionWithExpectedValues()
    {
        var user = User.Register(CreateEmail(), CreatePasswordHash(), UserRole.Candidate);
        var expiresAtUtc = DateTime.UtcNow.AddDays(7);

        var token = user.IssueRefreshToken("token-hash", expiresAtUtc);

        Assert.Single(user.RefreshTokens);
        Assert.Same(token, user.RefreshTokens.Single());
        Assert.Equal(user.Id, token.UserId);
        Assert.Equal("token-hash", token.TokenHash);
        Assert.Equal(expiresAtUtc, token.ExpiresAtUtc);
        Assert.True(token.IsActive);
    }

    [Fact]
    public void FindRefreshToken_WithMatchingHash_ReturnsToken()
    {
        var user = User.Register(CreateEmail(), CreatePasswordHash(), UserRole.Candidate);
        var issued = user.IssueRefreshToken("token-hash", DateTime.UtcNow.AddDays(7));

        var found = user.FindRefreshToken("token-hash");

        Assert.Same(issued, found);
    }

    [Fact]
    public void FindRefreshToken_WithUnknownHash_ReturnsNull()
    {
        var user = User.Register(CreateEmail(), CreatePasswordHash(), UserRole.Candidate);
        user.IssueRefreshToken("token-hash", DateTime.UtcNow.AddDays(7));

        var found = user.FindRefreshToken("unknown-hash");

        Assert.Null(found);
    }

    [Fact]
    public void RevokeRefreshToken_WithMatchingHash_RevokesTokenAndSetsReplacement()
    {
        var user = User.Register(CreateEmail(), CreatePasswordHash(), UserRole.Candidate);
        var token = user.IssueRefreshToken("token-hash", DateTime.UtcNow.AddDays(7));

        user.RevokeRefreshToken("token-hash", "new-token-hash");

        Assert.True(token.IsRevoked);
        Assert.False(token.IsActive);
        Assert.Equal("new-token-hash", token.ReplacedByTokenHash);
    }

    [Fact]
    public void RevokeRefreshToken_WithUnknownHash_DoesNotThrowAndChangesNothing()
    {
        var user = User.Register(CreateEmail(), CreatePasswordHash(), UserRole.Candidate);
        var token = user.IssueRefreshToken("token-hash", DateTime.UtcNow.AddDays(7));

        var exception = Record.Exception(() => user.RevokeRefreshToken("unknown-hash"));

        Assert.Null(exception);
        Assert.False(token.IsRevoked);
    }

    [Fact]
    public void RevokeAllActiveRefreshTokens_OnlyRevokesTokensThatAreCurrentlyActive()
    {
        var user = User.Register(CreateEmail(), CreatePasswordHash(), UserRole.Candidate);
        var active = user.IssueRefreshToken("active-hash", DateTime.UtcNow.AddDays(7));
        var alreadyExpired = user.IssueRefreshToken("expired-hash", DateTime.UtcNow.AddMinutes(-1));
        var alreadyRevoked = user.IssueRefreshToken("revoked-hash", DateTime.UtcNow.AddDays(7));
        alreadyRevoked.Revoke();

        user.RevokeAllActiveRefreshTokens();

        Assert.True(active.IsRevoked);
        Assert.False(alreadyExpired.IsRevoked);
        Assert.NotNull(alreadyRevoked.RevokedAtUtc);
    }

    [Fact]
    public void RegisterFailedLoginAttempt_BelowThreshold_IncrementsCounterAndDoesNotLock()
    {
        var user = User.Register(CreateEmail(), CreatePasswordHash(), UserRole.Candidate);

        user.RegisterFailedLoginAttempt();
        user.RegisterFailedLoginAttempt();

        Assert.Equal(2, user.FailedLoginAttemptCount);
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.False(user.IsLockedOut);
        Assert.Null(user.LockedUntilUtc);
    }

    [Fact]
    public void RegisterFailedLoginAttempt_AtThreshold_LocksAccountAndRaisesDomainEvent()
    {
        var user = User.Register(CreateEmail(), CreatePasswordHash(), UserRole.Candidate);

        for (var i = 0; i < 5; i++)
        {
            user.RegisterFailedLoginAttempt();
        }

        Assert.Equal(UserStatus.Locked, user.Status);
        Assert.True(user.IsLockedOut);
        Assert.NotNull(user.LockedUntilUtc);

        var domainEvent = Assert.Single(user.DomainEvents.OfType<UserLockedOutDomainEvent>());
        Assert.Equal(user.Id, domainEvent.UserId);
        Assert.Equal(user.LockedUntilUtc, domainEvent.LockedUntilUtc);
    }

    [Fact]
    public void RegisterSuccessfulLogin_ResetsFailedAttemptCounterAndLock()
    {
        var user = User.Register(CreateEmail(), CreatePasswordHash(), UserRole.Candidate);
        user.RegisterFailedLoginAttempt();
        user.RegisterFailedLoginAttempt();

        user.RegisterSuccessfulLogin();

        Assert.Equal(0, user.FailedLoginAttemptCount);
        Assert.Null(user.LockedUntilUtc);
    }

    [Fact]
    public void UnlockIfLockoutExpired_BeforeLockWindowElapses_StaysLocked()
    {
        var user = User.Register(CreateEmail(), CreatePasswordHash(), UserRole.Candidate);
        for (var i = 0; i < 5; i++)
        {
            user.RegisterFailedLoginAttempt();
        }

        user.UnlockIfLockoutExpired();

        Assert.Equal(UserStatus.Locked, user.Status);
        Assert.True(user.IsLockedOut);
    }

    [Fact]
    public void ChangePassword_UpdatesHashAndRevokesActiveRefreshTokensAndRaisesDomainEvent()
    {
        var user = User.Register(CreateEmail(), CreatePasswordHash("old-hash"), UserRole.Candidate);
        var activeToken = user.IssueRefreshToken("token-hash", DateTime.UtcNow.AddDays(7));
        var newPasswordHash = CreatePasswordHash("new-hash");

        user.ChangePassword(newPasswordHash);

        Assert.Equal(newPasswordHash, user.PasswordHash);
        Assert.True(activeToken.IsRevoked);
        Assert.Contains(user.DomainEvents, e => e is UserPasswordChangedDomainEvent);
    }

    [Fact]
    public void ChangeRole_ToDifferentRole_UpdatesRoleAndRaisesDomainEvent()
    {
        var user = User.Register(CreateEmail(), CreatePasswordHash(), UserRole.Candidate);

        user.ChangeRole(UserRole.CareerAdvisor);

        Assert.Equal(UserRole.CareerAdvisor, user.Role);
        var domainEvent = Assert.Single(user.DomainEvents.OfType<UserRoleChangedDomainEvent>());
        Assert.Equal(UserRole.Candidate, domainEvent.PreviousRole);
        Assert.Equal(UserRole.CareerAdvisor, domainEvent.NewRole);
    }

    [Fact]
    public void ChangeRole_ToSameRole_DoesNotRaiseDomainEvent()
    {
        var user = User.Register(CreateEmail(), CreatePasswordHash(), UserRole.Candidate);

        user.ChangeRole(UserRole.Candidate);

        Assert.Empty(user.DomainEvents.OfType<UserRoleChangedDomainEvent>());
    }

    [Fact]
    public void IssuePasswordResetToken_ThenResetPassword_UpdatesPasswordAndConsumesToken()
    {
        var user = User.Register(CreateEmail(), CreatePasswordHash("old-hash"), UserRole.Candidate);
        var activeRefreshToken = user.IssueRefreshToken("refresh-hash", DateTime.UtcNow.AddDays(7));
        var resetToken = user.IssuePasswordResetToken("reset-hash", DateTime.UtcNow.AddMinutes(30));
        var newPasswordHash = CreatePasswordHash("new-hash");

        var result = user.ResetPassword("reset-hash", newPasswordHash);

        Assert.True(result.IsSuccess);
        Assert.Equal(newPasswordHash, user.PasswordHash);
        Assert.True(resetToken.IsUsed);
        Assert.True(activeRefreshToken.IsRevoked);
        Assert.Contains(user.DomainEvents, e => e is UserPasswordResetDomainEvent);
    }

    [Fact]
    public void ResetPassword_WithUnknownToken_ReturnsFailure()
    {
        var user = User.Register(CreateEmail(), CreatePasswordHash("old-hash"), UserRole.Candidate);

        var result = user.ResetPassword("unknown-hash", CreatePasswordHash("new-hash"));

        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidResetToken", result.Error.Code);
        Assert.Equal(CreatePasswordHash("old-hash"), user.PasswordHash);
    }

    [Fact]
    public void ResetPassword_WithAlreadyUsedToken_ReturnsFailure()
    {
        var user = User.Register(CreateEmail(), CreatePasswordHash("old-hash"), UserRole.Candidate);
        user.IssuePasswordResetToken("reset-hash", DateTime.UtcNow.AddMinutes(30));
        user.ResetPassword("reset-hash", CreatePasswordHash("first-new-hash"));

        var result = user.ResetPassword("reset-hash", CreatePasswordHash("second-new-hash"));

        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidResetToken", result.Error.Code);
    }

    [Fact]
    public void ResetPassword_WithExpiredToken_ReturnsFailure()
    {
        var user = User.Register(CreateEmail(), CreatePasswordHash("old-hash"), UserRole.Candidate);
        user.IssuePasswordResetToken("reset-hash", DateTime.UtcNow.AddSeconds(-1));

        var result = user.ResetPassword("reset-hash", CreatePasswordHash("new-hash"));

        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidResetToken", result.Error.Code);
    }
}
