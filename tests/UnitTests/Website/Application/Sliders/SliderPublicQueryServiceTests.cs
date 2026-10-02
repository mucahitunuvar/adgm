using GenclikMerkezi.Modules.Website.Application.LinkTargets;
using GenclikMerkezi.Modules.Website.Application.Sliders;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Application.Sliders;

public class SliderPublicQueryServiceTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LanguageCode En = LanguageCode.Create("en").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeSliderRepository _sliderRepository = new();
    private readonly FakeMediaAssetRepository _mediaAssetRepository = new();
    private readonly FakeMediaFileStorageService _fileStorageService = new();
    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeContentTypeRepository _contentTypeRepository = new();

    private SliderPublicQueryService CreateService() =>
        new(_sliderRepository, _mediaAssetRepository, _fileStorageService, new LinkTargetResolver(_contentItemRepository, _contentTypeRepository));

    private Guid SeedImage(string? altText = null)
    {
        var id = Guid.NewGuid();
        var file = FileAttachment.Create($"website-images/{id}.jpg", "slide.jpg", "image/jpeg", 1024, Now, "MediaAsset", id);
        var image = MediaAsset.Create(
            id, MediaAssetKind.Image, file, [], 1600, 900, MediaFolder.Create("sliders").Value, null, null, false, UserId, Now).Value;

        if (altText is not null)
        {
            image.SetTranslation(Tr, altText, null, UserId, Now);
        }

        _mediaAssetRepository.Seed(image);
        return id;
    }

    private static Slide BuildSlide(
        Guid desktopImageId,
        bool isActive = true,
        DateTime? publishAtUtc = null,
        DateTime? unpublishAtUtc = null,
        int sortOrder = 1,
        LinkTarget? linkTarget = null,
        string? buttonLabel = null,
        string? altTextOverride = null,
        LanguageCode? translationLanguage = null) =>
        Slide.Create(
            desktopImageId, null, linkTarget ?? LinkTarget.CreateEmpty(), sortOrder, isActive, publishAtUtc, unpublishAtUtc,
            [SlideTranslation.Create(translationLanguage ?? Tr, null, "Başlık", null, buttonLabel, altTextOverride).Value]).Value;

    private Slider SeedSlider(params Slide[] slides)
    {
        var slider = Slider.Create("home-hero", Tr, "Ana Sayfa Hero", UserId, Now).Value;
        slider.ReplaceSlides(slides, UserId, Now);
        _sliderRepository.Seed(slider);
        return slider;
    }

    [Fact]
    public async Task GetVisibleSlidesAsync_ReturnsActiveSlidesOrderedBySortOrder()
    {
        var image = SeedImage();
        var slider = SeedSlider(BuildSlide(image, sortOrder: 2), BuildSlide(image, sortOrder: 1));

        var result = await CreateService().GetVisibleSlidesAsync(slider.Id, Tr, Tr, Now);

        Assert.Equal(2, result.Count);
        Assert.True(result[0].Id != result[1].Id);
    }

    [Fact]
    public async Task GetVisibleSlidesAsync_ExcludesInactiveSlides()
    {
        var image = SeedImage();
        var slider = SeedSlider(BuildSlide(image, isActive: false));

        var result = await CreateService().GetVisibleSlidesAsync(slider.Id, Tr, Tr, Now);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetVisibleSlidesAsync_ExcludesSlidesScheduledInTheFuture()
    {
        var image = SeedImage();
        var slider = SeedSlider(BuildSlide(image, publishAtUtc: Now.AddDays(1)));

        var result = await CreateService().GetVisibleSlidesAsync(slider.Id, Tr, Tr, Now);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetVisibleSlidesAsync_ExcludesSlidesWithoutTranslationInRequestedLanguage()
    {
        var image = SeedImage();
        var slider = SeedSlider(BuildSlide(image, translationLanguage: Tr));

        var result = await CreateService().GetVisibleSlidesAsync(slider.Id, En, Tr, Now);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetVisibleSlidesAsync_WhenLinkResolves_IncludesButtonLabelAndHref()
    {
        var image = SeedImage();
        var link = LinkTarget.ForExternalUrl("https://example.com/campaign").Value;
        var slider = SeedSlider(BuildSlide(image, linkTarget: link, buttonLabel: "Devamını Gör"));

        var result = await CreateService().GetVisibleSlidesAsync(slider.Id, Tr, Tr, Now);

        var slide = Assert.Single(result);
        Assert.Equal("Devamını Gör", slide.ButtonLabel);
        Assert.Equal("https://example.com/campaign", slide.ButtonHref);
    }

    [Fact]
    public async Task GetVisibleSlidesAsync_WhenLinkDoesNotResolve_HidesButtonButKeepsSlide()
    {
        var image = SeedImage();
        var link = LinkTarget.ForContent(Guid.NewGuid()).Value;
        var slider = SeedSlider(BuildSlide(image, linkTarget: link, buttonLabel: "Devamını Gör"));

        var result = await CreateService().GetVisibleSlidesAsync(slider.Id, Tr, Tr, Now);

        var slide = Assert.Single(result);
        Assert.Null(slide.ButtonLabel);
        Assert.Null(slide.ButtonHref);
    }

    [Fact]
    public async Task GetVisibleSlidesAsync_AltText_PrefersOverrideOverMediaAssetAltText()
    {
        var image = SeedImage(altText: "Kütüphane görseli");
        var slider = SeedSlider(BuildSlide(image, altTextOverride: "Özel açıklama"));

        var result = await CreateService().GetVisibleSlidesAsync(slider.Id, Tr, Tr, Now);

        Assert.Equal("Özel açıklama", Assert.Single(result).AltText);
    }

    [Fact]
    public async Task GetVisibleSlidesAsync_AltText_FallsBackToMediaAssetAltTextWhenNoOverride()
    {
        var image = SeedImage(altText: "Kütüphane görseli");
        var slider = SeedSlider(BuildSlide(image));

        var result = await CreateService().GetVisibleSlidesAsync(slider.Id, Tr, Tr, Now);

        Assert.Equal("Kütüphane görseli", Assert.Single(result).AltText);
    }

    [Fact]
    public async Task GetVisibleSlidesAsync_AltText_FallsBackToEmptyWhenNeitherIsSet()
    {
        var image = SeedImage();
        var slider = SeedSlider(BuildSlide(image));

        var result = await CreateService().GetVisibleSlidesAsync(slider.Id, Tr, Tr, Now);

        Assert.Equal(string.Empty, Assert.Single(result).AltText);
    }
}
