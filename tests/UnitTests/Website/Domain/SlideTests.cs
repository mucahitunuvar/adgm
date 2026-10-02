using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class SlideTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly DateTime Now = new(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc);

    private static SlideTranslation Translation(string? buttonLabel = null) =>
        SlideTranslation.Create(Tr, null, "Başlık", null, buttonLabel, null).Value;

    [Fact]
    public void Create_WithValidInput_Succeeds()
    {
        var result = Slide.Create(Guid.NewGuid(), null, LinkTarget.CreateEmpty(), 1, true, null, null, [Translation()]);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithEmptyDesktopImage_Fails()
    {
        var result = Slide.Create(Guid.Empty, null, LinkTarget.CreateEmpty(), 1, true, null, null, [Translation()]);

        Assert.True(result.IsFailure);
        Assert.Equal("Slide.DesktopImageRequired", result.Error.Code);
    }

    [Fact]
    public void Create_WithNoTranslations_Fails()
    {
        var result = Slide.Create(Guid.NewGuid(), null, LinkTarget.CreateEmpty(), 1, true, null, null, []);

        Assert.True(result.IsFailure);
        Assert.Equal("Slide.TranslationsRequired", result.Error.Code);
    }

    [Fact]
    public void Create_WithUnpublishAtBeforePublishAt_Fails()
    {
        var publishAt = Now;
        var unpublishAt = Now.AddHours(-1);

        var result = Slide.Create(Guid.NewGuid(), null, LinkTarget.CreateEmpty(), 1, true, publishAt, unpublishAt, [Translation()]);

        Assert.True(result.IsFailure);
        Assert.Equal("Slide.UnpublishMustBeAfterPublish", result.Error.Code);
    }

    [Fact]
    public void Create_WithUnpublishAtEqualToPublishAt_Fails()
    {
        var result = Slide.Create(Guid.NewGuid(), null, LinkTarget.CreateEmpty(), 1, true, Now, Now, [Translation()]);

        Assert.True(result.IsFailure);
        Assert.Equal("Slide.UnpublishMustBeAfterPublish", result.Error.Code);
    }

    [Fact]
    public void Create_WithUnpublishAtAfterPublishAt_Succeeds()
    {
        var result = Slide.Create(Guid.NewGuid(), null, LinkTarget.CreateEmpty(), 1, true, Now, Now.AddHours(1), [Translation()]);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithButtonLabelButNoLinkTarget_Fails()
    {
        var result = Slide.Create(Guid.NewGuid(), null, LinkTarget.CreateEmpty(), 1, true, null, null, [Translation("Devamını Gör")]);

        Assert.True(result.IsFailure);
        Assert.Equal("Slide.ButtonLabelRequiresLink", result.Error.Code);
    }

    [Fact]
    public void Create_WithButtonLabelAndLinkTarget_Succeeds()
    {
        var link = LinkTarget.ForExternalUrl("https://example.com").Value;

        var result = Slide.Create(Guid.NewGuid(), null, link, 1, true, null, null, [Translation("Devamını Gör")]);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithLinkTargetButNoButtonLabel_Succeeds()
    {
        var link = LinkTarget.ForExternalUrl("https://example.com").Value;

        var result = Slide.Create(Guid.NewGuid(), null, link, 1, true, null, null, [Translation()]);

        Assert.True(result.IsSuccess);
    }
}
