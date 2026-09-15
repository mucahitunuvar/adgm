using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Identity.Domain;

public class EmailTests
{
    [Theory]
    [InlineData("User@Example.com", "user@example.com")]
    [InlineData("  aday@genclikmerkezi.com  ", "aday@genclikmerkezi.com")]
    public void Create_WithValidValue_NormalizesToLowercaseAndTrims(string input, string expected)
    {
        var result = Email.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ReturnsValidationError(string? input)
    {
        var result = Email.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing-domain@")]
    [InlineData("@missing-local.com")]
    [InlineData("no-at-sign.com")]
    public void Create_WithInvalidFormat_ReturnsValidationError(string input)
    {
        var result = Email.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void TwoEmails_WithSameNormalizedValue_AreEqual()
    {
        var first = Email.Create("Aday@Example.com").Value;
        var second = Email.Create("aday@example.com").Value;

        Assert.Equal(first, second);
        Assert.True(first == second);
    }
}
