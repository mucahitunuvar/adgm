using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.ReplaceMenuItems;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Website.Features.ReplaceMenuItems;

public class ReplaceMenuItemsCommandHandlerTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeMenuRepository _menuRepository = new();
    private readonly FakeSiteLanguageRepository _siteLanguageRepository = new();
    private readonly ICacheService _cacheService =
        new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings()));

    private ReplaceMenuItemsCommandHandler CreateHandler()
    {
        var defaultLanguage = SiteLanguage.Create(Tr, "Türkçe", 1, UserId, Now);
        defaultLanguage.MarkAsDefault(UserId, Now);
        _siteLanguageRepository.Seed(defaultLanguage);

        return new ReplaceMenuItemsCommandHandler(
            _menuRepository, _siteLanguageRepository, new FakeCurrentUserContext(UserId), TimeProvider.System, _cacheService,
            new FakeUnitOfWork());
    }

    [Theory]
    [InlineData("999")]
    [InlineData("Header,Utility")] // 0|1 = 1 = Utility - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedLocation_Fails(string location)
    {
        var result = await CreateHandler().Handle(new ReplaceMenuItemsCommand(location, [], []), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Menu.LocationInvalid", result.Error.Code);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("None,Content")] // 0|1 = 1 = Content - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedLinkKind_Fails(string linkKind)
    {
        var menu = Menu.CreateEmpty(MenuLocation.Header, UserId, Now);
        _menuRepository.Seed(menu);

        var items = new[]
        {
            new MenuItemTreeInput(
                "item-1", null, 1, true, new MenuItemLinkInput(linkKind, null, null, null, null), false, null,
                [new MenuItemTranslationInput("tr", "Anasayfa")]),
        };
        var command = new ReplaceMenuItemsCommand("Header", menu.RowVersion, items);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("MenuItem.LinkKindInvalid", result.Error.Code);
    }
}
