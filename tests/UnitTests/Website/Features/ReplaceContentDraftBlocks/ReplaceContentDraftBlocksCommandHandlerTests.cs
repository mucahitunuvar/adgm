using System.Text.Json;
using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.ReplaceContentDraftBlocks;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Website.Features.ReplaceContentDraftBlocks;

public class ReplaceContentDraftBlocksCommandHandlerTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly SeoMetadata EmptySeo = SeoMetadata.CreateEmpty();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakePageLayoutRepository _pageLayoutRepository = new();
    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeContentTypeRepository _contentTypeRepository = new();
    private readonly FakeSiteLanguageRepository _siteLanguageRepository = new();
    private readonly FakeMediaAssetRepository _mediaAssetRepository = new();
    private readonly FakeVideoRepository _videoRepository = new();
    private readonly FakeSliderRepository _sliderRepository = new();
    private readonly FakeImpactMetricRepository _impactMetricRepository = new();
    private readonly FakeContentCategoryRepository _contentCategoryRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly ICacheService _cacheService =
        new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings()));

    private ContentItem SeedContentItem(bool supportsBlockLayout, out ContentType contentType)
    {
        contentType = ContentType.Create(
            ContentTypeKey.Create("sayfa").Value, "list", "detail", ContentTypeSortMode.Manual, 1,
            ContentTypeFeatureFlags.None with { SupportsBlockLayout = supportsBlockLayout, HasDetailPage = true }, Tr, "Sayfa", "sayfa",
            EmptySeo, UserId, Now).Value;
        _contentTypeRepository.Seed(contentType);

        var item = ContentItem.Create(
            contentType.Id, null, false, 1, false, null, null, Tr, "Başlık", "slug", "sayfa", [], null, "<p/>", EmptySeo, UserId, Now).Value;
        _contentItemRepository.Seed(item);

        return item;
    }

    private ReplaceContentDraftBlocksCommandHandler CreateHandler()
    {
        var defaultLanguage = SiteLanguage.Create(Tr, "Türkçe", 1, UserId, Now);
        defaultLanguage.MarkAsDefault(UserId, Now);
        _siteLanguageRepository.Seed(defaultLanguage);

        var registry = new BlockTypeRegistry([new RichTextBlockTypeDefinition(new FakeHtmlContentSanitizer())]);
        var processor = new LayoutBlockInputProcessor(
            registry,
            new PageLayoutReferenceValidator(
                _mediaAssetRepository, _videoRepository, _sliderRepository, _impactMetricRepository, _contentItemRepository,
                _contentTypeRepository, _contentCategoryRepository));

        return new ReplaceContentDraftBlocksCommandHandler(
            _pageLayoutRepository, _contentItemRepository, _contentTypeRepository, _siteLanguageRepository, processor,
            new FakeCurrentUserContext(UserId), new FakeTimeProvider(Now), _cacheService, _unitOfWork);
    }

    private static LayoutBlockInput RichTextInput() =>
        new(
            "rich-text", 1, true, JsonDocument.Parse("{}").RootElement,
            new Dictionary<string, JsonElement> { ["tr"] = JsonDocument.Parse("{\"body\":\"<p>x</p>\"}").RootElement });

    [Fact]
    public async Task Handle_FirstSave_CreatesLayoutIgnoringRowVersion()
    {
        var item = SeedContentItem(supportsBlockLayout: true, out _);

        var result = await CreateHandler().Handle(
            new ReplaceContentDraftBlocksCommand(item.Id, [], [RichTextInput()]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var layout = await _pageLayoutRepository.GetByContentItemIdAsync(item.Id);
        Assert.NotNull(layout);
        Assert.Single(layout!.DraftBlocks);
    }

    [Fact]
    public async Task Handle_ContentTypeDoesNotSupportBlockLayout_Fails()
    {
        var item = SeedContentItem(supportsBlockLayout: false, out _);

        var result = await CreateHandler().Handle(
            new ReplaceContentDraftBlocksCommand(item.Id, [], [RichTextInput()]), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PageLayout.ContentTypeDoesNotSupportBlockLayout", result.Error.Code);
    }

    [Fact]
    public async Task Handle_UnknownContentItem_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(
            new ReplaceContentDraftBlocksCommand(Guid.NewGuid(), [], [RichTextInput()]), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_SecondSaveWithStaleRowVersion_ReturnsConcurrencyConflict()
    {
        var item = SeedContentItem(supportsBlockLayout: true, out _);
        var handler = CreateHandler();
        await handler.Handle(new ReplaceContentDraftBlocksCommand(item.Id, [], [RichTextInput()]), CancellationToken.None);

        var result = await handler.Handle(
            new ReplaceContentDraftBlocksCommand(item.Id, [], [RichTextInput()]), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PageLayout.ConcurrencyConflict", result.Error.Code);
    }
}
