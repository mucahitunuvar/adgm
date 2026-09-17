using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.SharedKernel.Results;

public class PagedRequestTests
{
    [Fact]
    public void Page_DefaultsToOne()
    {
        Assert.Equal(1, new PagedRequest().Page);
    }

    [Fact]
    public void PageSize_DefaultsToTwenty()
    {
        Assert.Equal(20, new PagedRequest().PageSize);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(int.MinValue, 1)]
    [InlineData(1, 1)]
    [InlineData(5, 5)]
    public void Page_OutOfRangeValues_AreClampedToMinimum(int input, int expected)
    {
        Assert.Equal(expected, new PagedRequest { Page = input }.Page);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(1, 1)]
    [InlineData(50, 50)]
    [InlineData(100, 100)]
    [InlineData(101, 100)]
    [InlineData(500, 100)]
    [InlineData(int.MaxValue, 100)]
    public void PageSize_OutOfRangeValues_AreClampedToValidBounds(int input, int expected)
    {
        Assert.Equal(expected, new PagedRequest { PageSize = input }.PageSize);
    }

    [Fact]
    public void Clamping_DoesNotThrow_ForAnyIntegerValue()
    {
        var exception = Record.Exception(() => new PagedRequest { Page = -100, PageSize = int.MaxValue });

        Assert.Null(exception);
    }
}
