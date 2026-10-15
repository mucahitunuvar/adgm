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

    public async Task<IReadOnlyList<ContentItem>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        return await dbContext.ContentItems.AsNoTracking().Where(ci => ids.Contains(ci.Id)).ToListAsync(cancellationToken);
    }

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
        // ADR-024 §4.5 (Faz 1b Görev 6): the admin content list never returns trashed items - GetContentTrash is the dedicated view for those.
        var query = dbContext.ContentItems.AsNoTracking().Where(ci => ci.ParentId == parentId && ci.DeletedAtUtc == null);

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

        // categoryIds is non-null exactly when request.CategoryId was given (GetContentItemsQueryHandler
        // expands it to self+children first) - an empty (but non-null) list means the requested
        // categoryId matched no category, which must narrow to zero rows, not "no filter".
        if (categoryIds is not null)
        {
            query = categoryIds.Count == 0
                ? query.Where(_ => false)
                : query.Where(ci => ci.CategoryIds.Any(c => categoryIds.Contains(c)));
        }

        if (tagId is not null)
        {
            query = query.Where(ci => ci.Translations.Any(t => t.LanguageCode == languageCode && t.TagIds.Contains(tagId.Value)));
        }

        // Half-open ([from, to)): matches the task's explicit contract, not the closed-interval
        // ([from, to]) convention some older filters elsewhere in this module use (e.g.
        // PersonalDataAccessLogRepository.SearchAsync).
        if (publishedFromUtc is not null)
        {
            query = query.Where(ci => (ci.PublishAtUtc ?? ci.PublishedAtUtc) >= publishedFromUtc.Value);
        }

        if (publishedToUtc is not null)
        {
            query = query.Where(ci => (ci.PublishAtUtc ?? ci.PublishedAtUtc) < publishedToUtc.Value);
        }

        if (createdFromUtc is not null)
        {
            query = query.Where(ci => ci.CreatedAtUtc >= createdFromUtc.Value);
        }

        if (createdToUtc is not null)
        {
            query = query.Where(ci => ci.CreatedAtUtc < createdToUtc.Value);
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
            ci.CreatedAtUtc,
            ci.UpdatedAtUtc,
            ci.RowVersion,
            Title = ci.Translations.Where(t => t.LanguageCode == languageCode).Select(t => t.Title).FirstOrDefault(),
            // Same "effective publish date" SearchPublicListAsync's EffectivePublishDate already
            // computes - an immediately-published item only ever has PublishedAtUtc set.
            EffectivePublishAtUtc = ci.PublishAtUtc ?? ci.PublishedAtUtc,
        });

        if (requireLanguage)
        {
            projected = projected.Where(x => x.Title != null);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            projected = projected.Where(x => x.Title != null && x.Title.Contains(search));
        }

        // orderBy == null keeps today's default (SortOrder then Id) unchanged; every branch adds its
        // own deterministic ThenBy(Id) tiebreaker for stable pagination. Nullable sort keys
        // (UpdatedAtUtc, EffectivePublishAtUtc) always sort null-last via a `== null` primary key,
        // regardless of direction - the same pattern SearchPublicListAsync's EventDateAsc sort uses.
        var ordered = (orderBy, direction) switch
        {
            (ContentItemSortField.Title, ContentItemSortDirection.Desc) => projected.OrderByDescending(x => x.Title),
            (ContentItemSortField.Title, _) => projected.OrderBy(x => x.Title),
            (ContentItemSortField.CreatedAt, ContentItemSortDirection.Desc) => projected.OrderByDescending(x => x.CreatedAtUtc),
            (ContentItemSortField.CreatedAt, _) => projected.OrderBy(x => x.CreatedAtUtc),
            (ContentItemSortField.UpdatedAt, ContentItemSortDirection.Desc) =>
                projected.OrderBy(x => x.UpdatedAtUtc == null).ThenByDescending(x => x.UpdatedAtUtc),
            (ContentItemSortField.UpdatedAt, _) => projected.OrderBy(x => x.UpdatedAtUtc == null).ThenBy(x => x.UpdatedAtUtc),
            (ContentItemSortField.PublishAt, ContentItemSortDirection.Desc) =>
                projected.OrderBy(x => x.EffectivePublishAtUtc == null).ThenByDescending(x => x.EffectivePublishAtUtc),
            (ContentItemSortField.PublishAt, _) =>
                projected.OrderBy(x => x.EffectivePublishAtUtc == null).ThenBy(x => x.EffectivePublishAtUtc),
            (ContentItemSortField.SortOrder, ContentItemSortDirection.Desc) => projected.OrderByDescending(x => x.SortOrder),
            (ContentItemSortField.SortOrder, _) => projected.OrderBy(x => x.SortOrder),
            (null, _) => projected.OrderBy(x => x.SortOrder),
            // Unreachable: GetContentItemsQueryHandler only ever passes a value EnumParsing.TryParseDefined
            // accepted. The switch is over a public enum (not a closed union), so the compiler still
            // requires this arm for exhaustiveness.
            _ => throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, "Unrecognized content item sort field."),
        };

        var paged = await ordered
            .ThenBy(x => x.Id)
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

    public async Task<IReadOnlyList<ContentItem>> GetByFormDefinitionIdAsync(Guid formDefinitionId, CancellationToken cancellationToken = default) =>
        await dbContext.ContentItems.Where(ci => ci.FormDefinitionId == formDefinitionId).ToListAsync(cancellationToken);

    // ContentItemVisibility.IsVisibleAt(now) is a single Expression<Func<ContentItem, bool>> shared by
    // every query below (and by ContentItem.IsVisible itself) - EF Core's LINQ-to-Entities provider
    // translates the expression tree it is given, which is why the rule lives there instead of a
    // private helper method (a call to one would throw "could not be translated" instead of becoming SQL).
    public async Task<IReadOnlyList<RelatedContentCandidate>> GetVisibleRelatedCandidatesByIdsAsync(
        IReadOnlyList<Guid> ids, LanguageCode languageCode, DateTime now, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        return await dbContext.ContentItems.AsNoTracking()
            .Where(ci => ids.Contains(ci.Id))
            .Where(ContentItemVisibility.IsVisibleAt(now))
            .Where(ci => ci.Translations.Any(t => t.LanguageCode == languageCode))
            .Select(ci => new RelatedContentCandidate(
                ci.Id,
                ci.Translations.Where(t => t.LanguageCode == languageCode).Select(t => t.Title).First(),
                ci.Translations.Where(t => t.LanguageCode == languageCode).Select(t => t.Summary).First(),
                ci.Translations.Where(t => t.LanguageCode == languageCode).Select(t => t.FullPath).First(),
                ci.CoverImageMediaId,
                ci.PublishAtUtc ?? ci.PublishedAtUtc ?? DateTime.MinValue))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RelatedContentCandidate>> SearchRelatedCandidatesAsync(
        Guid contentTypeId, Guid excludeId, IReadOnlyList<Guid>? categoryIds, LanguageCode languageCode, DateTime now, int take,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.ContentItems.AsNoTracking()
            .Where(ci => ci.ContentTypeId == contentTypeId && ci.Id != excludeId)
            .Where(ContentItemVisibility.IsVisibleAt(now))
            .Where(ci => ci.Translations.Any(t => t.LanguageCode == languageCode));

        if (categoryIds is { Count: > 0 })
        {
            query = query.Where(ci => ci.CategoryIds.Any(c => categoryIds.Contains(c)));
        }

        return await query
            .OrderByDescending(ci => ci.PublishAtUtc ?? ci.PublishedAtUtc)
            .Take(take)
            .Select(ci => new RelatedContentCandidate(
                ci.Id,
                ci.Translations.Where(t => t.LanguageCode == languageCode).Select(t => t.Title).First(),
                ci.Translations.Where(t => t.LanguageCode == languageCode).Select(t => t.Summary).First(),
                ci.Translations.Where(t => t.LanguageCode == languageCode).Select(t => t.FullPath).First(),
                ci.CoverImageMediaId,
                ci.PublishAtUtc ?? ci.PublishedAtUtc ?? DateTime.MinValue))
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<ContentItemTrashListItem>> SearchTrashedAsync(
        LanguageCode languageCode, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var projected = dbContext.ContentItems.AsNoTracking()
            .Where(ci => ci.DeletedAtUtc != null)
            .Select(ci => new
            {
                ci.Id,
                ci.ContentTypeId,
                ci.RowVersion,
                DeletedAtUtc = ci.DeletedAtUtc!.Value,
                Title = ci.Translations.Where(t => t.LanguageCode == languageCode).Select(t => t.Title).FirstOrDefault()
                    ?? ci.Translations.Select(t => t.Title).FirstOrDefault(),
            });

        var paged = await projected
            .OrderByDescending(x => x.DeletedAtUtc)
            .ToPagedResultAsync(pagedRequest, cancellationToken);

        var items = paged.Items
            .Select(x => new ContentItemTrashListItem(
                x.Id, x.ContentTypeId, x.Title ?? string.Empty, x.DeletedAtUtc,
                x.DeletedAtUtc.AddDays(ContentItem.TrashRetentionDays), x.RowVersion))
            .ToList();

        return new PagedResult<ContentItemTrashListItem>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }

    public async Task<IReadOnlyList<ContentItem>> GetTrashedOlderThanAsync(DateTime threshold, CancellationToken cancellationToken = default) =>
        await dbContext.ContentItems.Where(ci => ci.DeletedAtUtc != null && ci.DeletedAtUtc < threshold).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ContentItem>> GetByRelatedContentItemIdAsync(Guid relatedContentItemId, CancellationToken cancellationToken = default) =>
        await dbContext.ContentItems.Where(ci => ci.RelatedContentItemIds.Contains(relatedContentItemId)).ToListAsync(cancellationToken);

    public async Task<PagedResult<PublicContentListItemCandidate>> SearchPublicListAsync(
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
        var query = dbContext.ContentItems.AsNoTracking()
            .Where(ci => ci.ContentTypeId == contentTypeId)
            .Where(ContentItemVisibility.IsVisibleAt(now))
            .Where(ci => ci.Translations.Any(t => t.LanguageCode == languageCode));

        if (categoryIds is { Count: > 0 })
        {
            query = query.Where(ci => ci.CategoryIds.Any(c => categoryIds.Contains(c)));
        }

        if (tagId is not null)
        {
            query = query.Where(ci => ci.Translations.Any(t => t.LanguageCode == languageCode && t.TagIds.Contains(tagId.Value)));
        }

        if (featured is not null)
        {
            query = query.Where(ci => ci.IsFeatured == featured.Value);
        }

        var projected = query.Select(ci => new
        {
            ci.Id,
            ci.CoverImageMediaId,
            ci.DetailImageMediaId,
            ci.PublishAtUtc,
            ci.UnpublishAtUtc,
            ci.PublishedAtUtc,
            ci.IsFeatured,
            ci.CategoryIds,
            ci.SortOrder,
            Title = ci.Translations.Where(t => t.LanguageCode == languageCode).Select(t => t.Title).First(),
            Summary = ci.Translations.Where(t => t.LanguageCode == languageCode).Select(t => t.Summary).First(),
            Body = ci.Translations.Where(t => t.LanguageCode == languageCode).Select(t => t.Body).First(),
            FullPath = ci.Translations.Where(t => t.LanguageCode == languageCode).Select(t => t.FullPath).First(),
            Seo = ci.Translations.Where(t => t.LanguageCode == languageCode).Select(t => t.Seo).First(),
        });

        if (!string.IsNullOrWhiteSpace(search))
        {
            projected = projected.Where(x => x.Title.Contains(search) || x.Summary.Contains(search));
        }

        var effectivePublishDateQuery = projected.Select(x => new
        {
            x.Id,
            x.CoverImageMediaId,
            x.DetailImageMediaId,
            x.PublishAtUtc,
            x.UnpublishAtUtc,
            x.IsFeatured,
            x.CategoryIds,
            x.SortOrder,
            x.Title,
            x.Summary,
            x.Body,
            x.FullPath,
            x.Seo,
            EffectivePublishDate = x.PublishAtUtc ?? x.PublishedAtUtc ?? DateTime.MinValue,
        });

        if (from is not null)
        {
            effectivePublishDateQuery = effectivePublishDateQuery.Where(x => x.EffectivePublishDate >= from.Value);
        }

        if (to is not null)
        {
            effectivePublishDateQuery = effectivePublishDateQuery.Where(x => x.EffectivePublishDate <= to.Value);
        }

        // Faz 4 Görev 2: EventDateAsc is a real sort now, not PublishDateDesc's stand-in - a left join
        // to EventSchedule (GroupJoin + SelectMany + DefaultIfEmpty, EF Core's standard LEFT JOIN
        // shape) so an item with no schedule yet still appears, just sorted after every item that has
        // one (StartsAtUtc == null sorts first under OrderBy(... == null), so it is used as the primary
        // key to push nulls to the end rather than throwing them in among real dates).
        var orderedQuery = sortMode switch
        {
            ContentTypeSortMode.Manual => effectivePublishDateQuery.OrderBy(x => x.SortOrder).ThenBy(x => x.Title),
            ContentTypeSortMode.EventDateAsc => effectivePublishDateQuery
                .GroupJoin(dbContext.EventSchedules, x => x.Id, es => es.ContentItemId, (x, es) => new { x, es })
                .SelectMany(g => g.es.DefaultIfEmpty(), (g, es) => new { g.x, StartsAtUtc = es == null ? (DateTime?)null : es.StartsAtUtc })
                .OrderBy(g => g.StartsAtUtc == null)
                .ThenBy(g => g.StartsAtUtc)
                .Select(g => g.x),
            _ => effectivePublishDateQuery.OrderByDescending(x => x.EffectivePublishDate),
        };

        var paged = await orderedQuery.ToPagedResultAsync(pagedRequest, cancellationToken);

        var items = paged.Items
            .Select(x => new PublicContentListItemCandidate(
                x.Id, x.Title, x.Summary, x.Body, x.FullPath, x.CoverImageMediaId, x.DetailImageMediaId, x.PublishAtUtc, x.UnpublishAtUtc,
                x.EffectivePublishDate, x.IsFeatured, x.CategoryIds, x.Seo))
            .ToList();

        return new PagedResult<PublicContentListItemCandidate>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }

    // ADR-024 §17 (Faz 1b bugfix): a single query (a UNION of the PublishAtUtc and UnpublishAtUtc
    // columns) rather than one query per column - GetPublicContentsQueryHandler runs this on every
    // request (cache hit or miss) to decide the list cache's TTL, so it stays a single lightweight
    // round trip. `> now` on a nullable column is false for both a null value and one that is already
    // in the past, so no separate null check is needed.
    public Task<DateTime?> GetEarliestUpcomingTransitionAsync(Guid contentTypeId, DateTime now, CancellationToken cancellationToken = default)
    {
        var visibleOfType = dbContext.ContentItems.AsNoTracking()
            .Where(ci => ci.ContentTypeId == contentTypeId && ci.DeletedAtUtc == null && ci.Status == ContentItemStatus.Published);

        var upcomingTransitions = visibleOfType.Where(ci => ci.PublishAtUtc > now).Select(ci => ci.PublishAtUtc!.Value)
            .Union(visibleOfType.Where(ci => ci.UnpublishAtUtc > now).Select(ci => ci.UnpublishAtUtc!.Value));

        return upcomingTransitions.OrderBy(t => t).Select(t => (DateTime?)t).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<DateTime?> GetEarliestUpcomingTransitionAsync(DateTime now, CancellationToken cancellationToken = default)
    {
        var visible = dbContext.ContentItems.AsNoTracking().Where(ci => ci.DeletedAtUtc == null && ci.Status == ContentItemStatus.Published);

        var upcomingTransitions = visible.Where(ci => ci.PublishAtUtc > now).Select(ci => ci.PublishAtUtc!.Value)
            .Union(visible.Where(ci => ci.UnpublishAtUtc > now).Select(ci => ci.UnpublishAtUtc!.Value));

        return upcomingTransitions.OrderBy(t => t).Select(t => (DateTime?)t).FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(ContentItem contentItem) => dbContext.ContentItems.Add(contentItem);

    public void Remove(ContentItem contentItem) => dbContext.ContentItems.Remove(contentItem);
}
