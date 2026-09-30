using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.Modules.Website.Application.Media;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicContentById;

// ADR-024 §17 (Faz 1b Görev 7): unlike GetContentPreview (Görev 6), this DOES enforce the item's own
// and every ancestor's visibility - a Draft/Unpublished item, or one whose ancestor is not visible,
// 404s here exactly as ADR-024 §4.4 defines "visible on the public site" ("içerik ancak kendisi ve
// tüm ataları görünürse görünür").
public sealed class GetPublicContentByIdQueryHandler(
    IContentItemRepository contentItemRepository,
    IContentTypeRepository contentTypeRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ISiteSettingsRepository siteSettingsRepository,
    IMediaAssetRepository mediaAssetRepository,
    IFileStorageService fileStorageService,
    IVideoRepository videoRepository,
    IContentCategoryRepository contentCategoryRepository,
    IContentTagRepository contentTagRepository,
    RelatedContentResolutionService relatedContentResolutionService,
    ContentPathCascadeService contentPathCascadeService,
    ICacheService cacheService,
    TimeProvider timeProvider)
    : IRequestHandler<GetPublicContentByIdQuery, Result<PublicContentDetailResponse>>
{
    private static readonly Error NotFoundError = Error.NotFound("ContentItem.NotFound", "This content item could not be found.");

    public async Task<Result<PublicContentDetailResponse>> Handle(GetPublicContentByIdQuery request, CancellationToken cancellationToken)
    {
        var contentItem = await contentItemRepository.GetByIdAsync(request.Id, cancellationToken);
        if (contentItem is null)
        {
            return Result.Failure<PublicContentDetailResponse>(NotFoundError);
        }

        var contentType = await contentTypeRepository.GetByIdAsync(contentItem.ContentTypeId, cancellationToken);
        if (contentType is null || !contentType.IsActive || !contentType.HasDetailPage)
        {
            return Result.Failure<PublicContentDetailResponse>(NotFoundError);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (!await contentPathCascadeService.IsVisibleWithAncestorsAsync(contentItem, now, cancellationToken))
        {
            return Result.Failure<PublicContentDetailResponse>(NotFoundError);
        }

        var activeLanguages = await siteLanguageRepository.GetActiveAsync(cancellationToken);
        var resolvedLanguage = (!string.IsNullOrWhiteSpace(request.Lang)
            ? activeLanguages.FirstOrDefault(l => string.Equals(l.Code.Value, request.Lang, StringComparison.OrdinalIgnoreCase))
            : null) ?? activeLanguages.First(l => l.IsDefault);
        var defaultLanguage = activeLanguages.First(l => l.IsDefault);

        var translation = contentItem.Translations.FirstOrDefault(t => t.LanguageCode == resolvedLanguage.Code);
        if (translation is null)
        {
            return Result.Failure<PublicContentDetailResponse>(NotFoundError);
        }

        var ancestorChain = await GetAncestorChainAsync(contentItem, cancellationToken);

        var transitions = new List<DateTime?> { contentItem.PublishAtUtc, contentItem.UnpublishAtUtc };
        foreach (var ancestor in ancestorChain)
        {
            transitions.Add(ancestor.PublishAtUtc);
            transitions.Add(ancestor.UnpublishAtUtc);
        }

        var ttl = ContentCacheTtlCalculator.Calculate(now, transitions);
        var cacheKey = WebsiteCacheKeys.PublicContentDetail(contentItem.Id, resolvedLanguage.Code.Value);

        var response = await cacheService.GetOrCreateAsync(
            cacheKey,
            ct => BuildResponseAsync(contentItem, contentType, translation, ancestorChain, resolvedLanguage, defaultLanguage, activeLanguages, now, ct),
            ttl,
            cancellationToken);

        return Result.Success(response);
    }

    private async Task<PublicContentDetailResponse> BuildResponseAsync(
        ContentItem contentItem, ContentType contentType, ContentItemTranslation translation, IReadOnlyList<ContentItem> ancestorChain,
        SiteLanguage resolvedLanguage, SiteLanguage defaultLanguage, IReadOnlyList<SiteLanguage> activeLanguages, DateTime now,
        CancellationToken cancellationToken)
    {
        var languageCode = resolvedLanguage.Code;
        var path = RoutePathFormat.BuildPublicPath(resolvedLanguage.Code.Value, defaultLanguage.Code.Value, translation.FullPath);

        var coverImage = await BuildImageAsync(contentItem.CoverImageMediaId, cancellationToken);
        var detailImage = contentType.SupportsDetailImage ? await BuildImageAsync(contentItem.DetailImageMediaId, cancellationToken) : null;

        var gallery = contentType.SupportsGallery ? await BuildGalleryAsync(contentItem, languageCode, cancellationToken) : [];
        var videos = contentType.SupportsVideos ? await BuildVideosAsync(contentItem.VideoIds, languageCode, cancellationToken) : [];
        var attachments = contentType.SupportsAttachments ? await BuildAttachmentsAsync(contentItem, languageCode, cancellationToken) : [];
        var categories = contentType.SupportsCategories
            ? await BuildCategoriesAsync(contentItem.CategoryIds, languageCode, cancellationToken)
            : [];
        var tags = contentType.SupportsTags ? await BuildTagsAsync(translation.TagIds, cancellationToken) : [];
        var children = contentType.SupportsHierarchy
            ? await BuildChildrenAsync(contentItem.Id, languageCode, defaultLanguage.Code.Value, now, cancellationToken)
            : [];
        var related = contentType.SupportsRelatedContent
            ? await BuildRelatedAsync(contentItem, languageCode, defaultLanguage.Code.Value, now, cancellationToken)
            : [];

        var breadcrumb = BuildBreadcrumb(contentItem, contentType, translation, ancestorChain, resolvedLanguage, defaultLanguage);
        var alternates = BuildAlternates(contentItem, ancestorChain, resolvedLanguage.Code.Value, defaultLanguage.Code.Value, activeLanguages);

        var settings = await siteSettingsRepository.GetAsync(cancellationToken) ?? SiteSettings.CreateDefault();
        var settingsTranslation = settings.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        var resolvedSeo = ContentSeoResolver.Resolve(
            translation.Seo, translation.Title, translation.Summary, path, contentItem.DetailImageMediaId, contentItem.CoverImageMediaId,
            settings.DefaultOgImageMediaId, settingsTranslation?.DefaultMetaDescription ?? string.Empty);
        var ogImage = await BuildImageAsync(resolvedSeo.OgImageMediaId, cancellationToken);
        var seo = new PublicContentDetailSeoResponse(
            resolvedSeo.MetaTitle, resolvedSeo.MetaDescription, resolvedSeo.OgTitle, resolvedSeo.OgDescription, ogImage?.Original,
            resolvedSeo.CanonicalUrl, resolvedSeo.NoIndex);

        return new PublicContentDetailResponse(
            contentItem.Id, contentType.Key.Value, contentType.DetailTemplate, translation.Title, translation.Summary, translation.Body, path,
            contentItem.PublishAtUtc ?? contentItem.PublishedAtUtc ?? now, contentItem.UpdatedAtUtc, coverImage, detailImage, gallery, videos,
            attachments, categories, tags, children, related, breadcrumb, alternates, seo);
    }

    private async Task<IReadOnlyList<ContentItem>> GetAncestorChainAsync(ContentItem item, CancellationToken cancellationToken)
    {
        var chain = new List<ContentItem>();
        var current = item;
        while (current.ParentId is not null)
        {
            var parent = await contentItemRepository.GetByIdAsync(current.ParentId.Value, cancellationToken);
            if (parent is null)
            {
                break;
            }

            chain.Add(parent);
            current = parent;
        }

        chain.Reverse();
        return chain;
    }

    private IReadOnlyList<PublicContentBreadcrumbItemResponse> BuildBreadcrumb(
        ContentItem contentItem, ContentType contentType, ContentItemTranslation translation, IReadOnlyList<ContentItem> ancestorChain,
        SiteLanguage resolvedLanguage, SiteLanguage defaultLanguage)
    {
        var items = new List<PublicContentBreadcrumbItemResponse>
        {
            new("Ana Sayfa", RoutePathFormat.BuildPublicPath(resolvedLanguage.Code.Value, defaultLanguage.Code.Value, string.Empty)),
        };

        if (contentType.HasListingPage)
        {
            var typeTranslation = contentType.Translations.FirstOrDefault(t => t.LanguageCode == resolvedLanguage.Code);
            if (typeTranslation is not null)
            {
                items.Add(new PublicContentBreadcrumbItemResponse(
                    typeTranslation.Name, RoutePathFormat.BuildPublicPath(resolvedLanguage.Code.Value, defaultLanguage.Code.Value, typeTranslation.RoutePrefix)));
            }
        }

        foreach (var ancestor in ancestorChain)
        {
            var ancestorTranslation = ancestor.Translations.FirstOrDefault(t => t.LanguageCode == resolvedLanguage.Code);
            if (ancestorTranslation is not null)
            {
                items.Add(new PublicContentBreadcrumbItemResponse(
                    ancestorTranslation.Title,
                    RoutePathFormat.BuildPublicPath(resolvedLanguage.Code.Value, defaultLanguage.Code.Value, ancestorTranslation.FullPath)));
            }
        }

        items.Add(new PublicContentBreadcrumbItemResponse(
            translation.Title, RoutePathFormat.BuildPublicPath(resolvedLanguage.Code.Value, defaultLanguage.Code.Value, translation.FullPath)));

        return items;
    }

    // ADR-024 §15: the item's own visibility does not vary by language - only whether the item and
    // every ancestor actually HAS a translation in a candidate language does (mirrors
    // RouteResolutionService.BuildDetailAlternates).
    private static IReadOnlyList<PublicContentDetailAlternateResponse> BuildAlternates(
        ContentItem contentItem, IReadOnlyList<ContentItem> ancestorChain, string languageCode, string defaultLanguageCode,
        IReadOnlyList<SiteLanguage> activeLanguages)
    {
        var alternates = new List<PublicContentDetailAlternateResponse>();
        foreach (var language in activeLanguages.Where(l => l.Code.Value != languageCode))
        {
            var itemTranslation = contentItem.Translations.FirstOrDefault(t => t.LanguageCode == language.Code);
            if (itemTranslation is null || ancestorChain.Any(a => a.Translations.All(t => t.LanguageCode != language.Code)))
            {
                continue;
            }

            alternates.Add(new PublicContentDetailAlternateResponse(
                language.Code.Value, RoutePathFormat.BuildPublicPath(language.Code.Value, defaultLanguageCode, itemTranslation.FullPath)));
        }

        return alternates;
    }

    private async Task<IReadOnlyList<PublicContentDetailGalleryItemResponse>> BuildGalleryAsync(
        ContentItem contentItem, LanguageCode languageCode, CancellationToken cancellationToken)
    {
        var items = new List<PublicContentDetailGalleryItemResponse>();
        foreach (var galleryItem in contentItem.GalleryItems.OrderBy(g => g.SortOrder))
        {
            var mediaAsset = await mediaAssetRepository.GetByIdAsync(galleryItem.MediaAssetId, cancellationToken);
            if (mediaAsset is null)
            {
                continue;
            }

            var overrideTranslation = galleryItem.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
            var mediaTranslation = mediaAsset.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
            var altText = GalleryItemDisplayResolver.ResolveAltText(overrideTranslation?.AltTextOverride, mediaTranslation?.AltText);
            var caption = GalleryItemDisplayResolver.ResolveCaption(overrideTranslation?.CaptionOverride, mediaTranslation?.Caption);

            items.Add(new PublicContentDetailGalleryItemResponse(
                mediaAsset.Id, await ToImageResponseAsync(mediaAsset, cancellationToken), altText, caption));
        }

        return items;
    }

    private async Task<IReadOnlyList<PublicContentDetailVideoResponse>> BuildVideosAsync(
        IReadOnlyList<Guid> videoIds, LanguageCode languageCode, CancellationToken cancellationToken)
    {
        var items = new List<PublicContentDetailVideoResponse>();
        foreach (var videoId in videoIds)
        {
            var video = await videoRepository.GetByIdAsync(videoId, cancellationToken);
            if (video is null || !video.IsActive)
            {
                continue;
            }

            var translation = video.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
            if (translation is null)
            {
                continue;
            }

            items.Add(new PublicContentDetailVideoResponse(
                video.Id, video.YouTubeVideoId.Value, video.YouTubeVideoId.BuildEmbedUrl(), video.YouTubeVideoId.BuildThumbnailUrl(),
                translation.Title, translation.Description));
        }

        return items;
    }

    private async Task<IReadOnlyList<PublicContentDetailAttachmentResponse>> BuildAttachmentsAsync(
        ContentItem contentItem, LanguageCode languageCode, CancellationToken cancellationToken)
    {
        var items = new List<PublicContentDetailAttachmentResponse>();
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

            items.Add(new PublicContentDetailAttachmentResponse(url, name, extension, mediaAsset.Original.SizeInBytes));
        }

        return items;
    }

    private async Task<IReadOnlyList<PublicContentDetailCategoryResponse>> BuildCategoriesAsync(
        IReadOnlyList<Guid> categoryIds, LanguageCode languageCode, CancellationToken cancellationToken)
    {
        var items = new List<PublicContentDetailCategoryResponse>();
        foreach (var categoryId in categoryIds)
        {
            var category = await contentCategoryRepository.GetByIdAsync(categoryId, cancellationToken);
            if (category is null || !category.IsActive)
            {
                continue;
            }

            var translation = category.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
            if (translation is not null)
            {
                items.Add(new PublicContentDetailCategoryResponse(category.Id, translation.Name, translation.Slug));
            }
        }

        return items;
    }

    private async Task<IReadOnlyList<string>> BuildTagsAsync(IReadOnlyList<Guid> tagIds, CancellationToken cancellationToken)
    {
        var names = new List<string>();
        foreach (var tagId in tagIds)
        {
            var tag = await contentTagRepository.GetByIdAsync(tagId, cancellationToken);
            if (tag is not null)
            {
                names.Add(tag.Name);
            }
        }

        return names;
    }

    private async Task<IReadOnlyList<PublicContentChildResponse>> BuildChildrenAsync(
        Guid parentId, LanguageCode languageCode, string defaultLanguageCode, DateTime now, CancellationToken cancellationToken)
    {
        var children = await contentItemRepository.GetChildrenAsync(parentId, cancellationToken);
        var items = new List<PublicContentChildResponse>();
        foreach (var child in children.Where(c => c.IsVisible(now)).OrderBy(c => c.SortOrder))
        {
            var translation = child.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
            if (translation is null)
            {
                continue;
            }

            var coverImage = await BuildImageAsync(child.CoverImageMediaId, cancellationToken);
            var path = RoutePathFormat.BuildPublicPath(languageCode.Value, defaultLanguageCode, translation.FullPath);
            items.Add(new PublicContentChildResponse(child.Id, translation.Title, translation.Summary, path, coverImage));
        }

        return items;
    }

    private async Task<IReadOnlyList<PublicContentRelatedItemResponse>> BuildRelatedAsync(
        ContentItem contentItem, LanguageCode languageCode, string defaultLanguageCode, DateTime now, CancellationToken cancellationToken)
    {
        var candidates = await relatedContentResolutionService.ResolveAsync(
            contentItem.Id, contentItem.ContentTypeId, contentItem.RelatedContentItemIds, contentItem.CategoryIds, languageCode, now,
            cancellationToken);

        var items = new List<PublicContentRelatedItemResponse>();
        foreach (var candidate in candidates)
        {
            var image = await BuildImageAsync(candidate.CoverImageMediaId, cancellationToken);
            var path = RoutePathFormat.BuildPublicPath(languageCode.Value, defaultLanguageCode, candidate.FullPath);
            items.Add(new PublicContentRelatedItemResponse(candidate.Id, candidate.Title, candidate.Summary, path, image?.Original));
        }

        return items;
    }

    private async Task<PublicContentDetailImageResponse?> BuildImageAsync(Guid? mediaAssetId, CancellationToken cancellationToken)
    {
        if (mediaAssetId is null)
        {
            return null;
        }

        var mediaAsset = await mediaAssetRepository.GetByIdAsync(mediaAssetId.Value, cancellationToken);
        return mediaAsset is null ? null : await ToImageResponseAsync(mediaAsset, cancellationToken);
    }

    private async Task<PublicContentDetailImageResponse> ToImageResponseAsync(MediaAsset mediaAsset, CancellationToken cancellationToken)
    {
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

        return new PublicContentDetailImageResponse(small, medium, large, originalUrl);
    }
}
