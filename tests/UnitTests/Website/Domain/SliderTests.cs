using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class SliderTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LanguageCode En = LanguageCode.Create("en").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc);

    private static Slide ValidSlide() =>
        Slide.Create(
            Guid.NewGuid(), null, LinkTarget.CreateEmpty(), 1, true, null, null,
            [SlideTranslation.Create(Tr, null, "Başlık", null, null, null).Value]).Value;

    [Fact]
    public void Create_WithValidKeyAndName_Succeeds()
    {
        var result = Slider.Create("home-hero", Tr, "Ana Sayfa Hero", UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("home-hero", result.Value.Key.Value);
        Assert.Single(result.Value.Translations, t => t.LanguageCode == Tr && t.Name == "Ana Sayfa Hero");
    }

    [Fact]
    public void Create_WithInvalidKey_Fails()
    {
        var result = Slider.Create("Invalid Key", Tr, "Ana Sayfa Hero", UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("SliderKey.InvalidFormat", result.Error.Code);
    }

    [Fact]
    public void Create_WithEmptyName_Fails()
    {
        var result = Slider.Create("home-hero", Tr, string.Empty, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("SliderTranslation.NameInvalid", result.Error.Code);
    }

    [Fact]
    public void ReplaceTranslations_WithValidSet_Succeeds()
    {
        var slider = Slider.Create("home-hero", Tr, "Ana Sayfa Hero", UserId, Now).Value;

        var result = slider.ReplaceTranslations(
            [SliderTranslation.Create(Tr, "Yeni Ad").Value, SliderTranslation.Create(En, "New Name").Value], UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, slider.Translations.Count);
    }

    [Fact]
    public void ReplaceTranslations_WithDuplicateLanguage_Fails()
    {
        var slider = Slider.Create("home-hero", Tr, "Ana Sayfa Hero", UserId, Now).Value;

        var result = slider.ReplaceTranslations(
            [SliderTranslation.Create(Tr, "Bir").Value, SliderTranslation.Create(Tr, "İki").Value], UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Slider.DuplicateTranslationLanguage", result.Error.Code);
    }

    [Fact]
    public void ReplaceTranslations_WithEmptyList_Fails()
    {
        var slider = Slider.Create("home-hero", Tr, "Ana Sayfa Hero", UserId, Now).Value;

        var result = slider.ReplaceTranslations([], UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Slider.TranslationsRequired", result.Error.Code);
    }

    [Fact]
    public void ReplaceSlides_WithinLimit_Succeeds()
    {
        var slider = Slider.Create("home-hero", Tr, "Ana Sayfa Hero", UserId, Now).Value;
        var slides = Enumerable.Range(0, Slider.MaxSlides).Select(_ => ValidSlide()).ToList();

        var result = slider.ReplaceSlides(slides, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(Slider.MaxSlides, slider.Slides.Count);
    }

    [Fact]
    public void ReplaceSlides_WithMoreThanMaxSlides_Fails()
    {
        var slider = Slider.Create("home-hero", Tr, "Ana Sayfa Hero", UserId, Now).Value;
        var slides = Enumerable.Range(0, Slider.MaxSlides + 1).Select(_ => ValidSlide()).ToList();

        var result = slider.ReplaceSlides(slides, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Slider.TooManySlides", result.Error.Code);
    }
}
