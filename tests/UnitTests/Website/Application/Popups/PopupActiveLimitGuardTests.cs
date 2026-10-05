using GenclikMerkezi.Modules.Website.Application.Popups;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Application.Popups;

public class PopupActiveLimitGuardTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakePopupRepository _popupRepository = new();

    private Popup SeedActivePopup(DateTime? unpublishAtUtc = null)
    {
        var popup = Popup.Create(
            PopupDisplayMode.Modal, null, LinkTarget.CreateEmpty(), PopupTargeting.CreateAllPages(), PopupDeviceTarget.All, null,
            unpublishAtUtc, 0, PopupFrequency.EveryVisit, null, true, 50, Tr, "Başlık", "<p>İçerik</p>", null, UserId, Now).Value;
        _popupRepository.Seed(popup);
        return popup;
    }

    [Fact]
    public async Task CheckAsync_WouldNotBeActive_Succeeds()
    {
        var result = await PopupActiveLimitGuard.CheckAsync(_popupRepository, wouldBeActive: false, null, Now, null, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CheckAsync_WouldBeActiveButAlreadyExpired_Succeeds()
    {
        var result = await PopupActiveLimitGuard.CheckAsync(
            _popupRepository, wouldBeActive: true, Now.AddHours(-1), Now, null, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CheckAsync_UnderLimit_Succeeds()
    {
        for (var i = 0; i < 19; i++)
        {
            SeedActivePopup();
        }

        var result = await PopupActiveLimitGuard.CheckAsync(_popupRepository, wouldBeActive: true, null, Now, null, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CheckAsync_AtLimit_Fails()
    {
        for (var i = 0; i < 20; i++)
        {
            SeedActivePopup();
        }

        var result = await PopupActiveLimitGuard.CheckAsync(_popupRepository, wouldBeActive: true, null, Now, null, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Popup.TooManyActivePopups", result.Error.Code);
    }

    [Fact]
    public async Task CheckAsync_AtLimit_ExcludingOneOfThem_Succeeds()
    {
        Popup? first = null;
        for (var i = 0; i < 20; i++)
        {
            var popup = SeedActivePopup();
            first ??= popup;
        }

        var result = await PopupActiveLimitGuard.CheckAsync(_popupRepository, wouldBeActive: true, null, Now, first!.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
