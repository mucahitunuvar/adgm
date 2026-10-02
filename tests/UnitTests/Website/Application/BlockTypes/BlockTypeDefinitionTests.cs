using System.Text.Json;
using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Application.BlockTypes;

// §4.2 "Her blok tipi için geçerli bir örnek ve en az bir geçersiz örnek (tablo testi)" - one
// valid/invalid pair per catalog entry, plus the cross-cutting JSON strictness rules (§4.1 "bilinmeyen
// alanlar reddedilir ... tip uyuşmazlığı 400 döner") that every block type shares via
// BlockTypeDefinition<TSettings, TTexts>.
public class BlockTypeDefinitionTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement;

    private static Result ParseWithDefaultTexts(IBlockTypeDefinition definition, string settingsJson, string? textsJson) =>
        ToPlainResult(definition.ParseAndValidate(
            Json(settingsJson),
            textsJson is null ? new Dictionary<LanguageCode, JsonElement>() : new Dictionary<LanguageCode, JsonElement> { [Tr] = Json(textsJson) },
            Tr));

    private static Result ToPlainResult(Result<ParsedBlockContent> result) =>
        result.IsSuccess ? Result.Success() : Result.Failure(result.Error);

    [Fact]
    public void UnknownSettingsField_IsRejected()
    {
        var result = ParseWithDefaultTexts(
            new HeroSliderBlockTypeDefinition(), $"{{\"sliderId\":\"{Guid.NewGuid()}\",\"extra\":1}}", null);

        Assert.True(result.IsFailure);
        Assert.Equal("hero-slider.InvalidSettings", result.Error.Code);
    }

    [Fact]
    public void SettingsTypeMismatch_IsRejected()
    {
        var result = ParseWithDefaultTexts(new HeroSliderBlockTypeDefinition(), "{\"sliderId\":123}", null);

        Assert.True(result.IsFailure);
        Assert.Equal("hero-slider.InvalidSettings", result.Error.Code);
    }

    [Fact]
    public void MissingDefaultLanguageTexts_IsRejected()
    {
        var definition = new LogoStripBlockTypeDefinition();
        var result = ToPlainResult(definition.ParseAndValidate(Json("{\"maxItems\":5}"), new Dictionary<LanguageCode, JsonElement>(), Tr));

        Assert.True(result.IsFailure);
        Assert.Equal("logo-strip.DefaultLanguageTextsRequired", result.Error.Code);
    }

    [Fact]
    public void HeroSlider_Valid_Succeeds()
    {
        var result = ParseWithDefaultTexts(new HeroSliderBlockTypeDefinition(), $"{{\"sliderId\":\"{Guid.NewGuid()}\"}}", null);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void HeroSlider_EmptySliderId_Fails()
    {
        var result = ParseWithDefaultTexts(new HeroSliderBlockTypeDefinition(), "{\"sliderId\":\"00000000-0000-0000-0000-000000000000\"}", null);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void LogoStrip_Valid_Succeeds()
    {
        var result = ParseWithDefaultTexts(new LogoStripBlockTypeDefinition(), "{\"maxItems\":10}", "{\"title\":\"Partnerler\"}");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void LogoStrip_MaxItemsOutOfRange_Fails()
    {
        var result = ParseWithDefaultTexts(new LogoStripBlockTypeDefinition(), "{\"maxItems\":0}", "{\"title\":\"Partnerler\"}");
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void QuickLinks_Valid_Succeeds()
    {
        var settings = $"{{\"items\":[{{\"iconKey\":\"star\",\"link\":{{\"kind\":\"ExternalUrl\",\"externalUrl\":\"https://x.com\"}}}}]}}";
        var texts = "{\"items\":[{\"label\":\"Link\"}]}";

        var result = ParseWithDefaultTexts(new QuickLinksBlockTypeDefinition(), settings, texts);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void QuickLinks_TextsItemCountMismatch_Fails()
    {
        var settings = $"{{\"items\":[{{\"iconKey\":\"star\",\"link\":{{\"kind\":\"ExternalUrl\",\"externalUrl\":\"https://x.com\"}}}}]}}";
        var texts = "{\"items\":[]}";

        var result = ParseWithDefaultTexts(new QuickLinksBlockTypeDefinition(), settings, texts);

        Assert.True(result.IsFailure);
        Assert.Equal("quick-links.ItemCountMismatch", result.Error.Code);
    }

    [Fact]
    public void ContentList_Valid_Succeeds()
    {
        var result = ParseWithDefaultTexts(
            new ContentListBlockTypeDefinition(),
            "{\"contentTypeKey\":\"haber\",\"count\":6,\"featuredOnly\":false,\"categoryId\":null,\"view\":\"cards\"}",
            "{\"title\":\"Haberler\",\"moreLabel\":null}");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ContentList_InvalidView_Fails()
    {
        var result = ParseWithDefaultTexts(
            new ContentListBlockTypeDefinition(),
            "{\"contentTypeKey\":\"haber\",\"count\":6,\"featuredOnly\":false,\"categoryId\":null,\"view\":\"grid\"}",
            "{\"title\":\"Haberler\",\"moreLabel\":null}");

        Assert.True(result.IsFailure);
        Assert.Equal("content-list.ViewInvalid", result.Error.Code);
    }

    [Fact]
    public void UpcomingEvents_Valid_Succeeds()
    {
        var result = ParseWithDefaultTexts(
            new UpcomingEventsBlockTypeDefinition(), "{\"contentTypeKeys\":[\"etkinlik\"],\"count\":3}", "{\"title\":\"Etkinlikler\",\"moreLabel\":null}");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void UpcomingEvents_EmptyContentTypeKeys_Fails()
    {
        var result = ParseWithDefaultTexts(
            new UpcomingEventsBlockTypeDefinition(), "{\"contentTypeKeys\":[],\"count\":3}", "{\"title\":\"Etkinlikler\",\"moreLabel\":null}");

        Assert.True(result.IsFailure);
        Assert.Equal("upcoming-events.ContentTypeKeysRequired", result.Error.Code);
    }

    [Fact]
    public void JobList_Valid_Succeeds()
    {
        var result = ParseWithDefaultTexts(new JobListBlockTypeDefinition(), "{\"count\":4}", "{\"title\":\"İlanlar\",\"moreLabel\":null}");
        Assert.True(result.IsSuccess);
        Assert.Equal([PageLayoutTargetKind.Home], new JobListBlockTypeDefinition().AllowedTargets);
    }

    [Fact]
    public void JobList_CountOutOfRange_Fails()
    {
        var result = ParseWithDefaultTexts(new JobListBlockTypeDefinition(), "{\"count\":0}", "{\"title\":\"İlanlar\",\"moreLabel\":null}");
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void FeatureMosaic_Valid_Succeeds()
    {
        var settings =
            $"{{\"items\":[{{\"imageMediaId\":null,\"link\":{{\"kind\":\"ExternalUrl\",\"externalUrl\":\"https://x.com\"}}}}]}}";
        var texts = "{\"items\":[{\"eyebrow\":null,\"title\":\"Başlık\"}]}";

        var result = ParseWithDefaultTexts(new FeatureMosaicBlockTypeDefinition(), settings, texts);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void FeatureMosaic_ItemWithoutLink_Fails()
    {
        var settings = "{\"items\":[{\"imageMediaId\":null,\"link\":{\"kind\":\"None\"}}]}";
        var texts = "{\"items\":[{\"eyebrow\":null,\"title\":\"Başlık\"}]}";

        var result = ParseWithDefaultTexts(new FeatureMosaicBlockTypeDefinition(), settings, texts);

        Assert.True(result.IsFailure);
        Assert.Equal("feature-mosaic.LinkRequired", result.Error.Code);
    }

    [Fact]
    public void ProcessSteps_Valid_Succeeds()
    {
        var texts = "{\"title\":\"Süreç\",\"steps\":[{\"title\":\"1\",\"text\":\"a\"},{\"title\":\"2\",\"text\":\"b\"}]}";

        var result = ParseWithDefaultTexts(new ProcessStepsBlockTypeDefinition(), "{\"stepCount\":2}", texts);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ProcessSteps_StepsCountMismatch_Fails()
    {
        var texts = "{\"title\":\"Süreç\",\"steps\":[{\"title\":\"1\",\"text\":\"a\"}]}";

        var result = ParseWithDefaultTexts(new ProcessStepsBlockTypeDefinition(), "{\"stepCount\":2}", texts);

        Assert.True(result.IsFailure);
        Assert.Equal("process-steps.StepCountMismatch", result.Error.Code);
    }

    [Fact]
    public void VideoFeature_Valid_Succeeds()
    {
        var texts =
            $"{{\"eyebrow\":null,\"title\":\"Video\",\"text\":null,\"link\":{{\"kind\":\"ExternalUrl\",\"externalUrl\":\"https://x.com\"}},\"linkLabel\":\"İzle\"}}";

        var result = ParseWithDefaultTexts(new VideoFeatureBlockTypeDefinition(), $"{{\"videoId\":\"{Guid.NewGuid()}\"}}", texts);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void VideoFeature_LinkLabelWithoutLink_Fails()
    {
        var texts = "{\"eyebrow\":null,\"title\":\"Video\",\"text\":null,\"link\":null,\"linkLabel\":\"İzle\"}";

        var result = ParseWithDefaultTexts(new VideoFeatureBlockTypeDefinition(), $"{{\"videoId\":\"{Guid.NewGuid()}\"}}", texts);

        Assert.True(result.IsFailure);
        Assert.Equal("video-feature.LinkLabelRequiresLink", result.Error.Code);
    }

    [Fact]
    public void ImpactStats_Valid_Succeeds()
    {
        var result = ParseWithDefaultTexts(new ImpactStatsBlockTypeDefinition(), $"{{\"metricIds\":[\"{Guid.NewGuid()}\"]}}", "{\"title\":null}");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ImpactStats_DuplicateMetricId_Fails()
    {
        var id = Guid.NewGuid();
        var result = ParseWithDefaultTexts(new ImpactStatsBlockTypeDefinition(), $"{{\"metricIds\":[\"{id}\",\"{id}\"]}}", "{\"title\":null}");

        Assert.True(result.IsFailure);
        Assert.Equal("impact-stats.DuplicateMetricId", result.Error.Code);
    }

    [Fact]
    public void Cta_Valid_Succeeds()
    {
        var settings = "{\"buttons\":[{\"link\":{\"kind\":\"ExternalUrl\",\"externalUrl\":\"https://x.com\"},\"style\":\"primary\"}]}";
        var texts = "{\"eyebrow\":null,\"title\":\"Başlık\",\"buttons\":[{\"label\":\"Git\"}]}";

        var result = ParseWithDefaultTexts(new CtaBlockTypeDefinition(), settings, texts);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Cta_InvalidStyle_Fails()
    {
        var settings = "{\"buttons\":[{\"link\":{\"kind\":\"ExternalUrl\",\"externalUrl\":\"https://x.com\"},\"style\":\"loud\"}]}";
        var texts = "{\"eyebrow\":null,\"title\":\"Başlık\",\"buttons\":[{\"label\":\"Git\"}]}";

        var result = ParseWithDefaultTexts(new CtaBlockTypeDefinition(), settings, texts);

        Assert.True(result.IsFailure);
        Assert.Equal("cta.StyleInvalid", result.Error.Code);
    }

    [Fact]
    public void RichText_Valid_Succeeds()
    {
        var definition = new RichTextBlockTypeDefinition(new FakeHtmlContentSanitizer());
        var result = ParseWithDefaultTexts(definition, "{}", "{\"body\":\"<p>metin</p>\"}");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void RichText_EmptyBody_Fails()
    {
        var definition = new RichTextBlockTypeDefinition(new FakeHtmlContentSanitizer());
        var result = ParseWithDefaultTexts(definition, "{}", "{\"body\":\"\"}");

        Assert.True(result.IsFailure);
        Assert.Equal("rich-text.BodyRequired", result.Error.Code);
    }

    [Fact]
    public void ImageText_Valid_Succeeds()
    {
        var definition = new ImageTextBlockTypeDefinition(new FakeHtmlContentSanitizer());
        var settings = $"{{\"imageMediaId\":\"{Guid.NewGuid()}\",\"imagePosition\":\"left\",\"link\":null}}";
        var texts = "{\"title\":\"Başlık\",\"body\":\"<p>x</p>\",\"linkLabel\":null}";

        var result = ParseWithDefaultTexts(definition, settings, texts);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ImageText_InvalidImagePosition_Fails()
    {
        var definition = new ImageTextBlockTypeDefinition(new FakeHtmlContentSanitizer());
        var settings = $"{{\"imageMediaId\":\"{Guid.NewGuid()}\",\"imagePosition\":\"center\",\"link\":null}}";
        var texts = "{\"title\":\"Başlık\",\"body\":\"<p>x</p>\",\"linkLabel\":null}";

        var result = ParseWithDefaultTexts(definition, settings, texts);

        Assert.True(result.IsFailure);
        Assert.Equal("image-text.ImagePositionInvalid", result.Error.Code);
    }

    [Fact]
    public void Faq_Valid_Succeeds()
    {
        var result = ParseWithDefaultTexts(
            new FaqBlockTypeDefinition(), "{\"contentTypeKey\":\"faq\",\"categoryId\":null,\"count\":10}", "{\"title\":null}");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Faq_CountOutOfRange_Fails()
    {
        var result = ParseWithDefaultTexts(
            new FaqBlockTypeDefinition(), "{\"contentTypeKey\":\"faq\",\"categoryId\":null,\"count\":0}", "{\"title\":null}");
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Gallery_Valid_Succeeds()
    {
        var result = ParseWithDefaultTexts(new GalleryBlockTypeDefinition(), $"{{\"mediaIds\":[\"{Guid.NewGuid()}\"]}}", "{\"title\":null}");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Gallery_DuplicateMediaId_Fails()
    {
        var id = Guid.NewGuid();
        var result = ParseWithDefaultTexts(new GalleryBlockTypeDefinition(), $"{{\"mediaIds\":[\"{id}\",\"{id}\"]}}", "{\"title\":null}");

        Assert.True(result.IsFailure);
        Assert.Equal("gallery.DuplicateMediaId", result.Error.Code);
    }
}
