using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class PageLayoutTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private static LayoutBlock Block(string typeKey = "rich-text", int sortOrder = 1) =>
        LayoutBlock.Create(typeKey, sortOrder, true, "{}", [LayoutBlockTranslation.Create(Tr, "{\"body\":\"<p>x</p>\"}")]).Value;

    [Fact]
    public void CreateForContent_WithEmptyId_Fails()
    {
        var result = PageLayout.CreateForContent(Guid.Empty, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("PageLayout.ContentItemIdRequired", result.Error.Code);
    }

    [Fact]
    public void ReplaceDraftBlocks_MoreThanMaxBlocks_Fails()
    {
        var layout = PageLayout.CreateForContent(Guid.NewGuid(), UserId, Now).Value;
        var blocks = Enumerable.Range(1, PageLayout.MaxBlocks + 1).Select(i => Block(sortOrder: i)).ToList();

        var result = layout.ReplaceDraftBlocks(blocks, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("PageLayout.TooManyBlocks", result.Error.Code);
    }

    [Fact]
    public void ReplaceDraftBlocks_SetsHasUnpublishedChanges()
    {
        var layout = PageLayout.CreateForContent(Guid.NewGuid(), UserId, Now).Value;

        var result = layout.ReplaceDraftBlocks([Block()], UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.True(layout.HasUnpublishedChanges);
        Assert.Single(layout.DraftBlocks);
        Assert.Empty(layout.PublishedBlocks);
    }

    [Fact]
    public void Publish_CopiesDraftToPublishedAndClearsHasUnpublishedChanges()
    {
        var layout = PageLayout.CreateForContent(Guid.NewGuid(), UserId, Now).Value;
        layout.ReplaceDraftBlocks([Block()], UserId, Now);

        var result = layout.Publish(UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.False(layout.HasUnpublishedChanges);
        Assert.Single(layout.PublishedBlocks);
        Assert.Equal(Now, layout.PublishedAtUtc);
        Assert.Equal(UserId, layout.PublishedByUserId);
        // Draft and published must not share LayoutBlock identity - Publish clones.
        Assert.NotEqual(layout.DraftBlocks[0].Id, layout.PublishedBlocks[0].Id);
    }

    [Fact]
    public void ReplaceDraftBlocks_AfterPublish_SetsHasUnpublishedChangesAgain()
    {
        var layout = PageLayout.CreateForContent(Guid.NewGuid(), UserId, Now).Value;
        layout.ReplaceDraftBlocks([Block()], UserId, Now);
        layout.Publish(UserId, Now);

        layout.ReplaceDraftBlocks([Block(), Block(sortOrder: 2)], UserId, Now);

        Assert.True(layout.HasUnpublishedChanges);
        Assert.Equal(2, layout.DraftBlocks.Count);
        Assert.Single(layout.PublishedBlocks);
    }

    [Fact]
    public void DiscardDraft_ResetsDraftToPublishedAndClearsHasUnpublishedChanges()
    {
        var layout = PageLayout.CreateForContent(Guid.NewGuid(), UserId, Now).Value;
        layout.ReplaceDraftBlocks([Block()], UserId, Now);
        layout.Publish(UserId, Now);
        layout.ReplaceDraftBlocks([Block(), Block(sortOrder: 2)], UserId, Now);

        var result = layout.DiscardDraft(UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.False(layout.HasUnpublishedChanges);
        Assert.Single(layout.DraftBlocks);
        Assert.Single(layout.PublishedBlocks);
    }

    [Fact]
    public void ReplaceDraftBlocks_RegeneratesRowVersion()
    {
        var layout = PageLayout.CreateForContent(Guid.NewGuid(), UserId, Now).Value;
        var originalRowVersion = layout.RowVersion;

        layout.ReplaceDraftBlocks([Block()], UserId, Now);

        Assert.NotEqual(originalRowVersion, layout.RowVersion);
    }
}
