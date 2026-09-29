using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class ContentItemRepository(WebsiteDbContext dbContext) : IContentItemRepository
{
    public Task<ContentItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.ContentItems.FirstOrDefaultAsync(ci => ci.Id == id, cancellationToken);

    // ADR-024 §17: projects only the requested language's title (a correlated scalar subquery, not a
    // navigation load) so the list query never materializes every language's full translation - unlike
    // GetByIdAsync, which returns the whole aggregate for the single-item detail view.
    public async Task<PagedResult<ContentItemListItem>> SearchAsync(
        Guid? contentTypeId,
        ContentItemStatus? status,
        LanguageCode languageCode,
        bool requireLanguage,
        string? search,
        bool? isFeatured,
        Guid? parentId,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.ContentItems.AsNoTracking().Where(ci => ci.ParentId == parentId);

        if (contentTypeId is not null)
        {
            query = query.Where(ci => ci.ContentTypeId == contentTypeId.Value);
        }

        if (status is not null)
        {
            query = query.Where(ci => ci.Status == status.Value);
        }

        if (isFeatured is not null)
        {
            query = query.Where(ci => ci.IsFeatured == isFeatured.Value);
        }

        var projected = query.Select(ci => new
        {
            ci.Id,
            ci.ContentTypeId,
            ci.ParentId,
            ci.Status,
            ci.SortOrder,
            ci.IsFeatured,
            ci.PublishAtUtc,
            ci.UnpublishAtUtc,
            ci.RowVersion,
            Title = ci.Translations.Where(t => t.LanguageCode == languageCode).Select(t => t.Title).FirstOrDefault(),
        });

        if (requireLanguage)
        {
            projected = projected.Where(x => x.Title != null);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            projected = projected.Where(x => x.Title != null && x.Title.Contains(search));
        }

        var paged = await projected
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .ToPagedResultAsync(pagedRequest, cancellationToken);

        var items = paged.Items
            .Select(x => new ContentItemListItem(
                x.Id, x.ContentTypeId, x.ParentId, x.Title ?? string.Empty, x.Status.ToString(), x.SortOrder, x.IsFeatured,
                x.PublishAtUtc, x.UnpublishAtUtc, x.RowVersion))
            .ToList();

        return new PagedResult<ContentItemListItem>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }

    public Task<bool> FullPathExistsAsync(
        LanguageCode languageCode, string fullPath, Guid? excludeId, CancellationToken cancellationToken = default) =>
        dbContext.ContentItems
            .Where(ci => excludeId == null || ci.Id != excludeId.Value)
            .AnyAsync(ci => ci.Translations.Any(t => t.LanguageCode == languageCode && t.FullPath == fullPath), cancellationToken);

    public Task<ContentItem?> GetByFullPathAsync(LanguageCode languageCode, string fullPath, CancellationToken cancellationToken = default) =>
        dbContext.ContentItems.FirstOrDefaultAsync(
            ci => ci.Translations.Any(t => t.LanguageCode == languageCode && t.FullPath == fullPath), cancellationToken);

    public async Task<IReadOnlyList<ContentItem>> GetByMediaAssetIdAsync(Guid mediaAssetId, CancellationToken cancellationToken = default) =>
        await dbContext.ContentItems
            .Where(ci =>
                ci.CoverImageMediaId == mediaAssetId
                || ci.DetailImageMediaId == mediaAssetId
                || ci.Translations.Any(t => t.Seo.OgImageMediaId == mediaAssetId)
                || ci.GalleryItems.Any(g => g.MediaAssetId == mediaAssetId)
                || ci.Attachments.Any(a => a.MediaAssetId == mediaAssetId))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ContentItem>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default) =>
        await dbContext.ContentItems.Where(ci => ci.ParentId == parentId).ToListAsync(cancellationToken);

    public Task<int> CountPublishedChildrenAsync(Guid parentId, CancellationToken cancellationToken = default) =>
        dbContext.ContentItems.CountAsync(ci => ci.ParentId == parentId && ci.Status == ContentItemStatus.Published, cancellationToken);

    public async Task<IReadOnlyList<ContentItem>> GetRootItemsByContentTypeIdAsync(Guid contentTypeId, CancellationToken cancellationToken = default) =>
        await dbContext.ContentItems
            .Where(ci => ci.ContentTypeId == contentTypeId && ci.ParentId == null)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ContentItem>> GetByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default) =>
        await dbContext.ContentItems
            .Where(ci => ci.Translations.Any(t => t.TagIds.Contains(tagId)))
            .ToListAsync(cancellationToken);

    public Task<int> CountByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default) =>
        dbContext.ContentItems.CountAsync(ci => ci.CategoryIds.Contains(categoryId), cancellationToken);

    public async Task<IReadOnlyList<ContentItem>> GetByVideoIdAsync(Guid videoId, CancellationToken cancellationToken = default) =>
        await dbContext.ContentItems.Where(ci => ci.VideoIds.Contains(videoId)).ToListAsync(cancellationToken);

    public void Add(ContentItem contentItem) => dbContext.ContentItems.Add(contentItem);
}
