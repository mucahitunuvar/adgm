using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class LegalDocumentKeyTests
{
    [Theory]
    [InlineData("kvkk-contact")]
    [InlineData("cookie-policy")]
    [InlineData("KVKK-CONTACT")]
    public void Create_WithValidKey_Succeeds(string value)
    {
        var result = LegalDocumentKey.Create(value);

        Assert.True(result.IsSuccess);
        Assert.Equal(value.ToLowerInvariant(), result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("kvkk_contact")]
    [InlineData("kvkk contact")]
    public void Create_WithInvalidKey_Fails(string? value)
    {
        var result = LegalDocumentKey.Create(value);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_LongerThanMaxLength_Fails()
    {
        var result = LegalDocumentKey.Create(new string('a', LegalDocumentKey.MaxLength + 1));

        Assert.True(result.IsFailure);
        Assert.Equal("LegalDocumentKey.InvalidFormat", result.Error.Code);
    }
}
