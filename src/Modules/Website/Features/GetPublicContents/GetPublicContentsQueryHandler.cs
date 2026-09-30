using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.Media;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicContents;

// ADR-024 §17 (Faz 1b Görev 7, TTL shortening added in a later bugfix). Cache: a `search` request is
// never cached (unbounded key space); everything else is cached under
// WebsiteCacheKeys.PublicContentList, TTL shortened to the ContentType's own earliest upcoming
// PublishAtUtc/UnpublishAtUtc transition (ContentCacheTtlCalculator, the same rule the detail endpoint
// already used) - GetEarliestUpcomingTransitionAsync runs on every request (cache hit or miss, since
// the TTL is decided before knowing which one this is) as a single lightweight query, so a scheduled
// publish/unpublish is reflected in the list within seconds rather than up to the default TTL late.
public sealed class GetPublicContentsQueryHandler(
    IContentTypeRepository contentTypeRepository,
    IContentCategoryRepository contentCategoryRepository,
    IContentTagRepository contentTagRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ISiteSettingsRepository siteSettingsRepository,
    IContentItemRepository contentItemRepository,
    IMediaAssetRepository mediaAssetRepository,
    IFileStorageService fileStorageService,
    ICacheService cacheService,
    TimeProvider timeProvider)
    : IRequestHandler<GetPublicContentsQuery, Result<PublicContentListResponse>>
{
    private const int MinSearchLength = 2;
    private const int MaxSearchLength = 100;

    public async Task<Result<PublicContentListResponse>> Handle(GetPublicContentsQuery request, CancellationToken cancellationToken)
    {
        var typeKeyResult = ContentTypeKey.Create(request.Type);
        if (typeKeyResult.IsFailure)
        {
            return Result.Failure<PublicContentListResponse>(Error.NotFound("ContentType.NotFound", $"Content type '{request.Type}' could not be found."));
        }

        var contentType = await contentTypeRepository.GetByKeyAsync(typeKeyResult.Value, cancellationToken);
        if (contentType is null || !contentType.IsActive || !contentType.HasListingPage)
        {
            return Result.Failure<PublicContentListResponse>(Error.NotFound("ContentType.NotFound", $"Content type '{request.Type}' could not be found."));
        }

        if (!string.IsNullOrWhiteSpace(request.Category) && !contentType.SupportsCategories)
        {
            return Result.Failure<PublicContentListResponse>(Error.Validation(
                "ContentType.CategoriesNotSupported", "This content type does not support category filtering."));
        }

        if (!string.IsNullOrWhiteSpace(request.Tag) && !contentType.SupportsTags)
        {
            return Result.Failure<PublicContentListResponse>(Error.Validation("ContentType.TagsNotSupported", "This content type does not support tag filtering."));
        }

        if (!string.IsNullOrWhiteSpace(request.Search) && (request.Search.Length < MinSearchLength || request.Search.Length > MaxSearchLength))
        {
            return Result.Failure<PublicContentListResponse>(Error.Validation(
                "ContentItem.SearchLengthInvalid", $"Search must be between {MinSearchLength} and {MaxSearchLength} characters."));
        }

        var activeLanguages = await siteLanguageRepository.GetActiveAsync(cancellationToken);
        var resolvedLanguage = (!string.IsNullOrWhiteSpace(request.Lang)
            ? activeLanguages.FirstOrDefault(l => string.Equals(l.Code.Value, request.Lang, StringComparison.OrdinalIgnoreCase))
            : null) ?? activeLanguages.First(l => l.IsDefault);
        var defaultLanguage = activeLanguages.First(l => l.IsDefault);

        var allCategories = contentType.SupportsCategories
            ? await contentCategoryRepository.GetByContentTypeIdAsync(contentType.Id, cancellationToken)
            : [];

        ContentCategory? selectedCategory = null;
        IReadOnlyList<Guid>? categoryIdFilter = null;
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            selectedCategory = allCategories.FirstOrDefault(c =>
                c.IsActive && c.Translations.Any(t => t.LanguageCode == resolvedLanguage.Code && t.Slug == request.Category));
            if (selectedCategory is null)
            {
                return Result.Success(EmptyResponse(contentType.ListTemplate, resolvedLanguage.Code.Value));
            }

            categoryIdFilter = [selectedCategory.Id, .. allCategories.Where(c => c.ParentId == selectedCategory.Id).Select(c => c.Id)];
        }

        Guid? tagIdFilter = null;
        if (!string.IsNullOrWhiteSpace(request.Tag))
        {
            var tag = await contentTagRepository.GetBySlugAsync(resolvedLanguage.Code, request.Tag, cancellationToken);
            if (tag is null)
            {
                return Result.Success(EmptyResponse(contentType.ListTemplate, resolvedLanguage.Code.Value));
            }

            tagIdFilter = tag.Id;
        }

        var pagedRequest = new PagedRequest { Page = request.Page, PageSize = Math.Min(request.PageSize, 50) };
        var isSearch = !string.IsNullOrWhiteSpace(request.Search);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        PagedResult<PublicContentListItemCandidate> paged;
        if (isSearch)
        {
            paged = await contentItemRepository.SearchPublicListAsync(
                contentType.Id, resolvedLanguage.Code, categoryIdFilter, tagIdFilter, request.Search, request.From, request.To,
                request.Featured, contentType.SortMode, now, pagedRequest, cancellationToken);
        }
        else
        {
            var earliestUpcomingTransition = await contentItemRepository.GetEarliestUpcomingTransitionAsync(contentType.Id, now, cancellationToken);
            var ttl = ContentCacheTtlCalculator.Calculate(now, [earliestUpcomingTransition]);

            var cacheKey = WebsiteCacheKeys.PublicContentList(
                contentType.Key.Value, resolvedLanguage.Code.Value, selectedCategory?.Id.ToString(), tagIdFilter?.ToString(),
                request.From?.ToString("O"), request.To?.ToString("O"), request.Featured, pagedRequest.Page, pagedRequest.PageSize);
            paged = await cacheService.GetOrCreateAsync(
                cacheKey,
                ct => contentItemRepository.SearchPublicListAsync(
                    contentType.Id, resolvedLanguage.Code, categoryIdFilter, tagIdFilter, null, request.From, request.To, request.Featured,
                    contentType.SortMode, now, pagedRequest, ct),
                ttl,
                cancellationToken);
        }

        var categoriesById = allCategories.ToDictionary(c => c.Id);
        var items = new List<PublicContentListItemResponse>();
        foreach (var candidate in paged.Items)
        {
            items.Add(await BuildItemAsync(candidate, resolvedLanguage.Code, categoriesById, contentType.HasDetailPage, cancellationToken));
        }

        var settings = await siteSettingsRepository.GetAsync(cancellationToken) ?? SiteSettings.CreateDefault();
        var settingsTranslation = settings.Translations.FirstOrDefault(t => t.LanguageCode == resolvedLanguage.Code);

        var seoSource = selectedCategory?.Translations.FirstOrDefault(t => t.LanguageCode == resolvedLanguage.Code)?.Seo
            ?? contentType.Translations.FirstOrDefault(t => t.LanguageCode == resolvedLanguage.Code)?.Seo
            ?? SeoMetadata.CreateEmpty();
        var pageTitle = selectedCategory?.Translations.FirstOrDefault(t => t.LanguageCode == resolvedLanguage.Code)?.Name
            ?? contentType.Translations.FirstOrDefault(t => t.LanguageCode == resolvedLanguage.Code)?.Name
            ?? contentType.Key.Value;
        var routePrefix = contentType.Translations.FirstOrDefault(t => t.LanguageCode == resolvedLanguage.Code)?.RoutePrefix ?? string.Empty;
        var categorySlug = selectedCategory?.Translations.FirstOrDefault(t => t.LanguageCode == resolvedLanguage.Code)?.Slug;
        var canonicalPath = categorySlug is null ? routePrefix : $"{routePrefix}/{categorySlug}";

        var resolvedSeo = ContentSeoResolver.Resolve(
            seoSource, pageTitle, string.Empty, canonicalPath, null, null, settings.DefaultOgImageMediaId,
            settingsTranslation?.DefaultMetaDescription ?? string.Empty);
        var ogImage = await BuildImageAsync(resolvedSeo.OgImageMediaId, cancellationToken);
        var seoResponse = new PublicContentSeoResponse(
            resolvedSeo.MetaTitle, resolvedSeo.MetaDescription, resolvedSeo.OgTitle, resolvedSeo.OgDescription, ogImage?.Original,
            resolvedSeo.CanonicalUrl, resolvedSeo.NoIndex);

        var alternates = BuildAlternates(contentType, selectedCategory, resolvedLanguage.Code.Value, defaultLanguage.Code.Value, activeLanguages);

        var categoryTree = contentType.SupportsCategories
            ? BuildCategoryTree(allCategories.Where(c => c.IsActive).ToList(), resolvedLanguage.Code)
            : [];

        var selectedCategoryResponse = selectedCategory is null
            ? null
            : new PublicContentCategoryResponse(
                selectedCategory.Id, selectedCategory.Translations.First(t => t.LanguageCode == resolvedLanguage.Code).Name,
                selectedCategory.Translations.First(t => t.LanguageCode == resolvedLanguage.Code).Slug);

        var contentTypeName = contentType.Translations.FirstOrDefault(t => t.LanguageCode == resolvedLanguage.Code)?.Name ?? contentType.Key.Value;

        return Result.Success(new PublicContentListResponse(
            contentTypeName, contentType.ListTemplate, seoResponse, alternates, categoryTree, selectedCategoryResponse,
            new PagedResult<PublicContentListItemResponse>(items, paged.TotalCount, paged.Page, paged.PageSize)));
    }

    private static PublicContentListResponse EmptyResponse(string listTemplate, string languageCode) =>
        new(
            string.Empty, listTemplate, new PublicContentSeoResponse(string.Empty, string.Empty, string.Empty, string.Empty, null, string.Empty, false),
            [], [], null, new PagedResult<PublicContentListItemResponse>([], 0, 1, PagedRequest.DefaultPageSize));

    private async Task<PublicContentListItemResponse> BuildItemAsync(
        PublicContentListItemCandidate candidate, LanguageCode languageCode, IReadOnlyDictionary<Guid, ContentCategory> categoriesById,
        bool hasDetailPage, CancellationToken cancellationToken)
    {
        var categories = candidate.CategoryIds
            .Where(categoriesById.ContainsKey)
            .Select(id => categoriesById[id])
            .Select(c => (Category: c, Translation: c.Translations.FirstOrDefault(t => t.LanguageCode == languageCode)))
            .Where(x => x.Translation is not null)
            .Select(x => new PublicContentCategoryResponse(x.Category.Id, x.Translation!.Name, x.Translation.Slug))
            .ToList();

        var coverImage = await BuildImageAsync(candidate.CoverImageMediaId, cancellationToken);

        if (hasDetailPage)
        {
            return new PublicContentListItemResponse(
                candidate.Id, candidate.Title, candidate.Summary, candidate.FullPath, coverImage, candidate.EffectivePublishDate,
                candidate.IsFeatured, categories, null, null, null);
        }

        // HasDetailPage = false types (FAQ, Team, Document) show their content inline in the list -
        // attachments are not part of the list projection (rare path), so the full aggregate is
        // loaded here instead, bounded by the page's own (<= 50) item count.
        var fullItem = await contentItemRepository.GetByIdAsync(candidate.Id, cancellationToken);
        var attachments = fullItem is null ? [] : await BuildAttachmentsAsync(fullItem, languageCode, cancellationToken);
        var detailImage = await BuildImageAsync(candidate.DetailImageMediaId, cancellationToken);

        return new PublicContentListItemResponse(
            candidate.Id, candidate.Title, candidate.Summary, null, coverImage, candidate.EffectivePublishDate, candidate.IsFeatured,
            categories, candidate.Body, detailImage, attachments);
    }

    private async Task<IReadOnlyList<PublicContentAttachmentResponse>> BuildAttachmentsAsync(
        ContentItem contentItem, LanguageCode languageCode, CancellationToken cancellationToken)
    {
        var items = new List<PublicContentAttachmentResponse>();
        foreach (var attachment in contentItem.Attachments.OrderBy(a => a.SortOrder))
        {
            var mediaAsset = await mediaAssetRepository.GetByIdAsync(attachment.MediaAssetId, cancellationToken);
            if (mediaAsset is null)
            {
                continue;
            }

            var overrideTranslation = attachment.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
            var name = AttachmentDisplayNameResolver.Resolve(overrideTranslation?.DisplayNameOverride, mediaAsset.Original.OriginalFileName);
            var url = await fileStorageService.GetUrlAsync(mediaAsset.Original.FileKey, cancellationToken);
            var extension = Path.GetExtension(mediaAsset.Original.OriginalFileName).TrimStart('.');

            items.Add(new PublicContentAttachmentResponse(url, name, extension, mediaAsset.Original.SizeInBytes));
        }

        return items;
    }

    private async Task<PublicContentImageResponse?> BuildImageAsync(Guid? mediaAssetId, CancellationToken cancellationToken)
    {
        if (mediaAssetId is null)
        {
            return null;
        }

        var mediaAsset = await mediaAssetRepository.GetByIdAsync(mediaAssetId.Value, cancellationToken);
        if (mediaAsset is null)
        {
            return null;
        }

        var originalUrl = await fileStorageService.GetUrlAsync(mediaAsset.Original.FileKey, cancellationToken);
        string? small = null;
        string? medium = null;
        string? large = null;

        foreach (var variant in mediaAsset.Variants)
        {
            var url = await fileStorageService.GetUrlAsync(variant.File.FileKey, cancellationToken);
            if (variant.VariantName == MediaAssetVariantNames.Small)
            {
                small = url;
            }
            else if (variant.VariantName == MediaAssetVariantNames.Medium)
            {
                medium = url;
            }
            else if (variant.VariantName == MediaAssetVariantNames.Large)
            {
                large = url;
            }
        }

        return new PublicContentImageResponse(small, medium, large, originalUrl);
    }

    private static IReadOnlyList<PublicContentAlternateResponse> BuildAlternates(
        ContentType contentType, ContentCategory? selectedCategory, string currentLanguageCode, string defaultLanguageCode,
        IReadOnlyList<SiteLanguage> activeLanguages)
    {
        var alternates = new List<PublicContentAlternateResponse>();
        foreach (var language in activeLanguages.Where(l => l.Code.Value != currentLanguageCode))
        {
            var typeTranslation = contentType.Translations.FirstOrDefault(t => t.LanguageCode == language.Code);
            if (typeTranslation is null)
            {
                continue;
            }

            if (selectedCategory is not null)
            {
                var categoryTranslation = selectedCategory.Translations.FirstOrDefault(t => t.LanguageCode == language.Code);
                if (categoryTranslation is null)
                {
                    continue;
                }

                var path = $"{typeTranslation.RoutePrefix}/{categoryTranslation.Slug}";
                alternates.Add(new PublicContentAlternateResponse(
                    language.Code.Value, RoutePathFormat.BuildPublicPath(language.Code.Value, defaultLanguageCode, path)));
            }
            else
            {
                alternates.Add(new PublicContentAlternateResponse(
                    language.Code.Value, RoutePathFormat.BuildPublicPath(language.Code.Value, defaultLanguageCode, typeTranslation.RoutePrefix)));
            }
        }

        return alternates;
    }

    private static IReadOnlyList<PublicContentCategoryTreeItemResponse> BuildCategoryTree(
        IReadOnlyList<ContentCategory> activeCategories, LanguageCode languageCode)
    {
        var roots = activeCategories.Where(c => c.ParentId is null).OrderBy(c => c.SortOrder).ToList();
        var childrenByParentId = activeCategories.Where(c => c.ParentId is not null).ToLookup(c => c.ParentId!.Value);

        var items = new List<PublicContentCategoryTreeItemResponse>();
        foreach (var root in roots)
        {
            var rootTranslation = root.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
            if (rootTranslation is null)
            {
                continue;
            }

            var children = new List<PublicContentCategoryTreeItemResponse>();
            foreach (var child in childrenByParentId[root.Id].OrderBy(c => c.SortOrder))
            {
                var childTranslation = child.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
                if (childTranslation is not null)
                {
                    children.Add(new PublicContentCategoryTreeItemResponse(child.Id, childTranslation.Name, childTranslation.Slug, []));
                }
            }

            items.Add(new PublicContentCategoryTreeItemResponse(root.Id, rootTranslation.Name, rootTranslation.Slug, children));
        }

        return items;
    }
}
