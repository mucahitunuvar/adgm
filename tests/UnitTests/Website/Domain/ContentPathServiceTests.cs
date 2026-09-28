using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class ContentPathServiceTests
{
    private static readonly Guid ItemId = Guid.NewGuid();
    private static readonly Guid ContentTypeId = Guid.NewGuid();
    private static readonly Guid ParentId = Guid.NewGuid();

    [Fact]
    public void ValidateParentAssignment_WithNullParent_Succeeds()
    {
        var result = ContentPathService.ValidateParentAssignment(ItemId, ContentTypeId, true, null, null, 0, [], 0);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateParentAssignment_WhenHierarchyNotSupported_Fails()
    {
        var result = ContentPathService.ValidateParentAssignment(
            ItemId, ContentTypeId, contentTypeSupportsHierarchy: false, ParentId, ContentTypeId, 1, [], 0);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.HierarchyNotSupported", result.Error.Code);
    }

    [Fact]
    public void ValidateParentAssignment_WhenParentIsItemItself_Fails()
    {
        var result = ContentPathService.ValidateParentAssignment(ItemId, ContentTypeId, true, ItemId, ContentTypeId, 1, [], 0);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.CyclicHierarchy", result.Error.Code);
    }

    [Fact]
    public void ValidateParentAssignment_WhenParentIsOwnDescendant_Fails()
    {
        var result = ContentPathService.ValidateParentAssignment(ItemId, ContentTypeId, true, ParentId, ContentTypeId, 1, [ParentId], 0);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.CyclicHierarchy", result.Error.Code);
    }

    [Fact]
    public void ValidateParentAssignment_WhenParentIsDifferentContentType_Fails()
    {
        var result = ContentPathService.ValidateParentAssignment(
            ItemId, ContentTypeId, true, ParentId, Guid.NewGuid(), 1, [], 0);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.ParentMustBeSameContentType", result.Error.Code);
    }

    [Theory]
    [InlineData(1, 0, true)]  // item becomes depth 2, no descendants - within MaxHierarchyDepth (3)
    [InlineData(2, 0, true)]  // item becomes depth 3 - exactly at the limit
    [InlineData(3, 0, false)] // item would become depth 4 - exceeds the limit
    [InlineData(1, 1, true)]  // item depth 2 + one level of its own descendants = depth 3 - still fits
    [InlineData(1, 2, false)] // item depth 2 + two levels of descendants = depth 4 - exceeds
    public void ValidateParentAssignment_EnforcesMaxHierarchyDepth(int parentDepth, int itemSubtreeHeight, bool expectSuccess)
    {
        var result = ContentPathService.ValidateParentAssignment(
            ItemId, ContentTypeId, true, ParentId, ContentTypeId, parentDepth, [], itemSubtreeHeight);

        Assert.Equal(expectSuccess, result.IsSuccess);
        if (!expectSuccess)
        {
            Assert.Equal("ContentItem.MaxHierarchyDepthExceeded", result.Error.Code);
        }
    }

    [Fact]
    public void ValidateParentAssignment_WithValidParent_Succeeds()
    {
        var result = ContentPathService.ValidateParentAssignment(ItemId, ContentTypeId, true, ParentId, ContentTypeId, 1, [], 0);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ComputeFullPath_WithRoutePrefixNoAncestors_JoinsPrefixAndSlug()
    {
        var fullPath = ContentPathService.ComputeFullPath("haberler", [], "yeni-haber");

        Assert.Equal("haberler/yeni-haber", fullPath);
    }

    [Fact]
    public void ComputeFullPath_WithEmptyRoutePrefixNoAncestors_IsJustTheSlug()
    {
        var fullPath = ContentPathService.ComputeFullPath(string.Empty, [], "hakkimizda");

        Assert.Equal("hakkimizda", fullPath);
    }

    [Fact]
    public void ComputeFullPath_WithAncestors_JoinsInRootToLeafOrder()
    {
        var fullPath = ContentPathService.ComputeFullPath("haberler", ["kategori-a", "kategori-b"], "alt-haber");

        Assert.Equal("haberler/kategori-a/kategori-b/alt-haber", fullPath);
    }

    [Fact]
    public void ComputeFullPath_WithEmptyRoutePrefixAndAncestors_OmitsThePrefixSegment()
    {
        var fullPath = ContentPathService.ComputeFullPath(string.Empty, ["ust-sayfa"], "alt-sayfa");

        Assert.Equal("ust-sayfa/alt-sayfa", fullPath);
    }

    [Fact]
    public void IsVisibleConsideringAncestors_TrueOnlyWhenSelfAndEveryAncestorAreVisible()
    {
        Assert.True(ContentPathService.IsVisibleConsideringAncestors(true, [true, true]));
        Assert.False(ContentPathService.IsVisibleConsideringAncestors(true, [true, false]));
        Assert.False(ContentPathService.IsVisibleConsideringAncestors(false, [true, true]));
        Assert.True(ContentPathService.IsVisibleConsideringAncestors(true, []));
    }
}
