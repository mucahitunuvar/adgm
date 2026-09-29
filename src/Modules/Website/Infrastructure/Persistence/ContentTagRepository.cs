using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class ContentTagRepository(WebsiteDbContext dbContext) : IContentTagRepository
{
    public Task<ContentTag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.ContentTags.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<ContentTag?> GetBySlugAsync(LanguageCode languageCode, string slug, CancellationToken cancellationToken = default) =>
        dbContext.ContentTags.FirstOrDefaultAsync(t => t.LanguageCode == languageCode && t.Slug == slug, cancellationToken);

    public Task<PagedResult<ContentTag>> SearchAsync(
        LanguageCode? languageCode, string? search, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ContentTags.AsNoTracking().AsQueryable();

        if (languageCode is not null)
        {
            query = query.Where(t => t.LanguageCode == languageCode);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t => t.Name.Contains(search));
        }

        return query.OrderBy(t => t.Name).ToPagedResultAsync(pagedRequest, cancellationToken);
    }

    public Task<int> CountUsagesAsync(Guid tagId, CancellationToken cancellationToken = default) =>
        dbContext.ContentItems.SelectMany(ci => ci.Translations).CountAsync(t => t.TagIds.Contains(tagId), cancellationToken);

    public async Task<IReadOnlyList<ContentTag>> GetUnusedOlderThanAsync(DateTime olderThanUtc, CancellationToken cancellationToken = default) =>
        await dbContext.ContentTags
            .Where(t => t.CreatedAtUtc < olderThanUtc && !dbContext.ContentItems.SelectMany(ci => ci.Translations).Any(tr => tr.TagIds.Contains(t.Id)))
            .ToListAsync(cancellationToken);

    public void Add(ContentTag tag) => dbContext.ContentTags.Add(tag);

    public void Remove(ContentTag tag) => dbContext.ContentTags.Remove(tag);
}
