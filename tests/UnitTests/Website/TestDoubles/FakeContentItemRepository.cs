using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeContentItemRepository : IContentItemRepository
{
    private readonly List<ContentItem> _contentItems = [];

    public bool FullPathExistsResult { get; set; }

    public void Seed(ContentItem contentItem) => _contentItems.Add(contentItem);

    public Task<ContentItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_contentItems.FirstOrDefault(c => c.Id == id));

    public Task<PagedResult<ContentItemListItem>> SearchAsync(
        Guid? contentTypeId, ContentItemStatus? status, LanguageCode languageCode, bool requireLanguage, string? search,
        bool? isFeatured, Guid? parentId, PagedRequest pagedRequest, CancellationToken cancellationToken = default) =>
        Task.FromResult(new PagedResult<ContentItemListItem>([], 0, pagedRequest.Page, pagedRequest.PageSize));

    public Task<bool> FullPathExistsAsync(LanguageCode languageCode, string fullPath, Guid? excludeId, CancellationToken cancellationToken = default) =>
        Task.FromResult(FullPathExistsResult);

    public Task<IReadOnlyList<ContentItem>> GetByMediaAssetIdAsync(Guid mediaAssetId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ContentItem>>([]);

    public Task<IReadOnlyList<ContentItem>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ContentItem>>([]);

    public Task<int> CountPublishedChildrenAsync(Guid parentId, CancellationToken cancellationToken = default) =>
        Task.FromResult(0);

    public Task<IReadOnlyList<ContentItem>> GetRootItemsByContentTypeIdAsync(Guid contentTypeId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ContentItem>>([]);

    public void Add(ContentItem contentItem) => _contentItems.Add(contentItem);
}
