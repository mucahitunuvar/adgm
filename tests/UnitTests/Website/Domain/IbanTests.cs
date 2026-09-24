using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class IbanTests
{
    [Theory]
    [InlineData("TR330006100519786457841326", "TR330006100519786457841326")]
    [InlineData("tr33 0006 1005 1978 6457 8413 26", "TR330006100519786457841326")]
    public void Create_WithValidInput_NormalizesToUppercaseWithoutSpaces(string input, string expected)
    {
        var result = Iban.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Fact]
    public void Create_WithNullOrWhitespace_Fails()
    {
        var result = Iban.Create("   ");

        Assert.True(result.IsFailure);
        Assert.Equal("Iban.Required", result.Error.Code);
    }

    [Theory]
    [InlineData("not-an-iban")]
    [InlineData("TR33")]
    [InlineData("1233330006100519786457841326")]
    public void Create_WithInvalidFormat_Fails(string input)
    {
        var result = Iban.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("Iban.InvalidFormat", result.Error.Code);
    }
}
