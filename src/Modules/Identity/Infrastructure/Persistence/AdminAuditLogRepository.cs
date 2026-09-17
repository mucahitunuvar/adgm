using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Identity.Infrastructure.Persistence;

public sealed class AdminAuditLogRepository(IdentityDbContext dbContext) : IAdminAuditLogRepository
{
    public void Add(AdminAuditLogEntry entry)
    {
        dbContext.AdminAuditLogEntries.Add(entry);
    }

    public async Task<PagedResult<AdminAuditLogEntry>> SearchAsync(
        AdminAuditLogFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.AdminAuditLogEntries.AsNoTracking();

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

        return await query
            .OrderByDescending(e => e.OccurredAtUtc)
            .ToPagedResultAsync(new PagedRequest { Page = filter.Page, PageSize = filter.PageSize }, cancellationToken);
    }
}
