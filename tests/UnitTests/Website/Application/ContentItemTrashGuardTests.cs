using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Application;

public class ContentItemTrashGuardTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly SeoMetadata EmptySeo = SeoMetadata.CreateEmpty();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc);

    private static ContentItem CreateItem() =>
        ContentItem.Create(
            Guid.NewGuid(), null, false, 1, false, null, null, Tr, "Başlık", null, "haberler", [], null, "<p/>", EmptySeo, UserId, Now).Value;

    [Fact]
    public void EnsureEditable_ForNonTrashedItem_Succeeds()
    {
        var item = CreateItem();

        var result = ContentItemTrashGuard.EnsureEditable(item);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void EnsureEditable_ForTrashedItem_Fails()
    {
        var item = CreateItem();
        item.MoveToTrash(UserId, Now);

        var result = ContentItemTrashGuard.EnsureEditable(item);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.InTrash", result.Error.Code);
    }
}
