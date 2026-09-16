using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Results;
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

    public async Task<PagedResult<UserSummary>> SearchAsync(UserSearchFilter filter, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Users.AsNoTracking();

        if (filter.Role is not null)
        {
            query = query.Where(u => u.Role == filter.Role);
        }

        if (filter.Status is not null)
        {
            query = query.Where(u => u.Status == filter.Status);
        }

        if (filter.EmailConfirmed is not null)
        {
            query = query.Where(u => u.EmailConfirmed == filter.EmailConfirmed);
        }

        var utcNow = DateTime.UtcNow;

        if (filter.IsLockedOut is true)
        {
            query = query.Where(u => u.Status == UserStatus.Locked && u.LockedUntilUtc != null && u.LockedUntilUtc > utcNow);
        }
        else if (filter.IsLockedOut is false)
        {
            query = query.Where(u => !(u.Status == UserStatus.Locked && u.LockedUntilUtc != null && u.LockedUntilUtc > utcNow));
        }

        if (!string.IsNullOrWhiteSpace(filter.Email))
        {
            // Email is a value object mapped via HasConversion (Email <-> string); EF Core cannot
            // translate a LIKE predicate over a member access chained onto it (u.Email.Value), and
            // EF.Property<string> only sidesteps that for a boolean Where predicate - it throws an
            // InvalidCastException the moment the value is actually materialized (e.g. in a Select).
            // Candidate ids are narrowed here instead, from whatever the other filters already
            // matched, using the ordinary (fully supported) whole-property read.
            var candidates = await query
                .Select(u => new { u.Id, u.Email })
                .ToListAsync(cancellationToken);

            var trimmedSearch = filter.Email.Trim();
            var matchingIds = candidates
                .Where(u => u.Email.Value.Contains(trimmedSearch, StringComparison.OrdinalIgnoreCase))
                .Select(u => u.Id)
                .ToList();

            query = query.Where(u => matchingIds.Contains(u.Id));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Projected as the natural Email-typed property (its HasConversion applies normally here,
        // same as any other read of u.Email) and mapped to UserSummary's plain string only after
        // materialization - unlike the Where clause above, Select requires the CLR type it declares
        // to match the property's model type, so EF.Property<string> would throw an InvalidCastException
        // here instead of just failing to translate.
        var rows = await query
            .OrderByDescending(u => u.CreatedAtUtc)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.Role,
                u.Status,
                u.EmailConfirmed,
                u.LockedUntilUtc,
                u.CreatedAtUtc,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(u => new UserSummary(
                u.Id,
                u.Email.Value,
                u.Role,
                u.Status,
                u.EmailConfirmed,
                u.Status == UserStatus.Locked && u.LockedUntilUtc is not null && u.LockedUntilUtc > utcNow,
                u.CreatedAtUtc))
            .ToList();

        return new PagedResult<UserSummary>(items, totalCount, filter.Page, filter.PageSize);
    }
}
