using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class MenuTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc);

    private static MenuItem Item(Guid id, Guid? parentId = null, LinkTarget? linkTarget = null) =>
        MenuItem.Create(
            id, parentId, 1, true, linkTarget ?? LinkTarget.CreateEmpty(), false, null,
            [MenuItemTranslation.Create(Tr, "Etiket").Value]).Value;

    [Fact]
    public void ReplaceItems_WithFlatList_Succeeds()
    {
        var menu = Menu.CreateEmpty(MenuLocation.Header, UserId, Now);
        var items = new[] { Item(Guid.NewGuid()), Item(Guid.NewGuid()) };

        var result = menu.ReplaceItems(items, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, menu.Items.Count);
    }

    [Fact]
    public void ReplaceItems_WithMoreThanMaxItems_Fails()
    {
        var menu = Menu.CreateEmpty(MenuLocation.Header, UserId, Now);
        var items = Enumerable.Range(0, Menu.MaxItems + 1).Select(_ => Item(Guid.NewGuid())).ToList();

        var result = menu.ReplaceItems(items, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Menu.TooManyItems", result.Error.Code);
    }

    [Fact]
    public void ReplaceItems_WithDuplicateId_Fails()
    {
        var menu = Menu.CreateEmpty(MenuLocation.Header, UserId, Now);
        var id = Guid.NewGuid();
        var items = new[] { Item(id), Item(id) };

        var result = menu.ReplaceItems(items, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Menu.DuplicateItemId", result.Error.Code);
    }

    [Fact]
    public void ReplaceItems_WithUnknownParent_Fails()
    {
        var menu = Menu.CreateEmpty(MenuLocation.Header, UserId, Now);
        var items = new[] { Item(Guid.NewGuid(), Guid.NewGuid()) };

        var result = menu.ReplaceItems(items, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Menu.ParentNotFound", result.Error.Code);
    }

    [Fact]
    public void ReplaceItems_WithCycle_Fails()
    {
        var menu = Menu.CreateEmpty(MenuLocation.Header, UserId, Now);
        var aId = Guid.NewGuid();
        var bId = Guid.NewGuid();
        var items = new[] { Item(aId, bId), Item(bId, aId) };

        var result = menu.ReplaceItems(items, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Menu.CyclicReference", result.Error.Code);
    }

    [Fact]
    public void ReplaceItems_WithThreeLevels_Succeeds()
    {
        var menu = Menu.CreateEmpty(MenuLocation.Header, UserId, Now);
        var rootId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var grandchildId = Guid.NewGuid();
        var items = new[] { Item(rootId), Item(childId, rootId), Item(grandchildId, childId) };

        var result = menu.ReplaceItems(items, UserId, Now);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ReplaceItems_WithFourLevels_Fails()
    {
        var menu = Menu.CreateEmpty(MenuLocation.Header, UserId, Now);
        var rootId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var grandchildId = Guid.NewGuid();
        var greatGrandchildId = Guid.NewGuid();
        var items = new[]
        {
            Item(rootId), Item(childId, rootId), Item(grandchildId, childId), Item(greatGrandchildId, grandchildId),
        };

        var result = menu.ReplaceItems(items, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Menu.MaxDepthExceeded", result.Error.Code);
    }

    [Fact]
    public void ReplaceItems_OnUtilityMenu_WithNestedItem_Fails()
    {
        var menu = Menu.CreateEmpty(MenuLocation.Utility, UserId, Now);
        var parentId = Guid.NewGuid();
        var items = new[] { Item(parentId), Item(Guid.NewGuid(), parentId) };

        var result = menu.ReplaceItems(items, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Menu.UtilityMustBeSingleLevel", result.Error.Code);
    }

    [Fact]
    public void ReplaceItems_OnUtilityMenu_WithFlatItems_Succeeds()
    {
        var menu = Menu.CreateEmpty(MenuLocation.Utility, UserId, Now);
        var items = new[] { Item(Guid.NewGuid()), Item(Guid.NewGuid()) };

        var result = menu.ReplaceItems(items, UserId, Now);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void DeactivateItemsLinkingToContent_ClearsLinkAndDeactivatesMatchingItems()
    {
        var menu = Menu.CreateEmpty(MenuLocation.Header, UserId, Now);
        var contentId = Guid.NewGuid();
        var linkedItemId = Guid.NewGuid();
        var otherItemId = Guid.NewGuid();
        menu.ReplaceItems(
            [Item(linkedItemId, linkTarget: LinkTarget.ForContent(contentId).Value), Item(otherItemId)], UserId, Now);

        var affected = menu.DeactivateItemsLinkingToContent(contentId, UserId, Now);

        Assert.True(affected);
        var linkedItem = menu.Items.Single(i => i.Id == linkedItemId);
        Assert.False(linkedItem.IsActive);
        Assert.True(linkedItem.LinkTarget.IsEmpty);
        var otherItem = menu.Items.Single(i => i.Id == otherItemId);
        Assert.True(otherItem.IsActive);
    }

    [Fact]
    public void DeactivateItemsLinkingToContent_WithNoMatchingItem_ReturnsFalse()
    {
        var menu = Menu.CreateEmpty(MenuLocation.Header, UserId, Now);
        menu.ReplaceItems([Item(Guid.NewGuid())], UserId, Now);

        var affected = menu.DeactivateItemsLinkingToContent(Guid.NewGuid(), UserId, Now);

        Assert.False(affected);
    }
}
