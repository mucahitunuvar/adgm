using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class NotFoundLogRepository(WebsiteDbContext dbContext) : INotFoundLogRepository
{
    public Task<NotFoundLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.NotFoundLogs.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public Task<NotFoundLog?> GetByPathAsync(LanguageCode languageCode, string path, CancellationToken cancellationToken = default) =>
        dbContext.NotFoundLogs.FirstOrDefaultAsync(n => n.LanguageCode == languageCode && n.Path == path, cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        dbContext.NotFoundLogs.CountAsync(cancellationToken);

    public Task<PagedResult<NotFoundLog>> SearchAsync(PagedRequest pagedRequest, CancellationToken cancellationToken = default) =>
        dbContext.NotFoundLogs.AsNoTracking().OrderByDescending(n => n.HitCount).ToPagedResultAsync(pagedRequest, cancellationToken);

    public Task<int> DeleteStaleAsync(DateTime olderThanUtc, int maxHitCount, CancellationToken cancellationToken = default) =>
        dbContext.NotFoundLogs
            .Where(n => n.LastSeenAtUtc < olderThanUtc && n.HitCount < maxHitCount)
            .ExecuteDeleteAsync(cancellationToken);

    public void Add(NotFoundLog notFoundLog) => dbContext.NotFoundLogs.Add(notFoundLog);

    public void Remove(NotFoundLog notFoundLog) => dbContext.NotFoundLogs.Remove(notFoundLog);
}
