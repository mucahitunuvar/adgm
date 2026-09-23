using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class LanguageCodeTests
{
    [Theory]
    [InlineData("tr", "tr")]
    [InlineData("EN", "en")]
    [InlineData("pt-BR", "pt-br")]
    [InlineData("  tr  ", "tr")]
    public void Create_WithValidInput_NormalizesToLowercaseTrimmed(string input, string expected)
    {
        var result = LanguageCode.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyInput_Fails(string? input)
    {
        var result = LanguageCode.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("LanguageCode.Required", result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Theory]
    [InlineData("t")]
    [InlineData("toolongsubtagxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")]
    [InlineData("tr_TR")]
    [InlineData("123")]
    [InlineData("tr#")]
    public void Create_WithInvalidFormat_Fails(string input)
    {
        var result = LanguageCode.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("LanguageCode.InvalidFormat", result.Error.Code);
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        var first = LanguageCode.Create("tr").Value;
        var second = LanguageCode.Create("TR").Value;

        Assert.Equal(first, second);
        Assert.True(first == second);
    }
}
