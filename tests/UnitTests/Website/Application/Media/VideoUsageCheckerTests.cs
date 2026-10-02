using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;
using GenclikMerkezi.Modules.Website.Application.Media;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Application.Media;

// ADR-024 §5 (Faz 1b Görev 4), extended in Faz 2 Görev 4: a video is also in use when a video-feature
// block in any PageLayout's draft or published blocks references it, in addition to the pre-existing
// ContentItem.VideoIds usage.
public class VideoUsageCheckerTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeSiteLanguageRepository _siteLanguageRepository = new();
    private readonly FakePageLayoutRepository _pageLayoutRepository = new();

    private VideoUsageChecker CreateChecker() =>
        new(
            _contentItemRepository, _siteLanguageRepository,
            new PageLayoutReferenceScanner(_pageLayoutRepository, new BlockTypeRegistry([new VideoFeatureBlockTypeDefinition()])));

    private static LayoutBlock VideoFeatureBlock(Guid videoId) =>
        LayoutBlock.Create(
            "video-feature", 1, true, $"{{\"videoId\":\"{videoId}\"}}",
            [LayoutBlockTranslation.Create(Tr, "{\"eyebrow\":null,\"title\":\"Video\",\"text\":null,\"link\":null,\"linkLabel\":null}")]).Value;

    [Fact]
    public async Task GetUsagesAsync_VideoReferencedByLayoutBlock_ReturnsUsage()
    {
        var videoId = Guid.NewGuid();
        var layout = PageLayout.CreateForContent(Guid.NewGuid(), UserId, Now).Value;
        layout.ReplaceDraftBlocks([VideoFeatureBlock(videoId)], UserId, Now);
        _pageLayoutRepository.Seed(layout);

        var usages = await CreateChecker().GetUsagesAsync(videoId);

        var usage = Assert.Single(usages);
        Assert.Equal("page-layout", usage.SourceKey);
    }

    [Fact]
    public async Task GetUsagesAsync_VideoNotReferencedAnywhere_ReturnsEmpty()
    {
        var usages = await CreateChecker().GetUsagesAsync(Guid.NewGuid());

        Assert.Empty(usages);
    }
}
