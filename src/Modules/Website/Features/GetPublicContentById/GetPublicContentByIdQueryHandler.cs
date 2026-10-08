using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.BlockTypes.PublicResolution;
using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.Modules.Website.Application.Forms.PublicResolution;
using GenclikMerkezi.Modules.Website.Application.Media;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Configuration;

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
    IPageLayoutRepository pageLayoutRepository,
    PublicPageLayoutResolver publicPageLayoutResolver,
    ISliderRepository sliderRepository,
    IFormDefinitionRepository formDefinitionRepository,
    PublicFormDefinitionResolver publicFormDefinitionResolver,
    IEventScheduleRepository eventScheduleRepository,
    ICacheService cacheService,
    IConfiguration configuration,
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

        // Faz 2 Görev 5 master prompt §5.3: when this type carries a block layout, its cached response
        // can also depend on other content's/sliders' schedules (hero-slider, content-list, ...) -
        // reusing the site-wide earliest-upcoming-transition calculation Görev 1's public site cache
        // already computes is enough ("Pratikte Görev 1'deki site geneli en yakın zamanlama hesabını
        // kullanmak yeterlidir").
        if (contentType.SupportsBlockLayout)
        {
            transitions.Add(await contentItemRepository.GetEarliestUpcomingTransitionAsync(now, cancellationToken));
            transitions.Add(await sliderRepository.GetEarliestUpcomingSlideTransitionAsync(now, cancellationToken));
        }

        var ttl = ContentCacheTtlCalculator.Calculate(now, transitions);
        var cacheKey = WebsiteCacheKeys.PublicContentDetail(contentItem.Id, resolvedLanguage.Code.Value);

        var response = await cacheService.GetOrCreateAsync(
            cacheKey,
            ct => BuildResponseAsync(contentItem, contentType, translation, ancestorChain, resolvedLanguage, defaultLanguage, activeLanguages, now, ct),
            ttl,
            cancellationToken);

        if (response.Event is not null)
        {
            response = response with { Event = await ResolveLiveEventFieldsAsync(response.Event, contentItem.Id, now, cancellationToken) };
        }

        return Result.Success(response);
    }

    // ADR-024 §17 (Faz 4 Görev 2): §1 "kontenjan/durum alanları cache'lenmez" - RegistrationState and
    // RemainingSpots are never read from the cached response above; they are recomputed here, every
    // request, from an uncached lightweight projection (GetRegistrationStateInputsByContentItemIdAsync).
    private async Task<PublicContentDetailEventResponse> ResolveLiveEventFieldsAsync(
        PublicContentDetailEventResponse cachedEvent, Guid contentItemId, DateTime now, CancellationToken cancellationToken)
    {
        var inputs = await eventScheduleRepository.GetRegistrationStateInputsByContentItemIdAsync(contentItemId, cancellationToken);
        if (inputs is null)
        {
            return cachedEvent;
        }

        var state = EventRegistrationStateResolver.Resolve(
            inputs.IsCancelled, inputs.RegistrationEnabled, inputs.RegistrationOpensAtUtc, inputs.RegistrationClosesAtUtc, inputs.StartsAtUtc,
            inputs.Capacity, inputs.ConfirmedCount, inputs.WaitlistEnabled, now);
        var remainingSpots = inputs.Capacity is { } capacity ? Math.Max(0, capacity - inputs.ConfirmedCount) : (int?)null;

        return cachedEvent with { RegistrationState = state.ToString(), RemainingSpots = remainingSpots };
    }

    private async Task<PublicContentDetailResponse> BuildResponseAsync(
        ContentItem contentItem, ContentType contentType, ContentItemTranslation translation, IReadOnlyList<ContentItem> ancestorChain,
        SiteLanguage resolvedLanguage, SiteLanguage defaultLanguage, IReadOnlyList<SiteLanguage> activeLanguages, DateTime now,
        CancellationToken cancellationToken)
    {
        var languageCode = resolvedLanguage.Code;
        var path = RoutePathFormat.BuildPublicPath(resolvedLanguage.Code.Value, defaultLanguage.Code.Value, translation.FullPath);
        var publicSiteBaseUrl = (configuration["Website:PublicSiteBaseUrl"] ?? string.Empty).TrimEnd('/');
        var pageAbsoluteUrl = publicSiteBaseUrl + path;
        var effectivePublishDate = contentItem.PublishAtUtc ?? contentItem.PublishedAtUtc ?? now;

        // ADR-024 §15 (Faz 5 Görev 6): moved ahead of gallery/video/etc. so both the Article/NewsArticle
        // JSON-LD object below and the final Seo response field share one resolution instead of running
        // ContentSeoResolver twice.
        var settings = await siteSettingsRepository.GetAsync(cancellationToken) ?? SiteSettings.CreateDefault();
        var settingsTranslation = settings.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        var resolvedSeo = ContentSeoResolver.Resolve(
            translation.Seo, translation.Title, translation.Summary, path, contentItem.DetailImageMediaId, contentItem.CoverImageMediaId,
            settings.DefaultOgImageMediaId, settingsTranslation?.DefaultMetaDescription ?? string.Empty);
        var ogImage = await BuildImageAsync(resolvedSeo.OgImageMediaId, cancellationToken);
        var seo = new PublicContentDetailSeoResponse(
            resolvedSeo.MetaTitle, resolvedSeo.MetaDescription, resolvedSeo.OgTitle, resolvedSeo.OgDescription, ogImage?.Original,
            resolvedSeo.CanonicalUrl, resolvedSeo.NoIndex);

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

        // Faz 2 Görev 5 master prompt §5.1: "türü SupportsBlockLayout ise ve yayındaki düzeni varsa
        // blocks alanı eklenir" - omitted (null) for every other type, or when the type supports block
        // layouts but this specific item never got one.
        IReadOnlyList<PublicLayoutBlockResponse>? blocks = null;
        if (contentType.SupportsBlockLayout)
        {
            var layout = await pageLayoutRepository.GetByContentItemIdAsync(contentItem.Id, cancellationToken);
            if (layout is not null)
            {
                blocks = await publicPageLayoutResolver.ResolveAsync(
                    layout.PublishedBlocks, languageCode, defaultLanguage.Code, now, cancellationToken);
            }
        }

        PublicFormDefinitionResponse? form = null;
        if (contentType.SupportsForm && contentItem.FormDefinitionId is not null)
        {
            var formDefinition = await formDefinitionRepository.GetByIdAsync(contentItem.FormDefinitionId.Value, cancellationToken);
            if (formDefinition is not null && formDefinition.IsActive)
            {
                form = await publicFormDefinitionResolver.ResolveAsync(formDefinition, languageCode, now, cancellationToken);
            }
        }

        // ADR-024 §11/§17 (Faz 4 Görev 2): RegistrationState/RemainingSpots are placeholders here -
        // this whole response is about to be cached, and those two fields are overwritten by the outer
        // Handle method on every request (cache hit or miss), never served from the cache itself.
        PublicContentDetailEventResponse? eventResponse = null;
        IReadOnlyDictionary<string, object?>? eventJsonLd = null;
        if (contentType.SupportsEvent)
        {
            var eventSchedule = await eventScheduleRepository.GetByContentItemIdAsync(contentItem.Id, cancellationToken);
            if (eventSchedule is not null)
            {
                var scheduleTranslation = eventSchedule.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
                eventResponse = new PublicContentDetailEventResponse(
                    eventSchedule.StartsAtUtc, eventSchedule.EndsAtUtc, eventSchedule.Format.ToString(), scheduleTranslation?.VenueName ?? string.Empty,
                    eventSchedule.IsCancelled, string.Empty, null, scheduleTranslation?.VenueAddress ?? string.Empty,
                    scheduleTranslation?.FeeInfo ?? string.Empty, scheduleTranslation?.Instructors ?? string.Empty,
                    scheduleTranslation?.ProgramFlow ?? string.Empty, scheduleTranslation?.AccessibilityNote ?? string.Empty, eventSchedule.MinAge,
                    eventSchedule.MaxAge, eventSchedule.RegistrationOpensAtUtc, eventSchedule.RegistrationClosesAtUtc);

                // ADR-024 §15/§11.3 (Faz 5 Görev 6): "Etkinlik SupportsEvent türlerinde SchemaKind'den
                // bağımsız olarak otomatik Event üretilir" - generated whenever a SupportsEvent type
                // actually has a schedule, regardless of ContentType.SchemaKind.
                eventJsonLd = StructuredDataBuilder.BuildEvent(
                    translation.Title, eventSchedule.StartsAtUtc, eventSchedule.EndsAtUtc, eventSchedule.IsCancelled, eventSchedule.Format,
                    scheduleTranslation?.VenueName ?? string.Empty, scheduleTranslation?.VenueAddress ?? string.Empty, resolvedSeo.MetaDescription,
                    ogImage?.Original, pageAbsoluteUrl);
            }
        }

        var breadcrumb = BuildBreadcrumb(contentItem, contentType, translation, ancestorChain, resolvedLanguage, defaultLanguage);
        var alternates = BuildAlternates(contentItem, ancestorChain, resolvedLanguage.Code.Value, defaultLanguage.Code.Value, activeLanguages);

        var jsonLd = await BuildJsonLdAsync(
            contentType, translation, breadcrumb, publicSiteBaseUrl, resolvedSeo, ogImage, effectivePublishDate, contentItem.UpdatedAtUtc,
            pageAbsoluteUrl, settingsTranslation, settings, eventJsonLd, cancellationToken);

        return new PublicContentDetailResponse(
            contentItem.Id, contentType.Key.Value, contentType.DetailTemplate, translation.Title, translation.Summary, translation.Body, path,
            effectivePublishDate, contentItem.UpdatedAtUtc, coverImage, detailImage, gallery, videos,
            attachments, categories, tags, children, related, breadcrumb, alternates, seo, blocks, form, eventResponse, jsonLd);
    }

    // ADR-024 §15/§1 (Faz 5 Görev 6): BreadcrumbList is always included; Article/NewsArticle is added
    // only for those two ContentSchemaKind values; Event (built by the caller, since it needs the
    // EventSchedule this method has no access to) is appended last when present.
    private async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> BuildJsonLdAsync(
        ContentType contentType, ContentItemTranslation translation, IReadOnlyList<PublicContentBreadcrumbItemResponse> breadcrumb,
        string publicSiteBaseUrl, ResolvedContentSeo resolvedSeo, PublicContentDetailImageResponse? ogImage, DateTime effectivePublishDate,
        DateTime? updatedAtUtc, string pageAbsoluteUrl, SiteSettingsTranslation? settingsTranslation, SiteSettings settings,
        IReadOnlyDictionary<string, object?>? eventJsonLd, CancellationToken cancellationToken)
    {
        var items = new List<IReadOnlyDictionary<string, object?>>
        {
            StructuredDataBuilder.BuildBreadcrumbList(breadcrumb.Select(b => (b.Title, publicSiteBaseUrl + b.Path)).ToList()),
        };

        if (contentType.SchemaKind is ContentSchemaKind.Article or ContentSchemaKind.NewsArticle)
        {
            var publisherLogo = await BuildImageAsync(settings.LogoLightMediaAssetId, cancellationToken);
            items.Add(StructuredDataBuilder.BuildArticle(
                contentType.SchemaKind, translation.Title, resolvedSeo.MetaDescription, ogImage?.Original, effectivePublishDate, updatedAtUtc,
                pageAbsoluteUrl, settingsTranslation?.SiteName ?? string.Empty, publisherLogo?.Original));
        }

        if (eventJsonLd is not null)
        {
            items.Add(eventJsonLd);
        }

        return items;
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
