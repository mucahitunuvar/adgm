using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.GetSiteSettings;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.GetSiteSettings;

public class GetSiteSettingsQueryHandlerTests
{
    private readonly FakeSiteSettingsRepository _siteSettingsRepository = new();
    private readonly FakeMediaAssetRepository _mediaAssetRepository = new();
    private readonly FakeMediaFileStorageService _fileStorageService = new();

    private GetSiteSettingsQueryHandler CreateHandler() =>
        new(_siteSettingsRepository, _mediaAssetRepository, _fileStorageService);

    [Fact]
    public async Task Handle_WhenNoSettingsPersistedYet_ReturnsAdrSpecifiedDefaults()
    {
        var result = await CreateHandler().Handle(new GetSiteSettingsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.BotProtectionEnabled);
        Assert.False(result.Value.GlobalSearchEnabled);
        Assert.Empty(result.Value.BankAccounts);
    }

    [Fact]
    public async Task Handle_WithLogoReference_ResolvesItsPublicUrl()
    {
        var file = FileAttachment.Create(
            "website-images/2026/09/23/logo.png", "logo.png", "image/png", 1024, DateTime.UtcNow, "MediaAsset", Guid.NewGuid());
        var logo = MediaAsset.Create(
            Guid.NewGuid(), MediaAssetKind.Image, file, [], 200, 200,
            MediaFolder.Create("logos").Value, null, null, false, Guid.NewGuid(), DateTime.UtcNow).Value;
        _mediaAssetRepository.Seed(logo);

        var settings = SiteSettings.CreateDefault();
        settings.UpdateTheme(SiteTheme.Create(logo.Id, null, null, null, null, null).Value, Guid.NewGuid(), DateTime.UtcNow);
        _siteSettingsRepository.Seed(settings);

        var result = await CreateHandler().Handle(new GetSiteSettingsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(logo.Id, result.Value.Theme.LogoLightMediaAssetId);
        Assert.NotNull(result.Value.Theme.LogoLightUrl);
    }

    [Fact]
    public async Task Handle_IncludesInactiveBankAccounts_UnlikeThePublicProjection()
    {
        var settings = SiteSettings.CreateDefault();
        var iban = Iban.Create("TR330006100519786457841326").Value;
        settings.ReplaceBankAccounts([BankAccount.Create(iban, "Ziraat", "Dernek", null, 1, isActive: false)], Guid.NewGuid(), DateTime.UtcNow);
        _siteSettingsRepository.Seed(settings);

        var result = await CreateHandler().Handle(new GetSiteSettingsQuery(), CancellationToken.None);

        var account = Assert.Single(result.Value.BankAccounts);
        Assert.False(account.IsActive);
    }
}
