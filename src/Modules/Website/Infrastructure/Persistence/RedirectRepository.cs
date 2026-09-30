using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class RedirectRepository(WebsiteDbContext dbContext) : IRedirectRepository
{
    public Task<Redirect?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Redirects.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<Redirect?> GetByFromPathAsync(LanguageCode languageCode, string fromPath, CancellationToken cancellationToken = default) =>
        dbContext.Redirects.FirstOrDefaultAsync(r => r.LanguageCode == languageCode && r.FromPath == fromPath, cancellationToken);

    public Task<PagedResult<Redirect>> SearchAsync(
        LanguageCode? languageCode, bool? isAutomatic, string? search, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Redirects.AsNoTracking().AsQueryable();

        if (languageCode is not null)
        {
            query = query.Where(r => r.LanguageCode == languageCode);
        }

        if (isAutomatic is not null)
        {
            query = query.Where(r => r.IsAutomatic == isAutomatic.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r => r.FromPath.Contains(search));
        }

        return query.OrderByDescending(r => r.CreatedAtUtc).ToPagedResultAsync(pagedRequest, cancellationToken);
    }

    public async Task<IReadOnlyList<Redirect>> GetByTargetContentItemIdAsync(Guid contentItemId, CancellationToken cancellationToken = default) =>
        await dbContext.Redirects.Where(r => r.TargetContentItemId == contentItemId).ToListAsync(cancellationToken);

    public void Add(Redirect redirect) => dbContext.Redirects.Add(redirect);

    public void Remove(Redirect redirect) => dbContext.Redirects.Remove(redirect);
}
