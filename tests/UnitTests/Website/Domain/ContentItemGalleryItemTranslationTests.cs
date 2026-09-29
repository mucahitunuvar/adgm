using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class ContentItemGalleryItemTranslationTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;

    [Fact]
    public void Create_WithNullOverrides_Succeeds()
    {
        var result = ContentItemGalleryItemTranslation.Create(Tr, null, null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.AltTextOverride);
        Assert.Null(result.Value.CaptionOverride);
    }

    [Fact]
    public void Create_WithBlankOverrides_NormalizesToNull()
    {
        var result = ContentItemGalleryItemTranslation.Create(Tr, "   ", "   ");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.AltTextOverride);
        Assert.Null(result.Value.CaptionOverride);
    }

    [Fact]
    public void Create_WithAltTextOverrideLongerThanMax_Fails()
    {
        var tooLong = new string('a', ContentItemGalleryItemTranslation.MaxAltTextLength + 1);

        var result = ContentItemGalleryItemTranslation.Create(Tr, tooLong, null);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItemGalleryItem.AltTextOverrideTooLong", result.Error.Code);
    }

    [Fact]
    public void Create_WithCaptionOverrideLongerThanMax_Fails()
    {
        var tooLong = new string('a', ContentItemGalleryItemTranslation.MaxCaptionLength + 1);

        var result = ContentItemGalleryItemTranslation.Create(Tr, null, tooLong);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItemGalleryItem.CaptionOverrideTooLong", result.Error.Code);
    }
}
