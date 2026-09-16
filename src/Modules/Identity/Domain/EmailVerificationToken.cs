using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Identity.Domain;

public sealed class EmailVerificationToken : Entity
{
    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UsedAtUtc { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;

    public bool IsUsed => UsedAtUtc is not null;

    public bool IsActive => !IsExpired && !IsUsed;

    private EmailVerificationToken(Guid id, Guid userId, string tokenHash, DateTime expiresAtUtc)
        : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private EmailVerificationToken()
    {
        TokenHash = string.Empty;
    }

    internal static EmailVerificationToken Create(Guid userId, string tokenHash, DateTime expiresAtUtc)
    {
        return new EmailVerificationToken(Guid.NewGuid(), userId, tokenHash, expiresAtUtc);
    }

    public void MarkUsed()
    {
        if (IsUsed)
        {
            return;
        }

        UsedAtUtc = DateTime.UtcNow;
    }
}
