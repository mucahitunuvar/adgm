using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Identity.Domain;

public sealed class RefreshToken : Entity
{
    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public string? ReplacedByTokenHash { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;

    public bool IsRevoked => RevokedAtUtc is not null;

    public bool IsActive => !IsExpired && !IsRevoked;

    private RefreshToken(Guid id, Guid userId, string tokenHash, DateTime expiresAtUtc)
        : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private RefreshToken()
    {
        TokenHash = string.Empty;
    }

    internal static RefreshToken Create(Guid userId, string tokenHash, DateTime expiresAtUtc)
    {
        return new RefreshToken(Guid.NewGuid(), userId, tokenHash, expiresAtUtc);
    }

    public void Revoke(string? replacedByTokenHash = null)
    {
        if (IsRevoked)
        {
            return;
        }

        RevokedAtUtc = DateTime.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
