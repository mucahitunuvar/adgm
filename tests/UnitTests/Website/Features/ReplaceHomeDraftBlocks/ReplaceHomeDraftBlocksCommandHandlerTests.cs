using System.Text.Json;
using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.ReplaceHomeDraftBlocks;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Website.Features.ReplaceHomeDraftBlocks;

public class ReplaceHomeDraftBlocksCommandHandlerTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakePageLayoutRepository _pageLayoutRepository = new();
    private readonly FakeSiteLanguageRepository _siteLanguageRepository = new();
    private readonly FakeMediaAssetRepository _mediaAssetRepository = new();
    private readonly FakeVideoRepository _videoRepository = new();
    private readonly FakeSliderRepository _sliderRepository = new();
    private readonly FakeImpactMetricRepository _impactMetricRepository = new();
    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeContentTypeRepository _contentTypeRepository = new();
    private readonly FakeContentCategoryRepository _contentCategoryRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly ICacheService _cacheService =
        new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings()));

    private PageLayout SeedHomeLayout()
    {
        var layout = PageLayout.CreateHome(Guid.NewGuid(), UserId, Now);
        _pageLayoutRepository.Seed(layout);
        return layout;
    }

    private ReplaceHomeDraftBlocksCommandHandler CreateHandler()
    {
        var defaultLanguage = SiteLanguage.Create(Tr, "Türkçe", 1, UserId, Now);
        defaultLanguage.MarkAsDefault(UserId, Now);
        _siteLanguageRepository.Seed(defaultLanguage);

        var registry = new BlockTypeRegistry([new HeroSliderBlockTypeDefinition(), new RichTextBlockTypeDefinition(new FakeHtmlContentSanitizer())]);
        var processor = new LayoutBlockInputProcessor(
            registry,
            new PageLayoutReferenceValidator(
                _mediaAssetRepository, _videoRepository, _sliderRepository, _impactMetricRepository, _contentItemRepository,
                _contentTypeRepository, _contentCategoryRepository));

        return new ReplaceHomeDraftBlocksCommandHandler(
            _pageLayoutRepository, _siteLanguageRepository, processor, new FakeCurrentUserContext(UserId), new FakeTimeProvider(Now),
            _cacheService, _unitOfWork);
    }

    private static LayoutBlockInput RichTextInput() =>
        new(
            "rich-text", 1, true, JsonDocument.Parse("{}").RootElement,
            new Dictionary<string, JsonElement> { ["tr"] = JsonDocument.Parse("{\"body\":\"<p>x</p>\"}").RootElement });

    [Fact]
    public async Task Handle_WithMatchingRowVersion_ReplacesDraftBlocks()
    {
        var layout = SeedHomeLayout();

        var result = await CreateHandler().Handle(
            new ReplaceHomeDraftBlocksCommand(layout.RowVersion, [RichTextInput()]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(layout.DraftBlocks);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithStaleRowVersion_ReturnsConcurrencyConflict()
    {
        var layout = SeedHomeLayout();
        var staleRowVersion = layout.RowVersion;
        layout.ReplaceDraftBlocks([], UserId, Now.AddMinutes(1));

        var result = await CreateHandler().Handle(
            new ReplaceHomeDraftBlocksCommand(staleRowVersion, [RichTextInput()]), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PageLayout.ConcurrencyConflict", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownBlockType_Fails()
    {
        var layout = SeedHomeLayout();
        var input = new LayoutBlockInput("does-not-exist", 1, true, JsonDocument.Parse("{}").RootElement);

        var result = await CreateHandler().Handle(new ReplaceHomeDraftBlocksCommand(layout.RowVersion, [input]), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PageLayout.UnknownBlockType", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
