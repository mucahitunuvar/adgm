using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.PublishContentLayout;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Website.Features.PublishContentLayout;

public class PublishContentLayoutCommandHandlerTests
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

    private PublishContentLayoutCommandHandler CreateHandler()
    {
        var defaultLanguage = SiteLanguage.Create(Tr, "Türkçe", 1, UserId, Now);
        defaultLanguage.MarkAsDefault(UserId, Now);
        _siteLanguageRepository.Seed(defaultLanguage);

        var registry = new BlockTypeRegistry([new ImpactStatsBlockTypeDefinition()]);
        var processor = new LayoutBlockInputProcessor(
            registry,
            new PageLayoutReferenceValidator(
                _mediaAssetRepository, _videoRepository, _sliderRepository, _impactMetricRepository, _contentItemRepository,
                _contentTypeRepository, _contentCategoryRepository));

        return new PublishContentLayoutCommandHandler(
            _pageLayoutRepository, _siteLanguageRepository, processor, new FakeCurrentUserContext(UserId), new FakeTimeProvider(Now),
            _cacheService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_UnknownLayout_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new PublishContentLayoutCommand(Guid.NewGuid(), []), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PageLayout.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_DraftReferencesMetricThatNoLongerExists_FailsRevalidationAndDoesNotPublish()
    {
        // §4.3 "bu anda referans doğrulaması tekrar yapılır" - the metric existed when the draft was
        // saved but was deleted afterwards (ImpactMetric has no layout usage checker), so publish must
        // re-validate and refuse rather than silently publishing a dangling reference.
        var metricId = Guid.NewGuid();
        var layout = PageLayout.CreateForContent(Guid.NewGuid(), UserId, Now).Value;
        var block = LayoutBlock.Create(
            "impact-stats", 1, true, $"{{\"metricIds\":[\"{metricId}\"]}}",
            [LayoutBlockTranslation.Create(Tr, "{\"title\":null}")]).Value;
        layout.ReplaceDraftBlocks([block], UserId, Now);
        _pageLayoutRepository.Seed(layout);

        var result = await CreateHandler().Handle(new PublishContentLayoutCommand(layout.ContentItemId!.Value, layout.RowVersion), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PageLayout.ImpactMetricNotFound", result.Error.Code);
        Assert.Empty(layout.PublishedBlocks);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_DraftReferencesExistingMetric_PublishesSuccessfully()
    {
        var metric = ImpactMetric.Create(
            100m, null, 1, Tr, "Mezun", "kişi", "2026", "Kayıtlar", UserId, Now).Value;
        _impactMetricRepository.Seed(metric);

        var layout = PageLayout.CreateForContent(Guid.NewGuid(), UserId, Now).Value;
        var block = LayoutBlock.Create(
            "impact-stats", 1, true, $"{{\"metricIds\":[\"{metric.Id}\"]}}",
            [LayoutBlockTranslation.Create(Tr, "{\"title\":null}")]).Value;
        layout.ReplaceDraftBlocks([block], UserId, Now);
        _pageLayoutRepository.Seed(layout);

        var result = await CreateHandler().Handle(new PublishContentLayoutCommand(layout.ContentItemId!.Value, layout.RowVersion), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(layout.PublishedBlocks);
        Assert.False(layout.HasUnpublishedChanges);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }
}
