using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Modules.Website.Application.Popups;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.CreatePopup;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Website.Features.CreatePopup;

public class CreatePopupCommandHandlerTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakePopupRepository _popupRepository = new();
    private readonly FakeSiteLanguageRepository _siteLanguageRepository = new();
    private readonly FakeMediaAssetRepository _mediaAssetRepository = new();
    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeHtmlContentSanitizer _htmlContentSanitizer = new();
    private readonly ICacheService _cacheService =
        new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings()));

    private CreatePopupCommandHandler CreateHandler()
    {
        var defaultLanguage = SiteLanguage.Create(Tr, "Türkçe", 1, UserId, Now);
        defaultLanguage.MarkAsDefault(UserId, Now);
        _siteLanguageRepository.Seed(defaultLanguage);

        return new CreatePopupCommandHandler(
            _popupRepository, _siteLanguageRepository, _mediaAssetRepository, _contentItemRepository, _htmlContentSanitizer,
            new FakeCurrentUserContext(UserId), TimeProvider.System, _cacheService, new FakeUnitOfWork());
    }

    private static CreatePopupCommand CreateCommand(
        string displayMode = "Banner", string deviceTarget = "All", string frequency = "EveryVisit") =>
        new(
            displayMode, null, null, new PopupTargetingInput("AllPages", null, null), deviceTarget, null, null, 0, frequency, null,
            true, 1, "Başlık", "Gövde", "Buton");

    [Theory]
    [InlineData("999")]
    [InlineData("Modal,Banner")] // 0|1 = 1 = Banner - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedDisplayMode_Fails(string displayMode)
    {
        var result = await CreateHandler().Handle(CreateCommand(displayMode), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Popup.DisplayModeInvalid", result.Error.Code);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("All,Desktop")] // 0|1 = 1 = Desktop - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedDeviceTarget_Fails(string deviceTarget)
    {
        var result = await CreateHandler().Handle(CreateCommand(deviceTarget: deviceTarget), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Popup.DeviceTargetInvalid", result.Error.Code);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("EveryVisit,OncePerSession")] // 0|1 = 1 = OncePerSession - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedFrequency_Fails(string frequency)
    {
        var result = await CreateHandler().Handle(CreateCommand(frequency: frequency), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Popup.FrequencyInvalid", result.Error.Code);
    }
}
