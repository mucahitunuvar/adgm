using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenclikMerkezi.UnitTests.Website.Application.ContentPaths;

public class ContentItemPermanentDeletionServiceTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly SeoMetadata EmptySeo = SeoMetadata.CreateEmpty();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ContentTypeId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeRedirectRepository _redirectRepository = new();
    private readonly FakeMenuRepository _menuRepository = new();
    private readonly FakePageLayoutRepository _pageLayoutRepository = new();
    private readonly FakeEventScheduleRepository _eventScheduleRepository = new();
    private readonly FakeEventRegistrationUsageChecker _eventRegistrationUsageChecker = new();
    private readonly FakeContentItemRevisionRepository _contentItemRevisionRepository = new();

    private ContentItemPermanentDeletionService CreateService() =>
        new(
            _contentItemRepository, _redirectRepository, _menuRepository, _pageLayoutRepository, _eventScheduleRepository,
            _eventRegistrationUsageChecker, _contentItemRevisionRepository, NullLogger<ContentItemPermanentDeletionService>.Instance);

    private static ContentItem CreateItem(Guid? parentId = null, string slug = "haber") =>
        ContentItem.Create(
            ContentTypeId, parentId, false, 1, false, null, null, Tr, "Başlık", slug, "haberler", [], null, "<p/>", EmptySeo, UserId, Now).Value;

    [Fact]
    public async Task DeleteAsync_WithChildren_Fails()
    {
        var parent = CreateItem(slug: "ust");
        var child = CreateItem(parent.Id, "alt");
        _contentItemRepository.Seed(parent);
        _contentItemRepository.Seed(child);

        var result = await CreateService().DeleteAsync(parent, UserId, Now, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.HasChildren", result.Error.Code);
    }

    [Fact]
    public async Task DeleteAsync_WithoutChildren_RemovesTheItem()
    {
        var item = CreateItem();
        _contentItemRepository.Seed(item);

        var result = await CreateService().DeleteAsync(item, UserId, Now, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(await _contentItemRepository.GetByIdAsync(item.Id));
    }

    [Fact]
    public async Task DeleteAsync_RemovesRedirectsTargetingTheItem()
    {
        var item = CreateItem();
        _contentItemRepository.Seed(item);
        var redirect = Redirect.CreateAutomatic(Tr, "/eski-yol", item.Id, UserId, Now).Value;
        _redirectRepository.Seed(redirect);

        await CreateService().DeleteAsync(item, UserId, Now, CancellationToken.None);

        Assert.Empty(_redirectRepository.Redirects);
    }

    [Fact]
    public async Task DeleteAsync_RemovesReferenceFromOtherItemsRelatedContentItemIds()
    {
        var item = CreateItem(slug: "silinecek");
        var relatingItem = CreateItem(slug: "iliskili");
        relatingItem.SetRelatedContent([item.Id], UserId, Now);
        _contentItemRepository.Seed(item);
        _contentItemRepository.Seed(relatingItem);

        await CreateService().DeleteAsync(item, UserId, Now, CancellationToken.None);

        Assert.DoesNotContain(item.Id, relatingItem.RelatedContentItemIds);
    }

    [Fact]
    public async Task DeleteAsync_ClearsAndDeactivatesMenuItemsLinkingToTheItem()
    {
        var item = CreateItem(slug: "silinecek");
        _contentItemRepository.Seed(item);

        var menu = Menu.CreateEmpty(MenuLocation.Header, UserId, Now);
        var translation = MenuItemTranslation.Create(Tr, "Etiket").Value;
        var linkTarget = LinkTarget.ForContent(item.Id).Value;
        var menuItem = MenuItem.Create(Guid.NewGuid(), null, 1, true, linkTarget, false, null, [translation]).Value;
        menu.ReplaceItems([menuItem], UserId, Now);
        _menuRepository.Seed(menu);

        await CreateService().DeleteAsync(item, UserId, Now, CancellationToken.None);

        var updatedItem = Assert.Single(menu.Items);
        Assert.False(updatedItem.IsActive);
        Assert.True(updatedItem.LinkTarget.IsEmpty);
    }

    [Fact]
    public async Task DeleteAsync_RemovesThePageLayoutForTheItem()
    {
        var item = CreateItem(slug: "silinecek");
        _contentItemRepository.Seed(item);

        var layout = PageLayout.CreateForContent(item.Id, UserId, Now).Value;
        _pageLayoutRepository.Seed(layout);

        await CreateService().DeleteAsync(item, UserId, Now, CancellationToken.None);

        Assert.Null(await _pageLayoutRepository.GetByContentItemIdAsync(item.Id));
    }

    [Fact]
    public async Task DeleteAsync_WithRegistrations_Fails()
    {
        var item = CreateItem(slug: "silinecek");
        _contentItemRepository.Seed(item);
        _eventRegistrationUsageChecker.HasRegistrationsResult = true;

        var result = await CreateService().DeleteAsync(item, UserId, Now, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Event.HasRegistrations", result.Error.Code);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTheEventScheduleForTheItem()
    {
        var item = CreateItem(slug: "silinecek");
        _contentItemRepository.Seed(item);
        var schedule = EventSchedule.Create(
            item.Id, Now.AddDays(10), Now.AddDays(10).AddHours(2), EventFormat.InPerson, null, null, false, null, null, null, null,
            true, false, Tr, "Salon", "Adres", null, null, null, null, UserId, Now).Value;
        _eventScheduleRepository.Seed(schedule);

        await CreateService().DeleteAsync(item, UserId, Now, CancellationToken.None);

        Assert.Null(await _eventScheduleRepository.GetByContentItemIdAsync(item.Id));
    }
}
