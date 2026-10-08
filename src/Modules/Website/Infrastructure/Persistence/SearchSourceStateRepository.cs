using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class SearchSourceStateRepository(WebsiteDbContext dbContext) : ISearchSourceStateRepository
{
    public Task<SearchSourceState?> GetAsync(string sourceKey, CancellationToken cancellationToken = default) =>
        dbContext.SearchSourceStates.FirstOrDefaultAsync(s => s.SourceKey == sourceKey, cancellationToken);

    public async Task<SearchSourceState> GetOrCreateAsync(string sourceKey, CancellationToken cancellationToken = default)
    {
        var existing = await GetAsync(sourceKey, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var created = SearchSourceState.Create(sourceKey).Value;
        dbContext.SearchSourceStates.Add(created);

        return created;
    }

    public async Task<IReadOnlyList<SearchSourceState>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SearchSourceStates.AsNoTracking().OrderBy(s => s.SourceKey).ToListAsync(cancellationToken);
}
