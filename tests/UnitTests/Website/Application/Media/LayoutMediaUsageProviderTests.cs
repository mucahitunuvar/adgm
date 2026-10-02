using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;
using GenclikMerkezi.Modules.Website.Application.Media;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Application.Media;

// §4.3 "Kullanım koruması": a MediaAsset referenced as a block's image (here: image-text) in any
// PageLayout's draft or published blocks cannot be deleted.
public class LayoutMediaUsageProviderTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakePageLayoutRepository _pageLayoutRepository = new();

    private LayoutMediaUsageProvider CreateProvider() =>
        new(new PageLayoutReferenceScanner(_pageLayoutRepository, new BlockTypeRegistry([new ImageTextBlockTypeDefinition(new FakeHtmlContentSanitizer())])));

    private static LayoutBlock ImageTextBlock(Guid imageMediaId) =>
        LayoutBlock.Create(
            "image-text", 1, true, $"{{\"imageMediaId\":\"{imageMediaId}\",\"imagePosition\":\"left\",\"link\":null}}",
            [LayoutBlockTranslation.Create(Tr, "{\"title\":\"Başlık\",\"body\":\"<p>x</p>\",\"linkLabel\":null}")]).Value;

    [Fact]
    public async Task GetUsagesAsync_MediaReferencedByLayoutBlock_ReturnsUsage()
    {
        var mediaId = Guid.NewGuid();
        var layout = PageLayout.CreateForContent(Guid.NewGuid(), UserId, Now).Value;
        layout.ReplaceDraftBlocks([ImageTextBlock(mediaId)], UserId, Now);
        _pageLayoutRepository.Seed(layout);

        var usages = await CreateProvider().GetUsagesAsync(mediaId);

        var usage = Assert.Single(usages);
        Assert.Equal("page-layout", usage.SourceKey);
    }

    [Fact]
    public async Task GetUsagesAsync_MediaNotReferenced_ReturnsEmpty()
    {
        var layout = PageLayout.CreateForContent(Guid.NewGuid(), UserId, Now).Value;
        layout.ReplaceDraftBlocks([ImageTextBlock(Guid.NewGuid())], UserId, Now);
        _pageLayoutRepository.Seed(layout);

        var usages = await CreateProvider().GetUsagesAsync(Guid.NewGuid());

        Assert.Empty(usages);
    }
}
