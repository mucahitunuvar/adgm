using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.GetBlockTypes;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.GetBlockTypes;

// §4.1 "GET /api/v1/admin/website/block-types ... her blok tipi için anahtar, kullanılabildiği
// hedefler ve alan açıklamaları": field descriptions must come straight from
// BlockTypeFieldDescriber's reflection over each type's own Settings/Texts record.
public class GetBlockTypesQueryHandlerTests
{
    private static IBlockTypeRegistry CreateFullRegistry() =>
        new BlockTypeRegistry(
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
            new RichTextBlockTypeDefinition(new FakeHtmlContentSanitizer()),
            new ImageTextBlockTypeDefinition(new FakeHtmlContentSanitizer()),
            new FaqBlockTypeDefinition(),
            new GalleryBlockTypeDefinition(),
        ]);

    [Fact]
    public async Task Handle_ReturnsAllFifteenCatalogEntries()
    {
        var result = await new GetBlockTypesQueryHandler(CreateFullRegistry()).Handle(new GetBlockTypesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(15, result.Value.Count);
    }

    [Fact]
    public async Task Handle_JobList_IsHomeOnlyAndHasNoSettingsReferenceFields()
    {
        var result = await new GetBlockTypesQueryHandler(CreateFullRegistry()).Handle(new GetBlockTypesQuery(), CancellationToken.None);

        var jobList = Assert.Single(result.Value, b => b.Key == "job-list");
        Assert.Equal(["Home"], jobList.AllowedTargets);
        var countField = Assert.Single(jobList.SettingsFields, f => f.Name == "count");
        Assert.Equal("int", countField.Type);
        Assert.True(countField.IsRequired);
        Assert.False(countField.IsLanguageDependent);
    }

    [Fact]
    public async Task Handle_QuickLinks_DescribesNestedItemFields()
    {
        var result = await new GetBlockTypesQueryHandler(CreateFullRegistry()).Handle(new GetBlockTypesQuery(), CancellationToken.None);

        var quickLinks = Assert.Single(result.Value, b => b.Key == "quick-links");
        var itemsField = Assert.Single(quickLinks.SettingsFields, f => f.Name == "items");
        Assert.Equal("array<object>", itemsField.Type);
        Assert.NotNull(itemsField.Fields);
        Assert.Contains(itemsField.Fields!, f => f.Name == "iconKey");
        Assert.Contains(itemsField.Fields!, f => f.Name == "link");
    }

    [Fact]
    public async Task Handle_HeroSlider_HasNoTextsFields()
    {
        var result = await new GetBlockTypesQueryHandler(CreateFullRegistry()).Handle(new GetBlockTypesQuery(), CancellationToken.None);

        var heroSlider = Assert.Single(result.Value, b => b.Key == "hero-slider");
        Assert.Empty(heroSlider.TextsFields);
    }
}
