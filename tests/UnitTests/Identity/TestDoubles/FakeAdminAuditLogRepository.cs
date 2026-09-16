using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Identity.TestDoubles;

public sealed class FakeAdminAuditLogRepository : IAdminAuditLogRepository
{
    private readonly List<AdminAuditLogEntry> _entries = [];

    public IReadOnlyCollection<AdminAuditLogEntry> Entries => _entries.AsReadOnly();

    public void Add(AdminAuditLogEntry entry)
    {
        _entries.Add(entry);
    }

    public Task<PagedResult<AdminAuditLogEntry>> SearchAsync(
        AdminAuditLogFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = _entries.AsEnumerable();

        if (filter.TargetUserId is not null)
        {
            query = query.Where(e => e.TargetUserId == filter.TargetUserId);
        }

        if (filter.ActionType is not null)
        {
            query = query.Where(e => e.ActionType == filter.ActionType);
        }

        if (filter.FromUtc is not null)
        {
            query = query.Where(e => e.OccurredAtUtc >= filter.FromUtc);
        }

        if (filter.ToUtc is not null)
        {
            query = query.Where(e => e.OccurredAtUtc <= filter.ToUtc);
        }

        var matched = query.OrderByDescending(e => e.OccurredAtUtc).ToList();

        var items = matched
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return Task.FromResult(new PagedResult<AdminAuditLogEntry>(items, matched.Count, filter.Page, filter.PageSize));
    }
}
