using System.Text.Json;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;
using GenclikMerkezi.Modules.Website.Application.ImpactMetrics;
using GenclikMerkezi.Modules.Website.Application.LinkTargets;
using GenclikMerkezi.Modules.Website.Application.Media;
using GenclikMerkezi.Modules.Website.Application.Partners;
using GenclikMerkezi.Modules.Website.Application.Sliders;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.PublicResolution;

// Faz 2 Görev 5 master prompt §5.2/§5.3: turns one PageLayout's block list (draft, for the admin
// preview, or published, for the public home/content-detail endpoints) into the public response shape -
// applying every hiding rule (inactive, no translation, unresolved required reference, empty list
// data, unresolved single/array link items) and resolving every block's `data` in as few round trips as
// possible: media and video ids referenced ANYWHERE in the layout are batched into one GetByIdsAsync
// call each, and every link target across quick-links/feature-mosaic/cta/video-feature/image-text is
// resolved in one LinkTargetResolver.ResolveManyAsync call (§5.3 "tüm medya ID'leri tek sorguda, tüm
// içerik link'leri tek sorguda"). Blocks whose own query is inherently per-block (content-list,
// upcoming-events, faq) are the explicitly sanctioned exception ("liste tipi bloklar kendi sorgularını
// çalıştırır; bu kabul edilebilir").
public sealed class PublicPageLayoutResolver(
    IBlockTypeRegistry blockTypeRegistry,
    IMediaAssetRepository mediaAssetRepository,
    IVideoRepository videoRepository,
    IFileStorageService fileStorageService,
    LinkTargetResolver linkTargetResolver,
    SliderPublicQueryService sliderPublicQueryService,
    PartnerPublicQueryService partnerPublicQueryService,
    ImpactMetricPublicQueryService impactMetricPublicQueryService,
    IContentTypeRepository contentTypeRepository,
    IContentItemRepository contentItemRepository)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private sealed record Candidate(LayoutBlock Block, IBlockTypeDefinition Definition, JsonElement Settings, JsonElement? Texts);

    public async Task<IReadOnlyList<PublicLayoutBlockResponse>> ResolveAsync(
        IReadOnlyList<LayoutBlock> blocks,
        LanguageCode languageCode,
        LanguageCode defaultLanguageCode,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var candidates = BuildCandidates(blocks, languageCode);
        if (candidates.Count == 0)
        {
            return [];
        }

        var (mediaById, videoById) = await LoadReferencedEntitiesAsync(candidates, languageCode, cancellationToken);
        var linkResolutions = await ResolveLinkTargetsAsync(candidates, languageCode, defaultLanguageCode, now, cancellationToken);

        var needsContentTypes = candidates.Any(c => c.Block.BlockTypeKey is "content-list" or "faq" or "upcoming-events");
        var allContentTypes = needsContentTypes
            ? await contentTypeRepository.GetAllAsync(cancellationToken)
            : [];

        var results = new List<PublicLayoutBlockResponse>();
        foreach (var candidate in candidates)
        {
            var response = await ResolveBlockAsync(
                candidate, languageCode, defaultLanguageCode, now, mediaById, videoById, linkResolutions, allContentTypes, cancellationToken);
            if (response is not null)
            {
                results.Add(response);
            }
        }

        return results;
    }

    private List<Candidate> BuildCandidates(IReadOnlyList<LayoutBlock> blocks, LanguageCode languageCode)
    {
        var candidates = new List<Candidate>();

        foreach (var block in blocks.Where(b => b.IsActive).OrderBy(b => b.SortOrder))
        {
            var definition = blockTypeRegistry.TryGet(block.BlockTypeKey);
            if (definition is null)
            {
                continue;
            }

            JsonElement? textsElement = null;
            if (definition.HasTexts)
            {
                var translation = block.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
                if (translation is null)
                {
                    continue;
                }

                textsElement = JsonSerializer.Deserialize<JsonElement>(translation.TextsJson);
            }

            var settingsElement = JsonSerializer.Deserialize<JsonElement>(block.SettingsJson);
            candidates.Add(new Candidate(block, definition, settingsElement, textsElement));
        }

        return candidates;
    }

    private async Task<(IReadOnlyDictionary<Guid, MediaAsset> MediaById, IReadOnlyDictionary<Guid, Video> VideoById)> LoadReferencedEntitiesAsync(
        IReadOnlyList<Candidate> candidates, LanguageCode languageCode, CancellationToken cancellationToken)
    {
        var imageMediaIds = new HashSet<Guid>();
        var videoIds = new HashSet<Guid>();

        foreach (var candidate in candidates)
        {
            var textsByLanguage = candidate.Texts is { } texts
                ? new Dictionary<LanguageCode, JsonElement> { [languageCode] = texts }
                : new Dictionary<LanguageCode, JsonElement>();
            var references = candidate.Definition.ExtractReferences(candidate.Settings, textsByLanguage);

            foreach (var id in references.ImageMediaIds)
            {
                imageMediaIds.Add(id);
            }

            foreach (var id in references.VideoIds)
            {
                videoIds.Add(id);
            }
        }

        var media = await mediaAssetRepository.GetByIdsAsync(imageMediaIds, cancellationToken);
        var videos = await videoRepository.GetByIdsAsync(videoIds, cancellationToken);

        return (media.ToDictionary(m => m.Id), videos.ToDictionary(v => v.Id));
    }

    private async Task<IReadOnlyDictionary<LinkTarget, LinkTargetResolution>> ResolveLinkTargetsAsync(
        IReadOnlyList<Candidate> candidates, LanguageCode languageCode, LanguageCode defaultLanguageCode, DateTime now,
        CancellationToken cancellationToken)
    {
        var targets = candidates.SelectMany(CollectLinkTargets).Distinct().ToList();
        return await linkTargetResolver.ResolveManyAsync(targets, languageCode, defaultLanguageCode, now, cancellationToken);
    }

    private static IEnumerable<LinkTarget> CollectLinkTargets(Candidate candidate)
    {
        switch (candidate.Block.BlockTypeKey)
        {
            case "quick-links":
                foreach (var item in Deserialize<QuickLinksBlockSettings>(candidate.Settings).Items)
                {
                    if (ToTarget(item.Link) is { } target)
                    {
                        yield return target;
                    }
                }

                break;

            case "feature-mosaic":
                foreach (var item in Deserialize<FeatureMosaicBlockSettings>(candidate.Settings).Items)
                {
                    if (ToTarget(item.Link) is { } target)
                    {
                        yield return target;
                    }
                }

                break;

            case "cta":
                foreach (var button in Deserialize<CtaBlockSettings>(candidate.Settings).Buttons)
                {
                    if (ToTarget(button.Link) is { } target)
                    {
                        yield return target;
                    }
                }

                break;

            case "video-feature" when candidate.Texts is { } videoTexts:
                var videoFeatureTexts = Deserialize<VideoFeatureBlockTexts>(videoTexts);
                if (ToTarget(videoFeatureTexts.Link) is { } videoTarget)
                {
                    yield return videoTarget;
                }

                break;

            case "image-text":
                var imageTextSettings = Deserialize<ImageTextBlockSettings>(candidate.Settings);
                if (ToTarget(imageTextSettings.Link) is { } imageTextTarget)
                {
                    yield return imageTextTarget;
                }

                break;
        }
    }

    private async Task<PublicLayoutBlockResponse?> ResolveBlockAsync(
        Candidate candidate,
        LanguageCode languageCode,
        LanguageCode defaultLanguageCode,
        DateTime now,
        IReadOnlyDictionary<Guid, MediaAsset> mediaById,
        IReadOnlyDictionary<Guid, Video> videoById,
        IReadOnlyDictionary<LinkTarget, LinkTargetResolution> linkResolutions,
        IReadOnlyList<ContentType> allContentTypes,
        CancellationToken cancellationToken) =>
        candidate.Block.BlockTypeKey switch
        {
            "hero-slider" => await ResolveHeroSliderAsync(candidate, languageCode, defaultLanguageCode, now, cancellationToken),
            "logo-strip" => await ResolveLogoStripAsync(candidate, languageCode, cancellationToken),
            "quick-links" => ResolveQuickLinks(candidate, linkResolutions),
            "content-list" => await ResolveContentListAsync(
                candidate, languageCode, defaultLanguageCode, now, allContentTypes, cancellationToken),
            "upcoming-events" => await ResolveUpcomingEventsAsync(
                candidate, languageCode, defaultLanguageCode, now, allContentTypes, cancellationToken),
            "job-list" => new PublicLayoutBlockResponse("job-list", candidate.Settings, candidate.Texts, null),
            "feature-mosaic" => await ResolveFeatureMosaicAsync(candidate, mediaById, linkResolutions, cancellationToken),
            "process-steps" => new PublicLayoutBlockResponse("process-steps", candidate.Settings, candidate.Texts, null),
            "video-feature" => ResolveVideoFeature(candidate, languageCode, videoById, linkResolutions),
            "impact-stats" => await ResolveImpactStatsAsync(candidate, languageCode, cancellationToken),
            "cta" => ResolveCta(candidate, linkResolutions),
            "rich-text" => new PublicLayoutBlockResponse("rich-text", null, candidate.Texts, null),
            "image-text" => await ResolveImageTextAsync(candidate, mediaById, linkResolutions, cancellationToken),
            "faq" => await ResolveFaqAsync(candidate, languageCode, now, allContentTypes, cancellationToken),
            "gallery" => await ResolveGalleryAsync(candidate, languageCode, mediaById, cancellationToken),
            _ => null,
        };

    private async Task<PublicLayoutBlockResponse?> ResolveHeroSliderAsync(
        Candidate candidate, LanguageCode languageCode, LanguageCode defaultLanguageCode, DateTime now, CancellationToken cancellationToken)
    {
        var settings = Deserialize<HeroSliderBlockSettings>(candidate.Settings);
        var slides = await sliderPublicQueryService.GetVisibleSlidesAsync(
            settings.SliderId, languageCode, defaultLanguageCode, now, cancellationToken);

        return slides.Count == 0 ? null : new PublicLayoutBlockResponse("hero-slider", candidate.Settings, null, slides);
    }

    private async Task<PublicLayoutBlockResponse?> ResolveLogoStripAsync(Candidate candidate, LanguageCode languageCode, CancellationToken cancellationToken)
    {
        var settings = Deserialize<LogoStripBlockSettings>(candidate.Settings);
        var partners = await partnerPublicQueryService.GetActiveAsync(languageCode, cancellationToken);
        var taken = partners.Take(settings.MaxItems).ToList();

        return taken.Count == 0 ? null : new PublicLayoutBlockResponse("logo-strip", candidate.Settings, candidate.Texts, taken);
    }

    private static PublicLayoutBlockResponse? ResolveQuickLinks(
        Candidate candidate, IReadOnlyDictionary<LinkTarget, LinkTargetResolution> linkResolutions)
    {
        var settings = Deserialize<QuickLinksBlockSettings>(candidate.Settings);
        var texts = Deserialize<QuickLinksBlockTexts>(candidate.Texts!.Value);

        var items = new List<PublicQuickLinkItemDataResponse>();
        for (var i = 0; i < settings.Items.Count; i++)
        {
            var itemSettings = settings.Items[i];
            var itemTexts = texts.Items[i];
            var href = ResolveHref(itemSettings.Link, linkResolutions);
            if (href is null)
            {
                continue;
            }

            items.Add(new PublicQuickLinkItemDataResponse(itemSettings.IconKey, itemTexts.Label, itemTexts.Description, href));
        }

        return items.Count == 0 ? null : new PublicLayoutBlockResponse("quick-links", null, null, items);
    }

    private async Task<PublicLayoutBlockResponse?> ResolveContentListAsync(
        Candidate candidate, LanguageCode languageCode, LanguageCode defaultLanguageCode, DateTime now, IReadOnlyList<ContentType> allContentTypes,
        CancellationToken cancellationToken)
    {
        var settings = Deserialize<ContentListBlockSettings>(candidate.Settings);
        var contentType = FindActiveContentType(settings.ContentTypeKey, allContentTypes);
        if (contentType is null)
        {
            return null;
        }

        IReadOnlyList<Guid>? categoryIds = settings.CategoryId is { } categoryId ? [categoryId] : null;
        var featured = settings.FeaturedOnly ? true : (bool?)null;
        var paged = await contentItemRepository.SearchPublicListAsync(
            contentType.Id, languageCode, categoryIds, null, null, null, null, featured, contentType.SortMode, now,
            new PagedRequest { Page = 1, PageSize = settings.Count }, cancellationToken);

        var items = await BuildContentListItemsAsync(paged.Items, contentType.HasDetailPage, languageCode, defaultLanguageCode, cancellationToken);
        if (items.Count == 0)
        {
            return null;
        }

        string? moreHref = null;
        if (contentType.HasListingPage)
        {
            var typeTranslation = contentType.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
            if (typeTranslation is not null)
            {
                moreHref = RoutePathFormat.BuildPublicPath(languageCode.Value, defaultLanguageCode.Value, typeTranslation.RoutePrefix);
            }
        }

        var data = new PublicContentListBlockDataResponse(items, moreHref);
        return new PublicLayoutBlockResponse("content-list", candidate.Settings, candidate.Texts, data);
    }

    private async Task<PublicLayoutBlockResponse?> ResolveUpcomingEventsAsync(
        Candidate candidate, LanguageCode languageCode, LanguageCode defaultLanguageCode, DateTime now, IReadOnlyList<ContentType> allContentTypes,
        CancellationToken cancellationToken)
    {
        var settings = Deserialize<UpcomingEventsBlockSettings>(candidate.Settings);

        var allItems = new List<PublicContentListBlockItemResponse>();
        foreach (var key in settings.ContentTypeKeys.Distinct(StringComparer.Ordinal))
        {
            var contentType = FindActiveContentType(key, allContentTypes);
            if (contentType is null || !contentType.SupportsEvent)
            {
                continue;
            }

            var paged = await contentItemRepository.SearchPublicListAsync(
                contentType.Id, languageCode, null, null, null, null, null, null, ContentTypeSortMode.EventDateAsc, now,
                new PagedRequest { Page = 1, PageSize = settings.Count }, cancellationToken);
            allItems.AddRange(
                await BuildContentListItemsAsync(paged.Items, contentType.HasDetailPage, languageCode, defaultLanguageCode, cancellationToken));
        }

        var merged = allItems.OrderByDescending(i => i.EffectivePublishDate).Take(settings.Count).ToList();
        return merged.Count == 0 ? null : new PublicLayoutBlockResponse("upcoming-events", candidate.Settings, candidate.Texts, new PublicUpcomingEventsBlockDataResponse(merged));
    }

    private async Task<PublicLayoutBlockResponse?> ResolveFeatureMosaicAsync(
        Candidate candidate, IReadOnlyDictionary<Guid, MediaAsset> mediaById, IReadOnlyDictionary<LinkTarget, LinkTargetResolution> linkResolutions,
        CancellationToken cancellationToken)
    {
        var settings = Deserialize<FeatureMosaicBlockSettings>(candidate.Settings);
        var texts = Deserialize<FeatureMosaicBlockTexts>(candidate.Texts!.Value);

        var items = new List<PublicFeatureMosaicItemDataResponse>();
        for (var i = 0; i < settings.Items.Count; i++)
        {
            var itemSettings = settings.Items[i];
            var itemTexts = texts.Items[i];
            var href = ResolveHref(itemSettings.Link, linkResolutions);
            if (href is null)
            {
                continue;
            }

            var image = itemSettings.ImageMediaId is { } imageId && mediaById.TryGetValue(imageId, out var asset)
                ? await BuildImageAsync(asset, cancellationToken)
                : null;

            items.Add(new PublicFeatureMosaicItemDataResponse(itemTexts.Eyebrow, itemTexts.Title, image, href));
        }

        return items.Count == 0 ? null : new PublicLayoutBlockResponse("feature-mosaic", null, null, items);
    }

    private static PublicLayoutBlockResponse? ResolveVideoFeature(
        Candidate candidate, LanguageCode languageCode, IReadOnlyDictionary<Guid, Video> videoById,
        IReadOnlyDictionary<LinkTarget, LinkTargetResolution> linkResolutions)
    {
        var settings = Deserialize<VideoFeatureBlockSettings>(candidate.Settings);
        if (!videoById.TryGetValue(settings.VideoId, out var video) || !video.IsActive)
        {
            return null;
        }

        if (video.Translations.All(t => t.LanguageCode != languageCode))
        {
            return null;
        }

        var texts = Deserialize<VideoFeatureBlockTexts>(candidate.Texts!.Value);
        var href = ResolveHref(texts.Link, linkResolutions);

        var data = new PublicVideoFeatureBlockDataResponse(video.YouTubeVideoId.BuildEmbedUrl(), video.YouTubeVideoId.BuildThumbnailUrl(), href);
        return new PublicLayoutBlockResponse("video-feature", candidate.Settings, candidate.Texts, data);
    }

    private async Task<PublicLayoutBlockResponse?> ResolveImpactStatsAsync(Candidate candidate, LanguageCode languageCode, CancellationToken cancellationToken)
    {
        var settings = Deserialize<ImpactStatsBlockSettings>(candidate.Settings);
        var metrics = await impactMetricPublicQueryService.GetActiveAsync(languageCode, cancellationToken);
        var filtered = settings.MetricIds.Count == 0
            ? metrics.Take(8).ToList()
            : metrics.Where(m => settings.MetricIds.Contains(m.Id)).ToList();

        return filtered.Count == 0 ? null : new PublicLayoutBlockResponse("impact-stats", candidate.Settings, candidate.Texts, filtered);
    }

    private static PublicLayoutBlockResponse? ResolveCta(Candidate candidate, IReadOnlyDictionary<LinkTarget, LinkTargetResolution> linkResolutions)
    {
        var settings = Deserialize<CtaBlockSettings>(candidate.Settings);
        var texts = Deserialize<CtaBlockTexts>(candidate.Texts!.Value);

        var buttons = new List<PublicCtaButtonDataResponse>();
        for (var i = 0; i < settings.Buttons.Count; i++)
        {
            var buttonSettings = settings.Buttons[i];
            var buttonTexts = texts.Buttons[i];
            var href = ResolveHref(buttonSettings.Link, linkResolutions);
            if (href is null)
            {
                continue;
            }

            buttons.Add(new PublicCtaButtonDataResponse(buttonTexts.Label, buttonSettings.Style, href));
        }

        return buttons.Count == 0
            ? null
            : new PublicLayoutBlockResponse("cta", null, null, new PublicCtaBlockDataResponse(texts.Eyebrow, texts.Title, buttons));
    }

    private async Task<PublicLayoutBlockResponse?> ResolveImageTextAsync(
        Candidate candidate, IReadOnlyDictionary<Guid, MediaAsset> mediaById, IReadOnlyDictionary<LinkTarget, LinkTargetResolution> linkResolutions,
        CancellationToken cancellationToken)
    {
        var settings = Deserialize<ImageTextBlockSettings>(candidate.Settings);
        if (!mediaById.TryGetValue(settings.ImageMediaId, out var asset))
        {
            // §5.2 "image-text görseli silinmiş (medya silme korumalı olduğu için bu yalnızca veri
            // tutarsızlığında olur)" - a defensive, normally-unreachable hide.
            return null;
        }

        var image = await BuildImageAsync(asset, cancellationToken);
        var href = ResolveHref(settings.Link, linkResolutions);

        return new PublicLayoutBlockResponse("image-text", candidate.Settings, candidate.Texts, new PublicImageTextBlockDataResponse(image, href));
    }

    private async Task<PublicLayoutBlockResponse?> ResolveFaqAsync(
        Candidate candidate, LanguageCode languageCode, DateTime now, IReadOnlyList<ContentType> allContentTypes, CancellationToken cancellationToken)
    {
        var settings = Deserialize<FaqBlockSettings>(candidate.Settings);
        var contentType = FindActiveContentType(settings.ContentTypeKey, allContentTypes);
        if (contentType is null)
        {
            return null;
        }

        IReadOnlyList<Guid>? categoryIds = settings.CategoryId is { } categoryId ? [categoryId] : null;
        var paged = await contentItemRepository.SearchPublicListAsync(
            contentType.Id, languageCode, categoryIds, null, null, null, null, null, contentType.SortMode, now,
            new PagedRequest { Page = 1, PageSize = settings.Count }, cancellationToken);

        var items = paged.Items.Select(i => new PublicFaqBlockItemResponse(i.Id, i.Title, i.Body)).ToList();
        return items.Count == 0 ? null : new PublicLayoutBlockResponse("faq", candidate.Settings, candidate.Texts, new PublicFaqBlockDataResponse(items));
    }

    private async Task<PublicLayoutBlockResponse?> ResolveGalleryAsync(
        Candidate candidate, LanguageCode languageCode, IReadOnlyDictionary<Guid, MediaAsset> mediaById, CancellationToken cancellationToken)
    {
        var settings = Deserialize<GalleryBlockSettings>(candidate.Settings);

        var images = new List<PublicGalleryImageDataResponse>();
        foreach (var mediaId in settings.MediaIds)
        {
            if (!mediaById.TryGetValue(mediaId, out var asset))
            {
                continue;
            }

            var image = await BuildImageAsync(asset, cancellationToken);
            var altText = asset.Translations.FirstOrDefault(t => t.LanguageCode == languageCode)?.AltText ?? string.Empty;
            images.Add(new PublicGalleryImageDataResponse(asset.Id, image, altText));
        }

        return images.Count == 0 ? null : new PublicLayoutBlockResponse("gallery", candidate.Settings, candidate.Texts, images);
    }

    private async Task<List<PublicContentListBlockItemResponse>> BuildContentListItemsAsync(
        IReadOnlyList<PublicContentListItemCandidate> candidates, bool hasDetailPage, LanguageCode languageCode, LanguageCode defaultLanguageCode,
        CancellationToken cancellationToken)
    {
        var items = new List<PublicContentListBlockItemResponse>();
        foreach (var candidate in candidates)
        {
            var coverImage = await BuildImageByIdAsync(candidate.CoverImageMediaId, cancellationToken);

            if (hasDetailPage)
            {
                var path = RoutePathFormat.BuildPublicPath(languageCode.Value, defaultLanguageCode.Value, candidate.FullPath);
                items.Add(new PublicContentListBlockItemResponse(
                    candidate.Id, candidate.Title, candidate.Summary, path, coverImage, candidate.EffectivePublishDate, candidate.IsFeatured,
                    null, null));
            }
            else
            {
                var detailImage = await BuildImageByIdAsync(candidate.DetailImageMediaId, cancellationToken);
                items.Add(new PublicContentListBlockItemResponse(
                    candidate.Id, candidate.Title, candidate.Summary, null, coverImage, candidate.EffectivePublishDate, candidate.IsFeatured,
                    candidate.Body, detailImage));
            }
        }

        return items;
    }

    private static ContentType? FindActiveContentType(string key, IReadOnlyList<ContentType> allContentTypes)
    {
        var keyResult = ContentTypeKey.Create(key);
        if (keyResult.IsFailure)
        {
            return null;
        }

        var contentType = allContentTypes.FirstOrDefault(t => t.Key == keyResult.Value);
        return contentType is { IsActive: true } ? contentType : null;
    }

    private static string? ResolveHref(LinkTargetDto? dto, IReadOnlyDictionary<LinkTarget, LinkTargetResolution> linkResolutions)
    {
        var target = ToTarget(dto);
        if (target is null)
        {
            return null;
        }

        return linkResolutions.TryGetValue(target, out var resolution) && resolution.IsResolved ? resolution.Href : null;
    }

    private static LinkTarget? ToTarget(LinkTargetDto? dto)
    {
        var result = LinkTargetDtoMapper.ToLinkTarget(dto);
        return result.IsSuccess && !result.Value.IsEmpty ? result.Value : null;
    }

    private async Task<PublicBlockImageResponse?> BuildImageByIdAsync(Guid? mediaAssetId, CancellationToken cancellationToken)
    {
        if (mediaAssetId is not { } id)
        {
            return null;
        }

        var asset = await mediaAssetRepository.GetByIdAsync(id, cancellationToken);
        return asset is null ? null : await BuildImageAsync(asset, cancellationToken);
    }

    private async Task<PublicBlockImageResponse> BuildImageAsync(MediaAsset mediaAsset, CancellationToken cancellationToken)
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

        return new PublicBlockImageResponse(small, medium, large, originalUrl);
    }

    private static T Deserialize<T>(JsonElement element) => element.Deserialize<T>(JsonOptions)!;
}
