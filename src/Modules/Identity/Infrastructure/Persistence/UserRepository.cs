using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Identity.Infrastructure.Persistence;

public sealed class UserRepository(IdentityDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.PasswordResetTokens)
            .Include(u => u.EmailVerificationTokens)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.PasswordResetTokens)
            .Include(u => u.EmailVerificationTokens)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.PasswordResetTokens)
            .Include(u => u.EmailVerificationTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.TokenHash == tokenHash), cancellationToken);
    }

    public Task<User?> GetByPasswordResetTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.PasswordResetTokens)
            .Include(u => u.EmailVerificationTokens)
            .FirstOrDefaultAsync(u => u.PasswordResetTokens.Any(t => t.TokenHash == tokenHash), cancellationToken);
    }

    public Task<User?> GetByEmailVerificationTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.PasswordResetTokens)
            .Include(u => u.EmailVerificationTokens)
            .FirstOrDefaultAsync(u => u.EmailVerificationTokens.Any(t => t.TokenHash == tokenHash), cancellationToken);
    }

    public void Add(User user)
    {
        dbContext.Users.Add(user);
    }
}
