using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

// A BlockTypeRegistry seeded with every real block type definition (§4.2's full catalog) - used by
// tests that resolve a layout's blocks (PublicPageLayoutResolverTests) rather than test one block
// type's own validation in isolation, where each test builds its own minimal single-type registry.
public static class TestBlockTypeRegistry
{
    public static IBlockTypeRegistry Create()
    {
        var sanitizer = new FakeHtmlContentSanitizer();

        return new BlockTypeRegistry(
        [
            new HeroSliderBlockTypeDefinition(),
            new LogoStripBlockTypeDefinition(),
            new QuickLinksBlockTypeDefinition(),
            new ContentListBlockTypeDefinition(),
            new UpcomingEventsBlockTypeDefinition(),
            new JobListBlockTypeDefinition(),
            new FeatureMosaicBlockTypeDefinition(),
            new ProcessStepsBlockTypeDefinition(),
            new VideoFeatureBlockTypeDefinition(),
            new ImpactStatsBlockTypeDefinition(),
            new CtaBlockTypeDefinition(),
            new RichTextBlockTypeDefinition(sanitizer),
            new ImageTextBlockTypeDefinition(sanitizer),
            new FaqBlockTypeDefinition(),
            new GalleryBlockTypeDefinition(),
        ]);
    }
}
