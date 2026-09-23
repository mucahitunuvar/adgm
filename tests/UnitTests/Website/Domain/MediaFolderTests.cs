using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class MediaFolderTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("  ", "")]
    [InlineData("  Logos  ", "Logos")]
    public void Create_WithValidInput_TrimsAndAllowsEmptyForRoot(string? input, string expected)
    {
        var result = MediaFolder.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Fact]
    public void Create_WithNestedPath_Fails()
    {
        var result = MediaFolder.Create("logos/2026");

        Assert.True(result.IsFailure);
        Assert.Equal("MediaFolder.MustBeFlat", result.Error.Code);
    }

    [Fact]
    public void Create_ExceedingMaxLength_Fails()
    {
        var result = MediaFolder.Create(new string('a', MediaFolder.MaxLength + 1));

        Assert.True(result.IsFailure);
        Assert.Equal("MediaFolder.TooLong", result.Error.Code);
    }
}
