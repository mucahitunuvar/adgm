using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Identity.TestDoubles;

public sealed class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users = [];

    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.Email == email));
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
    }

    public Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.FindRefreshToken(tokenHash) is not null));
    }

    public Task<User?> GetByPasswordResetTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.FindPasswordResetToken(tokenHash) is not null));
    }

    public Task<User?> GetByEmailVerificationTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.FindEmailVerificationToken(tokenHash) is not null));
    }

    public void Add(User user)
    {
        _users.Add(user);
    }

    public Task<PagedResult<UserSummary>> SearchAsync(UserSearchFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _users.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filter.Email))
        {
            query = query.Where(u => u.Email.Value.Contains(filter.Email, StringComparison.OrdinalIgnoreCase));
        }

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

        if (filter.IsLockedOut is not null)
        {
            query = query.Where(u => u.IsLockedOut == filter.IsLockedOut);
        }

        var matched = query.OrderByDescending(u => u.CreatedAtUtc).ToList();

        var items = matched
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(u => new UserSummary(
                u.Id, u.Email.Value, u.Role, u.Status, u.EmailConfirmed, u.IsLockedOut, u.CreatedAtUtc))
            .ToList();

        return Task.FromResult(new PagedResult<UserSummary>(items, matched.Count, filter.Page, filter.PageSize));
    }
}
