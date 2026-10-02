using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeContentCategoryRepository : IContentCategoryRepository
{
    private readonly List<ContentCategory> _categories = [];

    public void Seed(ContentCategory category) => _categories.Add(category);

    public Task<ContentCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_categories.FirstOrDefault(c => c.Id == id));

    public Task<IReadOnlyList<ContentCategory>> GetByContentTypeIdAsync(Guid contentTypeId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ContentCategory>>(_categories.Where(c => c.ContentTypeId == contentTypeId).ToList());

    public Task<ContentCategory?> GetByTypeAndSlugAsync(
        Guid contentTypeId, LanguageCode languageCode, string slug, CancellationToken cancellationToken = default) =>
        Task.FromResult(_categories.FirstOrDefault(
            c => c.ContentTypeId == contentTypeId && c.Translations.Any(t => t.LanguageCode == languageCode && t.Slug == slug)));

    public Task<bool> SlugExistsAsync(
        Guid contentTypeId, LanguageCode languageCode, string slug, Guid? excludeId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_categories
            .Where(c => c.ContentTypeId == contentTypeId && (excludeId == null || c.Id != excludeId.Value))
            .Any(c => c.Translations.Any(t => t.LanguageCode == languageCode && t.Slug == slug)));

    public Task<int> CountChildrenAsync(Guid categoryId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_categories.Count(c => c.ParentId == categoryId));

    public void Add(ContentCategory category) => _categories.Add(category);

    public void Remove(ContentCategory category) => _categories.RemoveAll(c => c.Id == category.Id);
}
