using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Identity.Domain;

public sealed class User : AggregateRoot
{
    private const int MaxFailedLoginAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private readonly List<RefreshToken> _refreshTokens = [];
    private readonly List<PasswordResetToken> _passwordResetTokens = [];
    private readonly List<EmailVerificationToken> _emailVerificationTokens = [];

    public Email Email { get; private set; }

    public PasswordHash PasswordHash { get; private set; }

    public UserRole Role { get; private set; }

    public UserStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public int FailedLoginAttemptCount { get; private set; }

    public DateTime? LockedUntilUtc { get; private set; }

    public bool EmailConfirmed { get; private set; }

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public IReadOnlyCollection<PasswordResetToken> PasswordResetTokens => _passwordResetTokens.AsReadOnly();

    public IReadOnlyCollection<EmailVerificationToken> EmailVerificationTokens => _emailVerificationTokens.AsReadOnly();

    public bool IsLockedOut => Status == UserStatus.Locked && LockedUntilUtc is not null && DateTime.UtcNow < LockedUntilUtc;

    private User(Guid id, Email email, PasswordHash passwordHash, UserRole role)
        : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        Status = UserStatus.Active;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private User()
    {
        Email = null!;
        PasswordHash = null!;
    }

    public static User Register(Email email, PasswordHash passwordHash, UserRole role)
    {
        var user = new User(Guid.NewGuid(), email, passwordHash, role);

        user.RaiseDomainEvent(new UserRegisteredDomainEvent(user.Id, email.Value, role));

        return user;
    }

    public RefreshToken IssueRefreshToken(string tokenHash, DateTime expiresAtUtc)
    {
        var refreshToken = RefreshToken.Create(Id, tokenHash, expiresAtUtc);
        _refreshTokens.Add(refreshToken);
        return refreshToken;
    }

    public RefreshToken? FindRefreshToken(string tokenHash)
    {
        return _refreshTokens.FirstOrDefault(rt => rt.TokenHash == tokenHash);
    }

    public void RevokeRefreshToken(string tokenHash, string? replacedByTokenHash = null)
    {
        FindRefreshToken(tokenHash)?.Revoke(replacedByTokenHash);
    }

    public void RevokeAllActiveRefreshTokens()
    {
        foreach (var refreshToken in _refreshTokens.Where(rt => rt.IsActive))
        {
            refreshToken.Revoke();
        }
    }

    public void RegisterFailedLoginAttempt()
    {
        FailedLoginAttemptCount++;

        if (FailedLoginAttemptCount >= MaxFailedLoginAttempts)
        {
            Status = UserStatus.Locked;
            LockedUntilUtc = DateTime.UtcNow.Add(LockoutDuration);
            RaiseDomainEvent(new UserLockedOutDomainEvent(Id, LockedUntilUtc.Value));
        }
    }

    public void RegisterSuccessfulLogin()
    {
        FailedLoginAttemptCount = 0;
        LockedUntilUtc = null;
    }

    public void UnlockIfLockoutExpired()
    {
        if (Status == UserStatus.Locked && LockedUntilUtc is not null && DateTime.UtcNow >= LockedUntilUtc)
        {
            Status = UserStatus.Active;
            LockedUntilUtc = null;
            FailedLoginAttemptCount = 0;
        }
    }

    public void ChangePassword(PasswordHash newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        RevokeAllActiveRefreshTokens();
        RaiseDomainEvent(new UserPasswordChangedDomainEvent(Id));
    }

    public void ChangeRole(UserRole role)
    {
        if (Role == role)
        {
            return;
        }

        var previousRole = Role;
        Role = role;
        RaiseDomainEvent(new UserRoleChangedDomainEvent(Id, previousRole, role));
    }

    public PasswordResetToken IssuePasswordResetToken(string tokenHash, DateTime expiresAtUtc)
    {
        var token = PasswordResetToken.Create(Id, tokenHash, expiresAtUtc);
        _passwordResetTokens.Add(token);
        return token;
    }

    public PasswordResetToken? FindPasswordResetToken(string tokenHash)
    {
        return _passwordResetTokens.FirstOrDefault(t => t.TokenHash == tokenHash);
    }

    public Result ResetPassword(string tokenHash, PasswordHash newPasswordHash)
    {
        var token = FindPasswordResetToken(tokenHash);

        if (token is null || !token.IsActive)
        {
            return Result.Failure(
                Error.Unauthorized("Auth.InvalidResetToken", "The password reset token is invalid or has expired."));
        }

        token.MarkUsed();
        PasswordHash = newPasswordHash;
        RevokeAllActiveRefreshTokens();
        RegisterSuccessfulLogin();

        RaiseDomainEvent(new UserPasswordResetDomainEvent(Id));

        return Result.Success();
    }

    public EmailVerificationToken IssueEmailVerificationToken(string tokenHash, DateTime expiresAtUtc)
    {
        var token = EmailVerificationToken.Create(Id, tokenHash, expiresAtUtc);
        _emailVerificationTokens.Add(token);
        return token;
    }

    public EmailVerificationToken? FindEmailVerificationToken(string tokenHash)
    {
        return _emailVerificationTokens.FirstOrDefault(t => t.TokenHash == tokenHash);
    }

    public Result ConfirmEmail(string tokenHash)
    {
        if (EmailConfirmed)
        {
            return Result.Success();
        }

        var token = FindEmailVerificationToken(tokenHash);

        if (token is null || !token.IsActive)
        {
            return Result.Failure(
                Error.Unauthorized("Auth.InvalidVerificationToken", "The email verification token is invalid or has expired."));
        }

        token.MarkUsed();
        EmailConfirmed = true;

        RaiseDomainEvent(new UserEmailVerifiedDomainEvent(Id, Email.Value));

        return Result.Success();
    }
}
