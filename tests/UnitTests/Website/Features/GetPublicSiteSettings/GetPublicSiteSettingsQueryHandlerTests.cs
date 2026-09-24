using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.GetPublicSiteSettings;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.GetPublicSiteSettings;

public class GetPublicSiteSettingsQueryHandlerTests
{
    private readonly FakeSiteSettingsRepository _siteSettingsRepository = new();
    private readonly FakeMediaAssetRepository _mediaAssetRepository = new();
    private readonly FakeMediaFileStorageService _fileStorageService = new();

    private GetPublicSiteSettingsQueryHandler CreateHandler() =>
        new(_siteSettingsRepository, _mediaAssetRepository, _fileStorageService);

    [Fact]
    public async Task Handle_WhenNoSettingsPersistedYet_ReturnsDefaultsWithoutThrowing()
    {
        var result = await CreateHandler().Handle(new GetPublicSiteSettingsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.DonationPageEnabled);
        Assert.Empty(result.Value.BankAccounts);
    }

    [Fact]
    public async Task Handle_OnlyReturnsActiveBankAccounts()
    {
        var settings = SiteSettings.CreateDefault();
        var iban = Iban.Create("TR330006100519786457841326").Value;
        settings.ReplaceBankAccounts(
            [
                BankAccount.Create(iban, "Ziraat", "Dernek", null, 1, isActive: true),
                BankAccount.Create(iban, "Halkbank", "Dernek", null, 2, isActive: false),
            ],
            Guid.NewGuid(), DateTime.UtcNow);
        _siteSettingsRepository.Seed(settings);

        var result = await CreateHandler().Handle(new GetPublicSiteSettingsQuery(), CancellationToken.None);

        var account = Assert.Single(result.Value.BankAccounts);
        Assert.Equal("Ziraat", account.BankName);
    }
}
