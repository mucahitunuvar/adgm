using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class ContentTypeKeyTests
{
    [Theory]
    [InlineData("news")]
    [InlineData("success-story")]
    [InlineData("volunteer-opportunity")]
    [InlineData("NEWS")]
    public void Create_WithValidKey_Succeeds(string value)
    {
        var result = ContentTypeKey.Create(value);

        Assert.True(result.IsSuccess);
        Assert.Equal(value.ToLowerInvariant(), result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("-news")]
    [InlineData("news-")]
    [InlineData("news--story")]
    [InlineData("news_story")]
    [InlineData("news story")]
    public void Create_WithInvalidKey_Fails(string? value)
    {
        var result = ContentTypeKey.Create(value);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_LongerThanMaxLength_Fails()
    {
        var result = ContentTypeKey.Create(new string('a', ContentTypeKey.MaxLength + 1));

        Assert.True(result.IsFailure);
        Assert.Equal("ContentTypeKey.InvalidFormat", result.Error.Code);
    }
}
