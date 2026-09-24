using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettings;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.UpdateSiteSettings;

public class UpdateSiteSettingsCommandHandlerTests
{
    private readonly FakeSiteSettingsRepository _siteSettingsRepository = new();
    private readonly FakeMediaAssetRepository _mediaAssetRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private UpdateSiteSettingsCommandHandler CreateHandler() =>
        new(_siteSettingsRepository, _mediaAssetRepository, new FakeCurrentUserContext(Guid.NewGuid()), _unitOfWork);

    private static UpdateSiteSettingsCommand BuildValidCommand() => new(
        new UpdateSiteSettingsThemeInput(null, null, null, "#111111", "#222222", "Inter"),
        new UpdateSiteSettingsContactInput("Ankara", "+90 555 000 00 00", "info@example.org", null, null),
        [new UpdateSiteSettingsSocialLinkInput("Instagram", "https://instagram.com/x", 1)],
        [new UpdateSiteSettingsBankAccountInput("TR330006100519786457841326", "Ziraat", "Dernek", null, 1, true)],
        [new UpdateSiteSettingsTranslationInput("tr", "Gençlik Merkezi", "Ana Sayfa", "Açıklama", "Footer")],
        new UpdateSiteSettingsFeatureFlagsInput(false, false, false, false, true),
        false,
        null);

    [Fact]
    public async Task Handle_WhenNoSettingsPersistedYet_CreatesAndPersistsTheSingleton()
    {
        var result = await CreateHandler().Handle(BuildValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var settings = await _siteSettingsRepository.GetAsync();
        Assert.NotNull(settings);
        Assert.Equal(SiteSettings.SingletonId, settings!.Id);
        Assert.Equal("#111111", settings.Theme.PrimaryColorHex);
        Assert.Equal("info@example.org", settings.Contact.Email);
        Assert.Single(settings.SocialLinks);
        Assert.Single(settings.BankAccounts);
        Assert.Single(settings.Translations);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenSettingsAlreadyExist_UpdatesInPlaceInsteadOfCreatingASecondRow()
    {
        _siteSettingsRepository.Seed(SiteSettings.CreateDefault());

        var result = await CreateHandler().Handle(BuildValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var settings = await _siteSettingsRepository.GetAsync();
        Assert.Equal("#111111", settings!.Theme.PrimaryColorHex);
    }

    [Fact]
    public async Task Handle_WithInvalidPrimaryColor_ReturnsFailure_DoesNotSave()
    {
        var command = BuildValidCommand() with
        {
            Theme = new UpdateSiteSettingsThemeInput(null, null, null, "not-a-color", null, null),
        };

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("SiteTheme.InvalidPrimaryColor", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithInvalidIban_ReturnsFailure_DoesNotSave()
    {
        var command = BuildValidCommand() with
        {
            BankAccounts = [new UpdateSiteSettingsBankAccountInput("not-an-iban", "Ziraat", "Dernek", null, 1, true)],
        };

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Iban.InvalidFormat", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenLogoMediaAssetDoesNotExist_ReturnsNotFound()
    {
        var command = BuildValidCommand() with
        {
            Theme = new UpdateSiteSettingsThemeInput(Guid.NewGuid(), null, null, null, null, null),
        };

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("SiteSettings.MediaAssetNotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenLogoMediaAssetExists_Succeeds()
    {
        var file = FileAttachment.Create(
            "website-images/2026/09/23/logo.png", "logo.png", "image/png", 1024, DateTime.UtcNow, "MediaAsset", Guid.NewGuid());
        var logo = MediaAsset.Create(
            Guid.NewGuid(), MediaAssetKind.Image, file, [], 200, 200,
            MediaFolder.Create("logos").Value, null, null, false, Guid.NewGuid(), DateTime.UtcNow).Value;
        _mediaAssetRepository.Seed(logo);

        var command = BuildValidCommand() with
        {
            Theme = new UpdateSiteSettingsThemeInput(logo.Id, null, null, null, null, null),
        };

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var settings = await _siteSettingsRepository.GetAsync();
        Assert.Equal(logo.Id, settings!.Theme.LogoLightMediaAssetId);
    }

    [Fact]
    public async Task Handle_WithInvalidTranslationLanguageCode_ReturnsFailure_DoesNotSave()
    {
        var command = BuildValidCommand() with
        {
            Translations = [new UpdateSiteSettingsTranslationInput("123", null, null, null, null)],
        };

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("LanguageCode.InvalidFormat", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
