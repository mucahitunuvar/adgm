using System.Text.Json;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Application.BlockTypes;

public class LayoutBlockInputProcessorTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement;

    private readonly FakeMediaAssetRepository _mediaAssetRepository = new();
    private readonly FakeVideoRepository _videoRepository = new();
    private readonly FakeSliderRepository _sliderRepository = new();
    private readonly FakeImpactMetricRepository _impactMetricRepository = new();
    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeContentTypeRepository _contentTypeRepository = new();
    private readonly FakeContentCategoryRepository _contentCategoryRepository = new();

    private LayoutBlockInputProcessor CreateProcessor(IBlockTypeRegistry registry) =>
        new(
            registry,
            new PageLayoutReferenceValidator(
                _mediaAssetRepository, _videoRepository, _sliderRepository, _impactMetricRepository, _contentItemRepository,
                _contentTypeRepository, _contentCategoryRepository));

    [Fact]
    public async Task ProcessAsync_UnknownBlockType_Fails()
    {
        var processor = CreateProcessor(new BlockTypeRegistry([new JobListBlockTypeDefinition()]));
        var inputs = new[] { new LayoutBlockInput("does-not-exist", 1, true, Json("{}")) };

        var result = await processor.ProcessAsync(inputs, PageLayoutTargetKind.Home, Tr, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PageLayout.UnknownBlockType", result.Error.Code);
    }

    [Fact]
    public async Task ProcessAsync_BlockTypeNotAllowedForTarget_Fails()
    {
        // §4.2 "job-list yalnızca Home hedefinde kullanılabilir" - rejected when used in a Content layout.
        var processor = CreateProcessor(new BlockTypeRegistry([new JobListBlockTypeDefinition()]));
        var inputs = new[]
        {
            new LayoutBlockInput(
                "job-list", 1, true, Json("{\"count\":3}"),
                new Dictionary<string, JsonElement> { ["tr"] = Json("{\"title\":\"İlanlar\",\"moreLabel\":null}") }),
        };

        var result = await processor.ProcessAsync(inputs, PageLayoutTargetKind.Content, Tr, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PageLayout.BlockTypeNotAllowedForTarget", result.Error.Code);
    }

    [Fact]
    public async Task ProcessAsync_ReferencedSliderDoesNotExist_Fails()
    {
        var processor = CreateProcessor(new BlockTypeRegistry([new HeroSliderBlockTypeDefinition()]));
        var inputs = new[] { new LayoutBlockInput("hero-slider", 1, true, Json($"{{\"sliderId\":\"{Guid.NewGuid()}\"}}")) };

        var result = await processor.ProcessAsync(inputs, PageLayoutTargetKind.Home, Tr, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PageLayout.SliderNotFound", result.Error.Code);
    }

    [Fact]
    public async Task ProcessAsync_ReferencedSliderExists_Succeeds()
    {
        var slider = Slider.Create("home-hero", Tr, "Ana Sayfa", Guid.NewGuid(), DateTime.UtcNow).Value;
        _sliderRepository.Seed(slider);

        var processor = CreateProcessor(new BlockTypeRegistry([new HeroSliderBlockTypeDefinition()]));
        var inputs = new[] { new LayoutBlockInput("hero-slider", 1, true, Json($"{{\"sliderId\":\"{slider.Id}\"}}")) };

        var result = await processor.ProcessAsync(inputs, PageLayoutTargetKind.Home, Tr, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
    }

    [Fact]
    public async Task ValidateStoredBlocksAsync_ReferencedMetricNoLongerExists_Fails()
    {
        // §4.3 "bu anda referans doğrulaması tekrar yapılır" - an impact-stats block built from a
        // metric that has since been deleted must fail re-validation at publish time.
        var metricId = Guid.NewGuid();
        var block = LayoutBlock.Create(
            "impact-stats", 1, true, $"{{\"metricIds\":[\"{metricId}\"]}}",
            [LayoutBlockTranslation.Create(Tr, "{\"title\":null}")]).Value;

        var processor = CreateProcessor(new BlockTypeRegistry([new ImpactStatsBlockTypeDefinition()]));

        var result = await processor.ValidateStoredBlocksAsync([block], PageLayoutTargetKind.Home, Tr, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PageLayout.ImpactMetricNotFound", result.Error.Code);
    }
}
