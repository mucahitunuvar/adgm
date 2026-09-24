using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class SiteSettingsTests
{
    [Fact]
    public void CreateDefault_HasAdrSpecifiedDefaults()
    {
        var settings = SiteSettings.CreateDefault();

        Assert.Equal(SiteSettings.SingletonId, settings.Id);
        Assert.True(settings.BotProtectionEnabled);
        Assert.False(settings.GlobalSearchEnabled);
        Assert.False(settings.NewsletterEnabled);
        Assert.False(settings.PublicJobListingsEnabled);
        Assert.False(settings.DonationPageEnabled);
        Assert.False(settings.MaintenanceModeEnabled);
        Assert.Empty(settings.SocialLinks);
        Assert.Empty(settings.BankAccounts);
        Assert.Empty(settings.Translations);
    }

    [Fact]
    public void UpdateTheme_ReplacesThemeAndTouchesAuditFields()
    {
        var settings = SiteSettings.CreateDefault();
        var theme = SiteTheme.Create(null, null, null, "#111111", "#222222", "Inter").Value;
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        settings.UpdateTheme(theme, userId, now);

        Assert.Equal("#111111", settings.Theme.PrimaryColorHex);
        Assert.Equal(userId, settings.UpdatedByUserId);
        Assert.Equal(now, settings.UpdatedAtUtc);
    }

    [Fact]
    public void ReplaceSocialLinks_ClearsPreviousAndAddsNewOnes()
    {
        var settings = SiteSettings.CreateDefault();
        settings.ReplaceSocialLinks([SocialLink.Create("Instagram", "https://instagram.com/x", 1)], Guid.NewGuid(), DateTime.UtcNow);

        settings.ReplaceSocialLinks([SocialLink.Create("LinkedIn", "https://linkedin.com/x", 1)], Guid.NewGuid(), DateTime.UtcNow);

        var link = Assert.Single(settings.SocialLinks);
        Assert.Equal("LinkedIn", link.Platform);
    }

    [Fact]
    public void ReplaceBankAccounts_ClearsPreviousAndAddsNewOnes()
    {
        var settings = SiteSettings.CreateDefault();
        var iban = Iban.Create("TR330006100519786457841326").Value;

        settings.ReplaceBankAccounts([BankAccount.Create(iban, "Ziraat", "Dernek", null, 1, true)], Guid.NewGuid(), DateTime.UtcNow);

        var account = Assert.Single(settings.BankAccounts);
        Assert.Equal("Ziraat", account.BankName);
        Assert.True(account.IsActive);
    }

    [Fact]
    public void SetTranslation_WithNewLanguage_AddsTranslation()
    {
        var settings = SiteSettings.CreateDefault();
        var tr = LanguageCode.Create("tr").Value;

        settings.SetTranslation(tr, "Gençlik Merkezi", "Ana Sayfa", "Açıklama", "Footer", "Bakımdayız", Guid.NewGuid(), DateTime.UtcNow);

        var translation = Assert.Single(settings.Translations);
        Assert.Equal("Gençlik Merkezi", translation.SiteName);
        Assert.Equal("Bakımdayız", translation.MaintenanceMessage);
    }

    [Fact]
    public void SetTranslation_CalledTwiceForSameLanguage_UpdatesInPlaceRatherThanDuplicating()
    {
        var settings = SiteSettings.CreateDefault();
        var tr = LanguageCode.Create("tr").Value;
        settings.SetTranslation(tr, "İlk", null, null, null, null, Guid.NewGuid(), DateTime.UtcNow);

        settings.SetTranslation(tr, "Güncel", null, null, null, null, Guid.NewGuid(), DateTime.UtcNow);

        var translation = Assert.Single(settings.Translations);
        Assert.Equal("Güncel", translation.SiteName);
    }

    [Fact]
    public void SetMaintenanceMode_TogglesTheGlobalSwitchOnly()
    {
        var settings = SiteSettings.CreateDefault();

        settings.SetMaintenanceMode(true, Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(settings.MaintenanceModeEnabled);
    }

    [Fact]
    public void SetTurnstileSiteKey_WithBlankValue_StoresEmptyString()
    {
        var settings = SiteSettings.CreateDefault();

        settings.SetTurnstileSiteKey("   ", Guid.NewGuid(), DateTime.UtcNow);

        Assert.Equal(string.Empty, settings.TurnstileSiteKey);
    }

    [Fact]
    public void SetTurnstileSiteKey_TrimsAndStoresValue()
    {
        var settings = SiteSettings.CreateDefault();

        settings.SetTurnstileSiteKey("  0x4AAA...  ", Guid.NewGuid(), DateTime.UtcNow);

        Assert.Equal("0x4AAA...", settings.TurnstileSiteKey);
    }

    [Fact]
    public void UpdateFeatureFlags_ReplacesAllFlagsAtOnce()
    {
        var settings = SiteSettings.CreateDefault();

        settings.UpdateFeatureFlags(true, true, true, true, false, Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(settings.GlobalSearchEnabled);
        Assert.True(settings.NewsletterEnabled);
        Assert.True(settings.PublicJobListingsEnabled);
        Assert.True(settings.DonationPageEnabled);
        Assert.False(settings.BotProtectionEnabled);
    }
}
