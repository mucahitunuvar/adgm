using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class FormDefinitionKeyTests
{
    [Theory]
    [InlineData("contact-form")]
    [InlineData("volunteer")]
    [InlineData("CONTACT-FORM")]
    public void Create_WithValidKey_Succeeds(string value)
    {
        var result = FormDefinitionKey.Create(value);

        Assert.True(result.IsSuccess);
        Assert.Equal(value.ToLowerInvariant(), result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("contact_form")]
    [InlineData("contact form")]
    public void Create_WithInvalidKey_Fails(string? value)
    {
        var result = FormDefinitionKey.Create(value);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_LongerThanMaxLength_Fails()
    {
        var result = FormDefinitionKey.Create(new string('a', FormDefinitionKey.MaxLength + 1));

        Assert.True(result.IsFailure);
        Assert.Equal("FormDefinitionKey.InvalidFormat", result.Error.Code);
    }
}
