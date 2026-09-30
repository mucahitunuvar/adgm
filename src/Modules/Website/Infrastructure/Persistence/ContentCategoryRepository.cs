using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class ContentCategoryRepository(WebsiteDbContext dbContext) : IContentCategoryRepository
{
    public Task<ContentCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.ContentCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ContentCategory>> GetByContentTypeIdAsync(Guid contentTypeId, CancellationToken cancellationToken = default) =>
        await dbContext.ContentCategories
            .AsNoTracking()
            .Where(c => c.ContentTypeId == contentTypeId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);

    public Task<ContentCategory?> GetByTypeAndSlugAsync(
        Guid contentTypeId, LanguageCode languageCode, string slug, CancellationToken cancellationToken = default) =>
        dbContext.ContentCategories
            .Where(c => c.ContentTypeId == contentTypeId)
            .FirstOrDefaultAsync(c => c.Translations.Any(t => t.LanguageCode == languageCode && t.Slug == slug), cancellationToken);

    public Task<bool> SlugExistsAsync(
        Guid contentTypeId, LanguageCode languageCode, string slug, Guid? excludeId, CancellationToken cancellationToken = default) =>
        dbContext.ContentCategories
            .Where(c => c.ContentTypeId == contentTypeId && (excludeId == null || c.Id != excludeId.Value))
            .AnyAsync(c => c.Translations.Any(t => t.LanguageCode == languageCode && t.Slug == slug), cancellationToken);

    public Task<int> CountChildrenAsync(Guid categoryId, CancellationToken cancellationToken = default) =>
        dbContext.ContentCategories.CountAsync(c => c.ParentId == categoryId, cancellationToken);

    public void Add(ContentCategory category) => dbContext.ContentCategories.Add(category);

    public void Remove(ContentCategory category) => dbContext.ContentCategories.Remove(category);
}
