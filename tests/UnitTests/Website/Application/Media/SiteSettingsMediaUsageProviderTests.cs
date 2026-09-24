using GenclikMerkezi.Modules.Website.Application.Media;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Application.Media;

public class SiteSettingsMediaUsageProviderTests
{
    private readonly FakeSiteSettingsRepository _siteSettingsRepository = new();

    private SiteSettingsMediaUsageProvider CreateProvider() => new(_siteSettingsRepository);

    [Fact]
    public async Task GetUsagesAsync_WhenNoSettingsPersistedYet_ReturnsEmpty()
    {
        var usages = await CreateProvider().GetUsagesAsync(Guid.NewGuid());

        Assert.Empty(usages);
    }

    [Fact]
    public async Task GetUsagesAsync_WhenMediaAssetIsNotReferenced_ReturnsEmpty()
    {
        var settings = SiteSettings.CreateDefault();
        settings.UpdateIdentity(Guid.NewGuid(), null, null, null, Guid.NewGuid(), DateTime.UtcNow);
        _siteSettingsRepository.Seed(settings);

        var usages = await CreateProvider().GetUsagesAsync(Guid.NewGuid());

        Assert.Empty(usages);
    }

    [Fact]
    public async Task GetUsagesAsync_WhenMediaAssetIsTheLightLogo_ReturnsOneUsage()
    {
        var logoId = Guid.NewGuid();
        var settings = SiteSettings.CreateDefault();
        settings.UpdateIdentity(logoId, null, null, null, Guid.NewGuid(), DateTime.UtcNow);
        _siteSettingsRepository.Seed(settings);

        var usages = await CreateProvider().GetUsagesAsync(logoId);

        var usage = Assert.Single(usages);
        Assert.Equal("site-settings", usage.SourceKey);
    }

    [Fact]
    public async Task GetUsagesAsync_WhenMediaAssetIsBothDarkLogoAndFavicon_ReturnsTwoUsages()
    {
        var sharedId = Guid.NewGuid();
        var settings = SiteSettings.CreateDefault();
        settings.UpdateIdentity(null, sharedId, sharedId, null, Guid.NewGuid(), DateTime.UtcNow);
        _siteSettingsRepository.Seed(settings);

        var usages = await CreateProvider().GetUsagesAsync(sharedId);

        Assert.Equal(2, usages.Count);
    }

    [Fact]
    public async Task GetUsagesAsync_WhenMediaAssetIsTheDefaultOgImage_ReturnsOneUsage()
    {
        var ogImageId = Guid.NewGuid();
        var settings = SiteSettings.CreateDefault();
        settings.UpdateIdentity(null, null, null, ogImageId, Guid.NewGuid(), DateTime.UtcNow);
        _siteSettingsRepository.Seed(settings);

        var usages = await CreateProvider().GetUsagesAsync(ogImageId);

        Assert.Single(usages);
    }
}
