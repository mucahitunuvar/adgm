using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class SliderKeyTests
{
    [Theory]
    [InlineData("home-hero")]
    [InlineData("campaign")]
    [InlineData("HOME-HERO")]
    public void Create_WithValidKey_Succeeds(string value)
    {
        var result = SliderKey.Create(value);

        Assert.True(result.IsSuccess);
        Assert.Equal(value.ToLowerInvariant(), result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("home_hero")]
    [InlineData("home hero")]
    public void Create_WithInvalidKey_Fails(string? value)
    {
        var result = SliderKey.Create(value);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_LongerThanMaxLength_Fails()
    {
        var result = SliderKey.Create(new string('a', SliderKey.MaxLength + 1));

        Assert.True(result.IsFailure);
        Assert.Equal("SliderKey.InvalidFormat", result.Error.Code);
    }
}
