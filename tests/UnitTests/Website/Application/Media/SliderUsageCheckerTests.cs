using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;
using GenclikMerkezi.Modules.Website.Application.Media;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Application.Media;

// §4.3: the real implementation, replacing Görev 2's always-empty stub - a Slider referenced by a
// hero-slider block in any layout's draft or published blocks is reported as in use.
public class SliderUsageCheckerTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakePageLayoutRepository _pageLayoutRepository = new();

    private SliderUsageChecker CreateChecker() =>
        new(new PageLayoutReferenceScanner(_pageLayoutRepository, new BlockTypeRegistry([new HeroSliderBlockTypeDefinition()])));

    private static LayoutBlock HeroSliderBlock(Guid sliderId) =>
        LayoutBlock.Create("hero-slider", 1, true, $"{{\"sliderId\":\"{sliderId}\"}}", []).Value;

    [Fact]
    public async Task GetUsagesAsync_SliderReferencedInDraftBlock_ReturnsUsage()
    {
        var sliderId = Guid.NewGuid();
        var layout = PageLayout.CreateForContent(Guid.NewGuid(), UserId, Now).Value;
        layout.ReplaceDraftBlocks([HeroSliderBlock(sliderId)], UserId, Now);
        _pageLayoutRepository.Seed(layout);

        var usages = await CreateChecker().GetUsagesAsync(sliderId);

        Assert.Single(usages);
    }

    [Fact]
    public async Task GetUsagesAsync_SliderReferencedInPublishedBlockOnly_ReturnsUsage()
    {
        var sliderId = Guid.NewGuid();
        var layout = PageLayout.CreateForContent(Guid.NewGuid(), UserId, Now).Value;
        layout.ReplaceDraftBlocks([HeroSliderBlock(sliderId)], UserId, Now);
        layout.Publish(UserId, Now);
        layout.ReplaceDraftBlocks([], UserId, Now);
        _pageLayoutRepository.Seed(layout);

        var usages = await CreateChecker().GetUsagesAsync(sliderId);

        Assert.Single(usages);
    }

    [Fact]
    public async Task GetUsagesAsync_SliderNotReferenced_ReturnsEmpty()
    {
        var layout = PageLayout.CreateForContent(Guid.NewGuid(), UserId, Now).Value;
        layout.ReplaceDraftBlocks([HeroSliderBlock(Guid.NewGuid())], UserId, Now);
        _pageLayoutRepository.Seed(layout);

        var usages = await CreateChecker().GetUsagesAsync(Guid.NewGuid());

        Assert.Empty(usages);
    }
}
