using System.Text.Json;
using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Application.BlockTypes.PublicResolution;
using GenclikMerkezi.Modules.Website.Application.ImpactMetrics;
using GenclikMerkezi.Modules.Website.Application.LinkTargets;
using GenclikMerkezi.Modules.Website.Application.Partners;
using GenclikMerkezi.Modules.Website.Application.Sliders;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Application.BlockTypes.PublicResolution;

// Faz 2 Görev 5 master prompt §5.2/§5.3: every block type's `data` resolution, the hiding rules
// (inactive block, missing translation, unresolved required reference, empty list data, link-item
// removal) and the batched-reference-loading guarantee, all against fakes - no database involved (the
// real-DB/HTTP path is covered by PublicHomeAndBlockDataFlowTests in IntegrationTests).
public class PublicPageLayoutResolverTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LanguageCode En = LanguageCode.Create("en").Value;
    private static readonly SeoMetadata EmptySeo = SeoMetadata.CreateEmpty();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc);
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly FakeMediaAssetRepository _mediaAssetRepository = new();
    private readonly FakeVideoRepository _videoRepository = new();
    private readonly FakeMediaFileStorageService _fileStorageService = new();
    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeContentTypeRepository _contentTypeRepository = new();
    private readonly FakeSliderRepository _sliderRepository = new();
    private readonly FakePartnerRepository _partnerRepository = new();
    private readonly FakeImpactMetricRepository _impactMetricRepository = new();
    private readonly IBlockTypeRegistry _blockTypeRegistry = TestBlockTypeRegistry.Create();

    private PublicPageLayoutResolver CreateResolver()
    {
        var linkTargetResolver = new LinkTargetResolver(_contentItemRepository, _contentTypeRepository);
        var sliderPublicQueryService = new SliderPublicQueryService(_sliderRepository, _mediaAssetRepository, _fileStorageService, linkTargetResolver);
        var partnerPublicQueryService = new PartnerPublicQueryService(_partnerRepository, _mediaAssetRepository, _fileStorageService);
        var impactMetricPublicQueryService = new ImpactMetricPublicQueryService(_impactMetricRepository);

        return new PublicPageLayoutResolver(
            _blockTypeRegistry, _mediaAssetRepository, _videoRepository, _fileStorageService, linkTargetResolver, sliderPublicQueryService,
            partnerPublicQueryService, impactMetricPublicQueryService, _contentTypeRepository, _contentItemRepository);
    }

    private static JsonElement J(object value) => JsonSerializer.SerializeToElement(value, JsonOptions);

    private static LayoutBlock Block(
        string key, object settings, bool isActive = true, int sortOrder = 1, IReadOnlyDictionary<LanguageCode, object>? texts = null)
    {
        var translations = (texts ?? new Dictionary<LanguageCode, object>())
            .Select(kvp => LayoutBlockTranslation.Create(kvp.Key, JsonSerializer.Serialize(kvp.Value, JsonOptions)))
            .ToList();

        return LayoutBlock.Create(key, sortOrder, isActive, JsonSerializer.Serialize(settings, JsonOptions), translations).Value;
    }

    private static readonly object ExternalLink = new
    {
        kind = "ExternalUrl", contentItemId = (Guid?)null, contentTypeId = (Guid?)null, internalPath = (string?)null,
        externalUrl = "https://example.com",
    };

    private static readonly object NoLink = new
    {
        kind = "None", contentItemId = (Guid?)null, contentTypeId = (Guid?)null, internalPath = (string?)null, externalUrl = (string?)null,
    };

    private static object UnresolvableContentLink(Guid contentItemId) => new
    {
        kind = "Content", contentItemId = (Guid?)contentItemId, contentTypeId = (Guid?)null, internalPath = (string?)null,
        externalUrl = (string?)null,
    };

    // --- fixtures ---

    private Guid SeedImage(string? altText = null)
    {
        var id = Guid.NewGuid();
        var file = FileAttachment.Create($"website-images/{id}.jpg", "image.jpg", "image/jpeg", 1024, Now, "MediaAsset", id);
        var image = MediaAsset.Create(
            id, MediaAssetKind.Image, file, [], 800, 600, MediaFolder.Create("blocks").Value, null, null, false, UserId, Now).Value;
        if (altText is not null)
        {
            image.SetTranslation(Tr, altText, null, UserId, Now);
        }

        _mediaAssetRepository.Seed(image);
        return id;
    }

    private Guid SeedSlider()
    {
        var image = SeedImage();
        var slider = Slider.Create($"slider-{Guid.NewGuid():N}"[..20], Tr, "Slider", UserId, Now).Value;
        var slide = Slide.Create(
            image, null, LinkTarget.CreateEmpty(), 1, true, null, null,
            [SlideTranslation.Create(Tr, null, "Başlık", null, null, null).Value]).Value;
        slider.ReplaceSlides([slide], UserId, Now);
        _sliderRepository.Seed(slider);
        return slider.Id;
    }

    private Guid SeedVideo(bool isActive = true, LanguageCode? translationLanguage = null)
    {
        var video = Video.Create("https://youtu.be/dQw4w9WgXcQ", null, 1, translationLanguage ?? Tr, "Video", null, UserId, Now).Value;
        if (!isActive)
        {
            video.Deactivate(UserId, Now);
        }

        _videoRepository.Seed(video);
        return video.Id;
    }

    private Guid SeedPartner()
    {
        var logo = SeedImage();
        var partner = Partner.Create(logo, null, 1, Tr, "Partner", null, UserId, Now).Value;
        _partnerRepository.Seed(partner);
        return partner.Id;
    }

    private Guid SeedActiveMetric()
    {
        var metric = ImpactMetric.Create(1000, null, 1, Tr, "Etki", "genç", "2026", "Kayıt", UserId, Now).Value;
        metric.Activate(Tr, UserId, Now);
        _impactMetricRepository.Seed(metric);
        return metric.Id;
    }

    private ContentType SeedContentType(string key, bool hasListingPage = true, bool hasDetailPage = true, bool supportsEvent = false)
    {
        var contentType = ContentType.Create(
            ContentTypeKey.Create(key).Value, "cards", "article", supportsEvent ? ContentTypeSortMode.EventDateAsc : ContentTypeSortMode.PublishDateDesc,
            1, ContentTypeFeatureFlags.None with { HasDetailPage = hasDetailPage, HasListingPage = hasListingPage, SupportsEvent = supportsEvent },
            Tr, "Tür", hasListingPage ? key : null, EmptySeo, UserId, Now).Value;
        _contentTypeRepository.Add(contentType);
        return contentType;
    }

    private ContentItem SeedPublishedItem(ContentType contentType, string title = "Başlık")
    {
        var routePrefix = contentType.Translations.First().RoutePrefix;
        var item = ContentItem.Create(
            contentType.Id, null, false, 1, false, null, null, Tr, title, null, routePrefix, [], "özet", "<p>gövde</p>", EmptySeo, UserId, Now).Value;
        item.Publish(null, null, true, true, Tr, UserId, Now);
        _contentItemRepository.Add(item);
        return item;
    }

    // --- cross-cutting hiding rules ---

    [Fact]
    public async Task ResolveAsync_InactiveBlock_IsHidden()
    {
        var block = Block("rich-text", new { }, isActive: false, texts: new Dictionary<LanguageCode, object> { [Tr] = new { body = "<p>x</p>" } });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ResolveAsync_BlockWithoutTranslationInRequestedLanguage_IsHidden()
    {
        var block = Block("rich-text", new { }, texts: new Dictionary<LanguageCode, object> { [Tr] = new { body = "<p>x</p>" } });

        var result = await CreateResolver().ResolveAsync([block], En, Tr, Now, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ResolveAsync_UnknownBlockType_IsIgnored()
    {
        var block = Block("does-not-exist", new { });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ResolveAsync_OrdersBlocksBySortOrder()
    {
        var second = Block("rich-text", new { }, sortOrder: 2, texts: new Dictionary<LanguageCode, object> { [Tr] = new { body = "<p>2</p>" } });
        var first = Block("rich-text", new { }, sortOrder: 1, texts: new Dictionary<LanguageCode, object> { [Tr] = new { body = "<p>1</p>" } });

        var result = await CreateResolver().ResolveAsync([second, first], Tr, Tr, Now, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("<p>1</p>", result[0].Texts!.Value.GetProperty("body").GetString());
        Assert.Equal("<p>2</p>", result[1].Texts!.Value.GetProperty("body").GetString());
    }

    // --- rich-text ---

    [Fact]
    public async Task ResolveAsync_RichText_ReturnsTextsAndNoData()
    {
        var block = Block("rich-text", new { }, texts: new Dictionary<LanguageCode, object> { [Tr] = new { body = "<p>metin</p>" } });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        var response = Assert.Single(result);
        Assert.Equal("rich-text", response.Type);
        Assert.Null(response.Data);
        Assert.Equal("<p>metin</p>", response.Texts!.Value.GetProperty("body").GetString());
    }

    // --- process-steps / job-list (settings passthrough, no data) ---

    [Fact]
    public async Task ResolveAsync_JobList_ReturnsSettingsAndNoData()
    {
        var block = Block(
            "job-list", new { count = 3 }, texts: new Dictionary<LanguageCode, object> { [Tr] = new { title = "İlanlar", moreLabel = (string?)null } });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        var response = Assert.Single(result);
        Assert.Null(response.Data);
        Assert.Equal(3, response.Settings!.Value.GetProperty("count").GetInt32());
    }

    // --- hero-slider ---

    [Fact]
    public async Task ResolveAsync_HeroSlider_WithVisibleSlides_ReturnsSlides()
    {
        var sliderId = SeedSlider();
        var block = Block("hero-slider", new { sliderId });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        var response = Assert.Single(result);
        var slides = Assert.IsAssignableFrom<IReadOnlyList<PublicSliderSlideResponse>>(response.Data);
        Assert.Single(slides);
    }

    [Fact]
    public async Task ResolveAsync_HeroSlider_WithDeletedSlider_IsHidden()
    {
        var block = Block("hero-slider", new { sliderId = Guid.NewGuid() });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        Assert.Empty(result);
    }

    // --- logo-strip ---

    [Fact]
    public async Task ResolveAsync_LogoStrip_ReturnsActivePartnersUpToMaxItems()
    {
        SeedPartner();
        SeedPartner();
        var block = Block(
            "logo-strip", new { maxItems = 1 }, texts: new Dictionary<LanguageCode, object> { [Tr] = new { title = (string?)null } });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        var response = Assert.Single(result);
        var partners = Assert.IsAssignableFrom<IReadOnlyList<PublicPartnerResponse>>(response.Data);
        Assert.Single(partners);
    }

    [Fact]
    public async Task ResolveAsync_LogoStrip_WithNoActivePartners_IsHidden()
    {
        var block = Block(
            "logo-strip", new { maxItems = 10 }, texts: new Dictionary<LanguageCode, object> { [Tr] = new { title = (string?)null } });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        Assert.Empty(result);
    }

    // --- impact-stats ---

    [Fact]
    public async Task ResolveAsync_ImpactStats_EmptyMetricIds_ReturnsAllActiveMetrics()
    {
        SeedActiveMetric();
        var block = Block(
            "impact-stats", new { metricIds = Array.Empty<Guid>() }, texts: new Dictionary<LanguageCode, object> { [Tr] = new { title = (string?)null } });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        var response = Assert.Single(result);
        var metrics = Assert.IsAssignableFrom<IReadOnlyList<PublicImpactMetricResponse>>(response.Data);
        Assert.Single(metrics);
    }

    [Fact]
    public async Task ResolveAsync_ImpactStats_WithNoMatchingActiveMetrics_IsHidden()
    {
        var block = Block(
            "impact-stats", new { metricIds = new[] { Guid.NewGuid() } },
            texts: new Dictionary<LanguageCode, object> { [Tr] = new { title = (string?)null } });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        Assert.Empty(result);
    }

    // --- gallery ---

    [Fact]
    public async Task ResolveAsync_Gallery_ReturnsImagesWithAltTextFromMediaLibrary()
    {
        var image = SeedImage(altText: "Açıklama");
        var block = Block("gallery", new { mediaIds = new[] { image } }, texts: new Dictionary<LanguageCode, object> { [Tr] = new { title = (string?)null } });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        var response = Assert.Single(result);
        var images = Assert.IsAssignableFrom<IReadOnlyList<PublicGalleryImageDataResponse>>(response.Data);
        Assert.Equal("Açıklama", Assert.Single(images).AltText);
    }

    [Fact]
    public async Task ResolveAsync_Gallery_WithNoResolvableImages_IsHidden()
    {
        var block = Block(
            "gallery", new { mediaIds = new[] { Guid.NewGuid() } }, texts: new Dictionary<LanguageCode, object> { [Tr] = new { title = (string?)null } });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        Assert.Empty(result);
    }

    // --- image-text ---

    [Fact]
    public async Task ResolveAsync_ImageText_WithUnresolvableLink_KeepsBlockButHidesHref()
    {
        var image = SeedImage();
        var missingContentId = Guid.NewGuid();
        var block = Block(
            "image-text", new { imageMediaId = image, imagePosition = "left", link = UnresolvableContentLink(missingContentId) },
            texts: new Dictionary<LanguageCode, object> { [Tr] = new { title = "Başlık", body = "<p>x</p>", linkLabel = (string?)null } });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        var response = Assert.Single(result);
        var data = Assert.IsType<PublicImageTextBlockDataResponse>(response.Data);
        Assert.Null(data.Href);
        Assert.NotNull(data.Image);
    }

    [Fact]
    public async Task ResolveAsync_ImageText_WithMissingImage_IsHidden()
    {
        var block = Block(
            "image-text", new { imageMediaId = Guid.NewGuid(), imagePosition = "left", link = NoLink },
            texts: new Dictionary<LanguageCode, object> { [Tr] = new { title = "Başlık", body = "<p>x</p>", linkLabel = (string?)null } });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        Assert.Empty(result);
    }

    // --- video-feature ---

    [Fact]
    public async Task ResolveAsync_VideoFeature_WithActiveTranslatedVideo_ReturnsEmbedAndThumbnail()
    {
        var videoId = SeedVideo();
        var block = Block(
            "video-feature", new { videoId },
            texts: new Dictionary<LanguageCode, object>
            {
                [Tr] = new { eyebrow = (string?)null, title = "Video", text = (string?)null, link = (object?)null, linkLabel = (string?)null },
            });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        var response = Assert.Single(result);
        var data = Assert.IsType<PublicVideoFeatureBlockDataResponse>(response.Data);
        Assert.NotEmpty(data.EmbedUrl);
        Assert.NotEmpty(data.ThumbnailUrl);
        Assert.Null(data.Href);
    }

    [Fact]
    public async Task ResolveAsync_VideoFeature_WithInactiveVideo_IsHidden()
    {
        var videoId = SeedVideo(isActive: false);
        var block = Block(
            "video-feature", new { videoId },
            texts: new Dictionary<LanguageCode, object>
            {
                [Tr] = new { eyebrow = (string?)null, title = "Video", text = (string?)null, link = (object?)null, linkLabel = (string?)null },
            });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ResolveAsync_VideoFeature_WithVideoNotTranslatedInRequestedLanguage_IsHidden()
    {
        var videoId = SeedVideo(translationLanguage: Tr);
        var block = Block(
            "video-feature", new { videoId },
            texts: new Dictionary<LanguageCode, object>
            {
                [Tr] = new { eyebrow = (string?)null, title = "Video", text = (string?)null, link = (object?)null, linkLabel = (string?)null },
            });

        var result = await CreateResolver().ResolveAsync([block], En, Tr, Now, CancellationToken.None);

        Assert.Empty(result);
    }

    // --- quick-links / feature-mosaic / cta: item removal ---

    [Fact]
    public async Task ResolveAsync_QuickLinks_DropsUnresolvedItem_AndHidesBlockWhenNoneResolve()
    {
        var missingContentId = Guid.NewGuid();
        var block = Block(
            "quick-links",
            new { items = new[] { new { iconKey = "star", link = UnresolvableContentLink(missingContentId) } } },
            texts: new Dictionary<LanguageCode, object>
            {
                [Tr] = new { items = new[] { new { label = "Bağlantı", description = (string?)null } } },
            });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ResolveAsync_QuickLinks_WithResolvableLink_ReturnsMergedItem()
    {
        var block = Block(
            "quick-links",
            new { items = new[] { new { iconKey = "star", link = ExternalLink } } },
            texts: new Dictionary<LanguageCode, object>
            {
                [Tr] = new { items = new[] { new { label = "Bağlantı", description = (string?)null } } },
            });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        var response = Assert.Single(result);
        Assert.Null(response.Settings);
        Assert.Null(response.Texts);
        var items = Assert.IsAssignableFrom<IReadOnlyList<PublicQuickLinkItemDataResponse>>(response.Data);
        var item = Assert.Single(items);
        Assert.Equal("star", item.IconKey);
        Assert.Equal("Bağlantı", item.Label);
        Assert.Equal("https://example.com", item.Href);
    }

    [Fact]
    public async Task ResolveAsync_FeatureMosaic_WithResolvableLink_ReturnsMergedItem()
    {
        var image = SeedImage();
        var block = Block(
            "feature-mosaic",
            new { items = new[] { new { imageMediaId = (Guid?)image, link = ExternalLink } } },
            texts: new Dictionary<LanguageCode, object>
            {
                [Tr] = new { items = new[] { new { eyebrow = (string?)null, title = "Öne Çıkan" } } },
            });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        var response = Assert.Single(result);
        var items = Assert.IsAssignableFrom<IReadOnlyList<PublicFeatureMosaicItemDataResponse>>(response.Data);
        var item = Assert.Single(items);
        Assert.Equal("Öne Çıkan", item.Title);
        Assert.NotNull(item.Image);
    }

    [Fact]
    public async Task ResolveAsync_Cta_WithOneResolvableAndOneUnresolvableButton_KeepsOnlyResolvable()
    {
        var missingContentId = Guid.NewGuid();
        var block = Block(
            "cta",
            new
            {
                buttons = new[]
                {
                    new { link = ExternalLink, style = "primary" },
                    new { link = UnresolvableContentLink(missingContentId), style = "secondary" },
                },
            },
            texts: new Dictionary<LanguageCode, object>
            {
                [Tr] = new
                {
                    eyebrow = (string?)null, title = "Harekete Geç",
                    buttons = new[] { new { label = "Git" }, new { label = "Kayıp" } },
                },
            });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        var response = Assert.Single(result);
        var data = Assert.IsType<PublicCtaBlockDataResponse>(response.Data);
        Assert.Equal("Harekete Geç", data.Title);
        var button = Assert.Single(data.Buttons);
        Assert.Equal("Git", button.Label);
        Assert.Equal("primary", button.Style);
    }

    [Fact]
    public async Task ResolveAsync_Cta_WithNoResolvableButtons_IsHidden()
    {
        var missingContentId = Guid.NewGuid();
        var block = Block(
            "cta",
            new { buttons = new[] { new { link = UnresolvableContentLink(missingContentId), style = "primary" } } },
            texts: new Dictionary<LanguageCode, object>
            {
                [Tr] = new { eyebrow = (string?)null, title = "Harekete Geç", buttons = new[] { new { label = "Git" } } },
            });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        Assert.Empty(result);
    }

    // --- content-list ---

    [Fact]
    public async Task ResolveAsync_ContentList_ReturnsItemsAndMoreHref()
    {
        var contentType = SeedContentType("haber");
        SeedPublishedItem(contentType);
        var block = Block(
            "content-list", new { contentTypeKey = "haber", count = 5, featuredOnly = false, categoryId = (Guid?)null, view = "cards" },
            texts: new Dictionary<LanguageCode, object> { [Tr] = new { title = "Haberler", moreLabel = (string?)null } });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        var response = Assert.Single(result);
        var data = Assert.IsType<PublicContentListBlockDataResponse>(response.Data);
        Assert.Single(data.Items);
        Assert.Equal("/haber", data.MoreHref);
    }

    [Fact]
    public async Task ResolveAsync_ContentList_WithInactiveContentType_IsHidden()
    {
        var contentType = SeedContentType("haber");
        contentType.Deactivate(UserId, Now);
        var block = Block(
            "content-list", new { contentTypeKey = "haber", count = 5, featuredOnly = false, categoryId = (Guid?)null, view = "cards" },
            texts: new Dictionary<LanguageCode, object> { [Tr] = new { title = "Haberler", moreLabel = (string?)null } });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ResolveAsync_ContentList_WithNoVisibleItems_IsHidden()
    {
        SeedContentType("haber");
        var block = Block(
            "content-list", new { contentTypeKey = "haber", count = 5, featuredOnly = false, categoryId = (Guid?)null, view = "cards" },
            texts: new Dictionary<LanguageCode, object> { [Tr] = new { title = "Haberler", moreLabel = (string?)null } });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        Assert.Empty(result);
    }

    // --- upcoming-events ---

    [Fact]
    public async Task ResolveAsync_UpcomingEvents_MergesVisibleItemsAcrossSelectedTypes()
    {
        var eventType = SeedContentType("etkinlik", supportsEvent: true);
        SeedPublishedItem(eventType);
        var block = Block(
            "upcoming-events", new { contentTypeKeys = new[] { "etkinlik" }, count = 5 },
            texts: new Dictionary<LanguageCode, object> { [Tr] = new { title = "Etkinlikler", moreLabel = (string?)null } });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        var response = Assert.Single(result);
        var data = Assert.IsType<PublicUpcomingEventsBlockDataResponse>(response.Data);
        Assert.Single(data.Items);
    }

    // --- faq ---

    [Fact]
    public async Task ResolveAsync_Faq_ReturnsTitleAndBody()
    {
        var contentType = SeedContentType("sss", hasListingPage: false, hasDetailPage: false);
        SeedPublishedItem(contentType, "Soru 1");
        var block = Block(
            "faq", new { contentTypeKey = "sss", categoryId = (Guid?)null, count = 10 },
            texts: new Dictionary<LanguageCode, object> { [Tr] = new { title = (string?)null } });

        var result = await CreateResolver().ResolveAsync([block], Tr, Tr, Now, CancellationToken.None);

        var response = Assert.Single(result);
        var data = Assert.IsType<PublicFaqBlockDataResponse>(response.Data);
        var item = Assert.Single(data.Items);
        Assert.Equal("Soru 1", item.Title);
        Assert.Equal("<p>gövde</p>", item.Body);
    }

    // --- §5.3: batched reference loading ---

    [Fact]
    public async Task ResolveAsync_WithManyBlocksReferencingMedia_BatchesIntoASingleGetByIdsAsyncCall()
    {
        var images = Enumerable.Range(0, 5).Select(_ => SeedImage()).ToList();
        var blocks = images
            .Select((image, index) => Block(
                "image-text", new { imageMediaId = image, imagePosition = "left", link = NoLink }, sortOrder: index,
                texts: new Dictionary<LanguageCode, object>
                {
                    [Tr] = new { title = $"Başlık {index}", body = "<p>x</p>", linkLabel = (string?)null },
                }))
            .ToList();

        var result = await CreateResolver().ResolveAsync(blocks, Tr, Tr, Now, CancellationToken.None);

        Assert.Equal(5, result.Count);
        Assert.Equal(1, _mediaAssetRepository.GetByIdsAsyncCallCount);
    }

    [Fact]
    public async Task ResolveAsync_WithManyVideoFeatureBlocks_BatchesVideosIntoASingleGetByIdsAsyncCall()
    {
        var videoIds = Enumerable.Range(0, 3).Select(_ => SeedVideo()).ToList();
        var blocks = videoIds
            .Select((videoId, index) => Block(
                "video-feature", new { videoId }, sortOrder: index,
                texts: new Dictionary<LanguageCode, object>
                {
                    [Tr] = new
                    {
                        eyebrow = (string?)null, title = $"Video {index}", text = (string?)null, link = (object?)null, linkLabel = (string?)null,
                    },
                }))
            .ToList();

        var result = await CreateResolver().ResolveAsync(blocks, Tr, Tr, Now, CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal(1, _videoRepository.GetByIdsAsyncCallCount);
    }
}
