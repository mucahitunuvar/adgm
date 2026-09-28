using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class ContentTypeRepository(WebsiteDbContext dbContext) : IContentTypeRepository
{
    public Task<ContentType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.ContentTypes.FirstOrDefaultAsync(ct => ct.Id == id, cancellationToken);

    public Task<ContentType?> GetByKeyAsync(ContentTypeKey key, CancellationToken cancellationToken = default) =>
        dbContext.ContentTypes.FirstOrDefaultAsync(ct => ct.Key == key, cancellationToken);

    public async Task<IReadOnlyList<ContentType>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.ContentTypes
            .AsNoTracking()
            .OrderBy(ct => ct.SortOrder)
            .ToListAsync(cancellationToken);

    public Task<bool> RoutePrefixExistsAsync(
        LanguageCode languageCode, string routePrefix, Guid? excludeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(routePrefix))
        {
            return Task.FromResult(false);
        }

        return dbContext.ContentTypes
            .Where(ct => excludeId == null || ct.Id != excludeId.Value)
            .AnyAsync(ct => ct.Translations.Any(t => t.LanguageCode == languageCode && t.RoutePrefix == routePrefix), cancellationToken);
    }

    public void Add(ContentType contentType) => dbContext.ContentTypes.Add(contentType);
}
