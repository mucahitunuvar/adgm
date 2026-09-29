using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class ContentItemAttachmentTranslationTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;

    [Fact]
    public void Create_WithNullOverride_Succeeds()
    {
        var result = ContentItemAttachmentTranslation.Create(Tr, null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.DisplayNameOverride);
    }

    [Fact]
    public void Create_WithBlankOverride_NormalizesToNull()
    {
        var result = ContentItemAttachmentTranslation.Create(Tr, "   ");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.DisplayNameOverride);
    }

    [Fact]
    public void Create_WithDisplayNameOverrideLongerThanMax_Fails()
    {
        var tooLong = new string('a', ContentItemAttachmentTranslation.MaxDisplayNameLength + 1);

        var result = ContentItemAttachmentTranslation.Create(Tr, tooLong);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItemAttachment.DisplayNameOverrideTooLong", result.Error.Code);
    }
}
