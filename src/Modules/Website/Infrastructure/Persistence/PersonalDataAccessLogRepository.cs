using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class PersonalDataAccessLogRepository(WebsiteDbContext dbContext) : IPersonalDataAccessLogRepository
{
    public void Add(PersonalDataAccessLog accessLog) => dbContext.PersonalDataAccessLogs.Add(accessLog);

    public Task<PagedResult<PersonalDataAccessLog>> SearchAsync(
        DateTime? fromUtc, DateTime? toUtc, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var query = dbContext.PersonalDataAccessLogs.AsNoTracking().AsQueryable();

        if (fromUtc is not null)
        {
            query = query.Where(a => a.AccessedAtUtc >= fromUtc.Value);
        }

        if (toUtc is not null)
        {
            query = query.Where(a => a.AccessedAtUtc <= toUtc.Value);
        }

        return query.OrderByDescending(a => a.AccessedAtUtc).ToPagedResultAsync(pagedRequest, cancellationToken);
    }
}
