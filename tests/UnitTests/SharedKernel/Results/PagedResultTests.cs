using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.SharedKernel.Results;

public class PagedResultTests
{
    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(1, 20, 1)]
    [InlineData(20, 20, 1)]
    [InlineData(21, 20, 2)]
    [InlineData(100, 20, 5)]
    public void TotalPages_IsCalculatedFromTotalCountAndPageSize(int totalCount, int pageSize, int expectedTotalPages)
    {
        var result = new PagedResult<int>([], totalCount, Page: 1, PageSize: pageSize);

        Assert.Equal(expectedTotalPages, result.TotalPages);
    }

    [Fact]
    public void HasPreviousPage_IsFalse_OnFirstPage()
    {
        var result = new PagedResult<int>([], TotalCount: 50, Page: 1, PageSize: 20);

        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public void HasPreviousPage_IsTrue_AfterFirstPage()
    {
        var result = new PagedResult<int>([], TotalCount: 50, Page: 2, PageSize: 20);

        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public void HasNextPage_IsTrue_WhenMorePagesRemain()
    {
        var result = new PagedResult<int>([], TotalCount: 50, Page: 1, PageSize: 20);

        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void HasNextPage_IsFalse_OnLastPage()
    {
        var result = new PagedResult<int>([], TotalCount: 50, Page: 3, PageSize: 20);

        Assert.False(result.HasNextPage);
    }
}
