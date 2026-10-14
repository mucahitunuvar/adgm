using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Modules.Website.Application.Popups;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.UpdatePopup;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Website.Features.UpdatePopup;

public class UpdatePopupCommandHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakePopupRepository _popupRepository = new();
    private readonly FakeSiteLanguageRepository _siteLanguageRepository = new();
    private readonly FakeMediaAssetRepository _mediaAssetRepository = new();
    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly ICacheService _cacheService =
        new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings()));

    private UpdatePopupCommandHandler CreateHandler() =>
        new(
            _popupRepository, _siteLanguageRepository, _mediaAssetRepository, _contentItemRepository,
            new FakeCurrentUserContext(UserId), TimeProvider.System, _cacheService, new FakeUnitOfWork());

    private Popup SeedPopup()
    {
        var popup = Popup.Create(
            PopupDisplayMode.Banner, null, LinkTarget.CreateEmpty(), PopupTargeting.CreateAllPages(), PopupDeviceTarget.All, null,
            null, 0, PopupFrequency.EveryVisit, null, true, 1, LanguageCode.Create("tr").Value, "Başlık", "Gövde", null, UserId,
            Now).Value;
        _popupRepository.Seed(popup);
        return popup;
    }

    private static UpdatePopupCommand CreateCommand(
        Guid id, byte[] rowVersion, string displayMode = "Banner", string deviceTarget = "All", string frequency = "EveryVisit") =>
        new(
            id, rowVersion, displayMode, null, null, new PopupTargetingInput("AllPages", null, null), deviceTarget, null, null, 0,
            frequency, null, true, 1);

    [Theory]
    [InlineData("999")]
    [InlineData("Modal,Banner")] // 0|1 = 1 = Banner - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedDisplayMode_Fails(string displayMode)
    {
        var popup = SeedPopup();

        var result = await CreateHandler().Handle(CreateCommand(popup.Id, popup.RowVersion, displayMode), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Popup.DisplayModeInvalid", result.Error.Code);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("All,Desktop")] // 0|1 = 1 = Desktop - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedDeviceTarget_Fails(string deviceTarget)
    {
        var popup = SeedPopup();

        var result = await CreateHandler().Handle(
            CreateCommand(popup.Id, popup.RowVersion, deviceTarget: deviceTarget), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Popup.DeviceTargetInvalid", result.Error.Code);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("EveryVisit,OncePerSession")] // 0|1 = 1 = OncePerSession - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedFrequency_Fails(string frequency)
    {
        var popup = SeedPopup();

        var result = await CreateHandler().Handle(
            CreateCommand(popup.Id, popup.RowVersion, frequency: frequency), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Popup.FrequencyInvalid", result.Error.Code);
    }
}
