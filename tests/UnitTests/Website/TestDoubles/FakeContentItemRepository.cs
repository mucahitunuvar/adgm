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

    public Task<IReadOnlyList<ContentItem>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ContentItem>>(_contentItems.Where(c => ids.Contains(c.Id)).ToList());

    public Task<PagedResult<ContentItemListItem>> SearchAsync(
        Guid? contentTypeId,
        ContentItemStatus? status,
        LanguageCode languageCode,
        bool requireLanguage,
        string? search,
        bool? isFeatured,
        Guid? parentId,
        IReadOnlyList<Guid>? categoryIds,
        Guid? tagId,
        DateTime? publishedFromUtc,
        DateTime? publishedToUtc,
        DateTime? createdFromUtc,
        DateTime? createdToUtc,
        ContentItemSortField? orderBy,
        ContentItemSortDirection direction,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        var query = _contentItems.Where(c => c.ParentId == parentId && c.DeletedAtUtc is null);

        if (contentTypeId is not null)
        {
            query = query.Where(c => c.ContentTypeId == contentTypeId.Value);
        }

        if (status is not null)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (isFeatured is not null)
        {
            query = query.Where(c => c.IsFeatured == isFeatured.Value);
        }

        if (categoryIds is not null)
        {
            query = categoryIds.Count == 0 ? [] : query.Where(c => c.CategoryIds.Any(categoryIds.Contains));
        }

        if (tagId is not null)
        {
            query = query.Where(c => c.Translations.Any(t => t.LanguageCode == languageCode && t.TagIds.Contains(tagId.Value)));
        }

        if (publishedFromUtc is not null)
        {
            query = query.Where(c => (c.PublishAtUtc ?? c.PublishedAtUtc) >= publishedFromUtc.Value);
        }

        if (publishedToUtc is not null)
        {
            query = query.Where(c => (c.PublishAtUtc ?? c.PublishedAtUtc) < publishedToUtc.Value);
        }

        if (createdFromUtc is not null)
        {
            query = query.Where(c => c.CreatedAtUtc >= createdFromUtc.Value);
        }

        if (createdToUtc is not null)
        {
            query = query.Where(c => c.CreatedAtUtc < createdToUtc.Value);
        }

        var projected = query.Select(c => new
        {
            Item = c,
            Title = c.Translations.Where(t => t.LanguageCode == languageCode).Select(t => t.Title).FirstOrDefault(),
            EffectivePublishAtUtc = c.PublishAtUtc ?? c.PublishedAtUtc,
        });

        if (requireLanguage)
        {
            projected = projected.Where(x => x.Title != null);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            projected = projected.Where(x => x.Title != null && x.Title.Contains(search, StringComparison.Ordinal));
        }

        var ordered = (orderBy, direction) switch
        {
            (ContentItemSortField.Title, ContentItemSortDirection.Desc) => projected.OrderByDescending(x => x.Title),
            (ContentItemSortField.Title, _) => projected.OrderBy(x => x.Title),
            (ContentItemSortField.CreatedAt, ContentItemSortDirection.Desc) => projected.OrderByDescending(x => x.Item.CreatedAtUtc),
            (ContentItemSortField.CreatedAt, _) => projected.OrderBy(x => x.Item.CreatedAtUtc),
            (ContentItemSortField.UpdatedAt, ContentItemSortDirection.Desc) =>
                projected.OrderBy(x => x.Item.UpdatedAtUtc is null).ThenByDescending(x => x.Item.UpdatedAtUtc),
            (ContentItemSortField.UpdatedAt, _) => projected.OrderBy(x => x.Item.UpdatedAtUtc is null).ThenBy(x => x.Item.UpdatedAtUtc),
            (ContentItemSortField.PublishAt, ContentItemSortDirection.Desc) =>
                projected.OrderBy(x => x.EffectivePublishAtUtc is null).ThenByDescending(x => x.EffectivePublishAtUtc),
            (ContentItemSortField.PublishAt, _) =>
                projected.OrderBy(x => x.EffectivePublishAtUtc is null).ThenBy(x => x.EffectivePublishAtUtc),
            (ContentItemSortField.SortOrder, ContentItemSortDirection.Desc) => projected.OrderByDescending(x => x.Item.SortOrder),
            (ContentItemSortField.SortOrder, _) => projected.OrderBy(x => x.Item.SortOrder),
            (null, _) => projected.OrderBy(x => x.Item.SortOrder),
            _ => throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, "Unrecognized content item sort field."),
        };

        var items = ordered.ThenBy(x => x.Item.Id).ToList();
        var page = items
            .Skip((pagedRequest.Page - 1) * pagedRequest.PageSize)
            .Take(pagedRequest.PageSize)
            .Select(x => new ContentItemListItem(
                x.Item.Id, x.Item.ContentTypeId, x.Item.ParentId, x.Title ?? string.Empty, x.Item.Status.ToString(), x.Item.SortOrder,
                x.Item.IsFeatured, x.Item.PublishAtUtc, x.Item.UnpublishAtUtc, x.Item.RowVersion))
            .ToList();

        return Task.FromResult(new PagedResult<ContentItemListItem>(page, items.Count, pagedRequest.Page, pagedRequest.PageSize));
    }

    public Task<bool> FullPathExistsAsync(LanguageCode languageCode, string fullPath, Guid? excludeId, CancellationToken cancellationToken = default) =>
        Task.FromResult(FullPathExistsResult);

    public Task<ContentItem?> GetByFullPathAsync(LanguageCode languageCode, string fullPath, CancellationToken cancellationToken = default) =>
        Task.FromResult(_contentItems.FirstOrDefault(
            c => c.Translations.Any(t => t.LanguageCode == languageCode && t.FullPath == fullPath)));

    public Task<IReadOnlyList<ContentItem>> GetByMediaAssetIdAsync(Guid mediaAssetId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ContentItem>>([]);

    public Task<IReadOnlyList<ContentItem>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ContentItem>>(_contentItems.Where(c => c.ParentId == parentId).ToList());

    public Task<int> CountPublishedChildrenAsync(Guid parentId, CancellationToken cancellationToken = default) =>
        Task.FromResult(0);

    public Task<IReadOnlyList<ContentItem>> GetRootItemsByContentTypeIdAsync(Guid contentTypeId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ContentItem>>([]);

    public Task<IReadOnlyList<ContentItem>> GetByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ContentItem>>(_contentItems.Where(c => c.Translations.Any(t => t.TagIds.Contains(tagId))).ToList());

    public Task<int> CountByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_contentItems.Count(c => c.CategoryIds.Contains(categoryId)));

    public Task<IReadOnlyList<ContentItem>> GetByVideoIdAsync(Guid videoId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ContentItem>>(_contentItems.Where(c => c.VideoIds.Contains(videoId)).ToList());

    public Task<IReadOnlyList<ContentItem>> GetByFormDefinitionIdAsync(Guid formDefinitionId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ContentItem>>(_contentItems.Where(c => c.FormDefinitionId == formDefinitionId).ToList());

    public Task<IReadOnlyList<RelatedContentCandidate>> GetVisibleRelatedCandidatesByIdsAsync(
        IReadOnlyList<Guid> ids, LanguageCode languageCode, DateTime now, CancellationToken cancellationToken = default)
    {
        var results = _contentItems
            .Where(c => ids.Contains(c.Id) && c.IsVisible(now))
            .Select(c => (Item: c, Translation: c.Translations.FirstOrDefault(t => t.LanguageCode == languageCode)))
            .Where(x => x.Translation is not null)
            .Select(x => ToCandidate(x.Item, x.Translation!))
            .ToList();

        return Task.FromResult<IReadOnlyList<RelatedContentCandidate>>(results);
    }

    public Task<IReadOnlyList<RelatedContentCandidate>> SearchRelatedCandidatesAsync(
        Guid contentTypeId, Guid excludeId, IReadOnlyList<Guid>? categoryIds, LanguageCode languageCode, DateTime now, int take,
        CancellationToken cancellationToken = default)
    {
        var query = _contentItems.Where(c => c.ContentTypeId == contentTypeId && c.Id != excludeId && c.IsVisible(now));
        if (categoryIds is { Count: > 0 })
        {
            query = query.Where(c => c.CategoryIds.Any(categoryIds.Contains));
        }

        var results = query
            .Select(c => (Item: c, Translation: c.Translations.FirstOrDefault(t => t.LanguageCode == languageCode)))
            .Where(x => x.Translation is not null)
            .OrderByDescending(x => x.Item.PublishAtUtc ?? x.Item.PublishedAtUtc)
            .Take(take)
            .Select(x => ToCandidate(x.Item, x.Translation!))
            .ToList();

        return Task.FromResult<IReadOnlyList<RelatedContentCandidate>>(results);
    }

    private static RelatedContentCandidate ToCandidate(ContentItem item, ContentItemTranslation translation) =>
        new(item.Id, translation.Title, translation.Summary, translation.FullPath, item.CoverImageMediaId,
            item.PublishAtUtc ?? item.PublishedAtUtc ?? DateTime.MinValue);

    public Task<PagedResult<ContentItemTrashListItem>> SearchTrashedAsync(
        LanguageCode languageCode, PagedRequest pagedRequest, CancellationToken cancellationToken = default) =>
        Task.FromResult(new PagedResult<ContentItemTrashListItem>([], 0, pagedRequest.Page, pagedRequest.PageSize));

    public Task<IReadOnlyList<ContentItem>> GetTrashedOlderThanAsync(DateTime threshold, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ContentItem>>(_contentItems.Where(c => c.DeletedAtUtc is not null && c.DeletedAtUtc < threshold).ToList());

    public Task<IReadOnlyList<ContentItem>> GetByRelatedContentItemIdAsync(Guid relatedContentItemId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ContentItem>>(_contentItems.Where(c => c.RelatedContentItemIds.Contains(relatedContentItemId)).ToList());

    public Task<PagedResult<PublicContentListItemCandidate>> SearchPublicListAsync(
        Guid contentTypeId,
        LanguageCode languageCode,
        IReadOnlyList<Guid>? categoryIds,
        Guid? tagId,
        string? search,
        DateTime? from,
        DateTime? to,
        bool? featured,
        ContentTypeSortMode sortMode,
        DateTime now,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        var query = _contentItems.Where(c =>
            c.ContentTypeId == contentTypeId && c.IsVisible(now) && c.Translations.Any(t => t.LanguageCode == languageCode));

        if (categoryIds is { Count: > 0 })
        {
            query = query.Where(c => c.CategoryIds.Any(categoryIds.Contains));
        }

        if (tagId is not null)
        {
            query = query.Where(c => c.Translations.Any(t => t.LanguageCode == languageCode && t.TagIds.Contains(tagId.Value)));
        }

        if (featured is not null)
        {
            query = query.Where(c => c.IsFeatured == featured.Value);
        }

        var candidates = query
            .Select(c =>
            {
                var translation = c.Translations.First(t => t.LanguageCode == languageCode);
                var effectiveDate = c.PublishAtUtc ?? c.PublishedAtUtc ?? DateTime.MinValue;
                return new PublicContentListItemCandidate(
                    c.Id, translation.Title, translation.Summary, translation.Body, translation.FullPath, c.CoverImageMediaId,
                    c.DetailImageMediaId, c.PublishAtUtc, c.UnpublishAtUtc, effectiveDate, c.IsFeatured, c.CategoryIds, translation.Seo);
            })
            .Where(x => string.IsNullOrWhiteSpace(search) || x.Title.Contains(search) || x.Summary.Contains(search))
            .Where(x => from is null || x.EffectivePublishDate >= from.Value)
            .Where(x => to is null || x.EffectivePublishDate <= to.Value);

        var ordered = sortMode == ContentTypeSortMode.Manual
            ? candidates.OrderBy(x => x.Title)
            : candidates.OrderByDescending(x => x.EffectivePublishDate);

        var items = ordered.ToList();
        return Task.FromResult(new PagedResult<PublicContentListItemCandidate>(items, items.Count, pagedRequest.Page, pagedRequest.PageSize));
    }

    public Task<DateTime?> GetEarliestUpcomingTransitionAsync(Guid contentTypeId, DateTime now, CancellationToken cancellationToken = default)
    {
        var visibleOfType = _contentItems.Where(c =>
            c.ContentTypeId == contentTypeId && c.DeletedAtUtc is null && c.Status == ContentItemStatus.Published);

        var upcoming = visibleOfType
            .SelectMany(c => new DateTime?[] { c.PublishAtUtc, c.UnpublishAtUtc })
            .Where(t => t > now)
            .OrderBy(t => t)
            .FirstOrDefault();

        return Task.FromResult(upcoming);
    }

    public Task<DateTime?> GetEarliestUpcomingTransitionAsync(DateTime now, CancellationToken cancellationToken = default)
    {
        var visible = _contentItems.Where(c => c.DeletedAtUtc is null && c.Status == ContentItemStatus.Published);

        var upcoming = visible
            .SelectMany(c => new DateTime?[] { c.PublishAtUtc, c.UnpublishAtUtc })
            .Where(t => t > now)
            .OrderBy(t => t)
            .FirstOrDefault();

        return Task.FromResult(upcoming);
    }

    public void Add(ContentItem contentItem) => _contentItems.Add(contentItem);

    public void Remove(ContentItem contentItem) => _contentItems.Remove(contentItem);
}
