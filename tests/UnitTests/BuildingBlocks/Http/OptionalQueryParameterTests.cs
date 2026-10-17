using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.BuildingBlocks.Http;

public class OptionalQueryParameterTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_WithEmptyOrWhitespaceValue_SucceedsWithNull(string? rawValue)
    {
        var result = OptionalQueryParameter.Parse<Guid>(rawValue, "provinceId");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Parse_Guid_WithValidValue_SucceedsWithParsedValue()
    {
        var guid = Guid.NewGuid();

        var result = OptionalQueryParameter.Parse<Guid>(guid.ToString(), "provinceId");

        Assert.True(result.IsSuccess);
        Assert.Equal(guid, result.Value);
    }

    [Fact]
    public void Parse_Guid_WithMalformedValue_FailsWithValidationError()
    {
        var result = OptionalQueryParameter.Parse<Guid>("not-a-guid", "provinceId");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("Query.InvalidProvinceId", result.Error.Code);
    }

    [Theory]
    [InlineData("1", 1)]
    [InlineData("-5", -5)]
    public void Parse_Int_WithValidValue_SucceedsWithParsedValue(string rawValue, int expected)
    {
        var result = OptionalQueryParameter.Parse<int>(rawValue, "page");

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void Parse_Int_WithMalformedValue_FailsWithValidationError()
    {
        var result = OptionalQueryParameter.Parse<int>("abc", "page");

        Assert.True(result.IsFailure);
        Assert.Equal("Query.InvalidPage", result.Error.Code);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void Parse_Bool_WithValidValue_SucceedsWithParsedValue(string rawValue, bool expected)
    {
        var result = OptionalQueryParameter.Parse<bool>(rawValue, "isForDisabledCandidates");

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void Parse_Bool_WithMalformedValue_FailsWithValidationError()
    {
        var result = OptionalQueryParameter.Parse<bool>("maybe", "isForDisabledCandidates");

        Assert.True(result.IsFailure);
        Assert.Equal("Query.InvalidIsForDisabledCandidates", result.Error.Code);
    }

    [Fact]
    public void Parse_DateTime_WithValidValue_SucceedsWithParsedValue()
    {
        var result = OptionalQueryParameter.Parse<DateTime>("2026-01-15T10:00:00Z", "from");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public void Parse_DateTime_WithMalformedValue_FailsWithValidationError()
    {
        var result = OptionalQueryParameter.Parse<DateTime>("not-a-date", "from");

        Assert.True(result.IsFailure);
        Assert.Equal("Query.InvalidFrom", result.Error.Code);
    }
}
