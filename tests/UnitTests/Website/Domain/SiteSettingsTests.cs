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
        Assert.Null(settings.LogoLightMediaAssetId);
        Assert.Empty(settings.SocialLinks);
        Assert.Empty(settings.BankAccounts);
        Assert.Empty(settings.Translations);
        Assert.NotEmpty(settings.RowVersion);
    }

    [Fact]
    public void UpdateIdentity_SetsMediaReferencesAndTouchesAuditFields()
    {
        var settings = SiteSettings.CreateDefault();
        var logoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        settings.UpdateIdentity(logoId, null, null, null, userId, now);

        Assert.Equal(logoId, settings.LogoLightMediaAssetId);
        Assert.Equal(userId, settings.UpdatedByUserId);
        Assert.Equal(now, settings.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateIdentity_RegeneratesRowVersion()
    {
        var settings = SiteSettings.CreateDefault();
        var before = settings.RowVersion;

        settings.UpdateIdentity(null, null, null, null, Guid.NewGuid(), DateTime.UtcNow);

        Assert.NotEqual(before, settings.RowVersion);
    }

    [Fact]
    public void UpdateTheme_ReplacesThemeAndTouchesAuditFields()
    {
        var settings = SiteSettings.CreateDefault();
        var theme = SiteTheme.Create("#111111", "#222222", "Inter").Value;
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

        settings.ReplaceBankAccounts(
            [BankAccount.Create(iban, "Ziraat", "Dernek", BankAccountCurrency.TRY, null, 1, true)], Guid.NewGuid(), DateTime.UtcNow);

        var account = Assert.Single(settings.BankAccounts);
        Assert.Equal("Ziraat", account.BankName);
        Assert.Equal(BankAccountCurrency.TRY, account.Currency);
        Assert.True(account.IsActive);
    }

    [Fact]
    public void SetIdentityTranslation_WithNewLanguage_AddsTranslation_WithoutTouchingMaintenanceMessage()
    {
        var settings = SiteSettings.CreateDefault();
        var tr = LanguageCode.Create("tr").Value;

        settings.SetIdentityTranslation(tr, "Gençlik Merkezi", "Birlikte güçlüyüz", "Ana Sayfa", "Açıklama", "Footer", Guid.NewGuid(), DateTime.UtcNow);

        var translation = Assert.Single(settings.Translations);
        Assert.Equal("Gençlik Merkezi", translation.SiteName);
        Assert.Equal("Birlikte güçlüyüz", translation.Tagline);
        Assert.Equal(string.Empty, translation.MaintenanceMessage);
    }

    [Fact]
    public void SetIdentityTranslation_CalledTwiceForSameLanguage_UpdatesInPlaceRatherThanDuplicating()
    {
        var settings = SiteSettings.CreateDefault();
        var tr = LanguageCode.Create("tr").Value;
        settings.SetIdentityTranslation(tr, "İlk", null, null, null, null, Guid.NewGuid(), DateTime.UtcNow);

        settings.SetIdentityTranslation(tr, "Güncel", null, null, null, null, Guid.NewGuid(), DateTime.UtcNow);

        var translation = Assert.Single(settings.Translations);
        Assert.Equal("Güncel", translation.SiteName);
    }

    [Fact]
    public void SetIdentityTranslation_DoesNotOverwriteAnAlreadySetMaintenanceMessage()
    {
        var settings = SiteSettings.CreateDefault();
        var tr = LanguageCode.Create("tr").Value;
        settings.SetMaintenanceMessage(tr, "Bakımdayız", Guid.NewGuid(), DateTime.UtcNow);

        settings.SetIdentityTranslation(tr, "Gençlik Merkezi", null, null, null, null, Guid.NewGuid(), DateTime.UtcNow);

        var translation = Assert.Single(settings.Translations);
        Assert.Equal("Bakımdayız", translation.MaintenanceMessage);
    }

    [Fact]
    public void SetMaintenanceMessage_DoesNotOverwriteAlreadySetIdentityFields()
    {
        var settings = SiteSettings.CreateDefault();
        var tr = LanguageCode.Create("tr").Value;
        settings.SetIdentityTranslation(tr, "Gençlik Merkezi", null, null, null, null, Guid.NewGuid(), DateTime.UtcNow);

        settings.SetMaintenanceMessage(tr, "Bakımdayız", Guid.NewGuid(), DateTime.UtcNow);

        var translation = Assert.Single(settings.Translations);
        Assert.Equal("Gençlik Merkezi", translation.SiteName);
        Assert.Equal("Bakımdayız", translation.MaintenanceMessage);
    }

    [Fact]
    public void SetMaintenanceMode_TogglesTheGlobalSwitchOnly()
    {
        var settings = SiteSettings.CreateDefault();

        settings.SetMaintenanceMode(true, Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(settings.MaintenanceModeEnabled);
    }

    [Fact]
    public void UpdateBotProtection_WithBlankSiteKey_StoresEmptyString()
    {
        var settings = SiteSettings.CreateDefault();

        settings.UpdateBotProtection(false, "   ", Guid.NewGuid(), DateTime.UtcNow);

        Assert.False(settings.BotProtectionEnabled);
        Assert.Equal(string.Empty, settings.TurnstileSiteKey);
    }

    [Fact]
    public void UpdateBotProtection_TrimsAndStoresSiteKey()
    {
        var settings = SiteSettings.CreateDefault();

        settings.UpdateBotProtection(true, "  0x4AAA...  ", Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(settings.BotProtectionEnabled);
        Assert.Equal("0x4AAA...", settings.TurnstileSiteKey);
    }

    [Fact]
    public void UpdateFeatureFlags_ReplacesAllFlagsAtOnce()
    {
        var settings = SiteSettings.CreateDefault();

        settings.UpdateFeatureFlags(true, true, true, true, Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(settings.GlobalSearchEnabled);
        Assert.True(settings.NewsletterEnabled);
        Assert.True(settings.PublicJobListingsEnabled);
        Assert.True(settings.DonationPageEnabled);
    }
}
