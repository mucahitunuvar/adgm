using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class PartnerTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LanguageCode En = LanguageCode.Create("en").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid LogoMediaId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidInput_Succeeds()
    {
        var result = Partner.Create(LogoMediaId, "https://example.com", 1, Tr, "Örnek Partner", null, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(LogoMediaId, result.Value.LogoMediaId);
        Assert.Equal("https://example.com", result.Value.WebsiteUrl);
        Assert.True(result.Value.IsActive);
        Assert.Single(result.Value.Translations);
        Assert.Equal("Örnek Partner", result.Value.Translations[0].Name);
    }

    [Fact]
    public void Create_WithNonHttpsWebsiteUrl_Fails()
    {
        var result = Partner.Create(LogoMediaId, "http://example.com", 1, Tr, "Örnek Partner", null, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Partner.WebsiteUrlInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithoutWebsiteUrl_Succeeds()
    {
        var result = Partner.Create(LogoMediaId, null, 1, Tr, "Örnek Partner", null, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.WebsiteUrl);
    }

    [Fact]
    public void Create_WithEmptyDefaultLanguageName_Fails()
    {
        var result = Partner.Create(LogoMediaId, null, 1, Tr, "  ", null, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("PartnerTranslation.NameInvalid", result.Error.Code);
    }

    [Fact]
    public void Update_WithValidInput_ChangesLogoUrlAndSortOrder()
    {
        var partner = Partner.Create(LogoMediaId, null, 1, Tr, "Örnek Partner", null, UserId, Now).Value;
        var newLogoId = Guid.NewGuid();

        var result = partner.Update(newLogoId, "https://new-example.com", 5, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(newLogoId, partner.LogoMediaId);
        Assert.Equal("https://new-example.com", partner.WebsiteUrl);
        Assert.Equal(5, partner.SortOrder);
        Assert.NotNull(partner.UpdatedAtUtc);
    }

    [Fact]
    public void Update_WithNonHttpsWebsiteUrl_Fails()
    {
        var partner = Partner.Create(LogoMediaId, null, 1, Tr, "Örnek Partner", null, UserId, Now).Value;

        var result = partner.Update(LogoMediaId, "ftp://example.com", 1, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Partner.WebsiteUrlInvalid", result.Error.Code);
    }

    [Fact]
    public void SetTranslation_ForNewLanguage_AddsTranslation()
    {
        var partner = Partner.Create(LogoMediaId, null, 1, Tr, "Örnek Partner", null, UserId, Now).Value;

        var result = partner.SetTranslation(En, "Example Partner", "Description", UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, partner.Translations.Count);
    }

    [Fact]
    public void SetTranslation_ForExistingLanguage_UpdatesInPlace()
    {
        var partner = Partner.Create(LogoMediaId, null, 1, Tr, "Örnek Partner", null, UserId, Now).Value;

        var result = partner.SetTranslation(Tr, "Yeni İsim", "Açıklama", UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Single(partner.Translations);
        Assert.Equal("Yeni İsim", partner.Translations[0].Name);
        Assert.Equal("Açıklama", partner.Translations[0].Description);
    }

    [Fact]
    public void RemoveTranslation_ForDefaultLanguage_Fails()
    {
        var partner = Partner.Create(LogoMediaId, null, 1, Tr, "Örnek Partner", null, UserId, Now).Value;

        var result = partner.RemoveTranslation(Tr, Tr, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Partner.CannotDeleteDefaultTranslation", result.Error.Code);
    }

    [Fact]
    public void RemoveTranslation_ForNonDefaultLanguage_Succeeds()
    {
        var partner = Partner.Create(LogoMediaId, null, 1, Tr, "Örnek Partner", null, UserId, Now).Value;
        partner.SetTranslation(En, "Example Partner", null, UserId, Now);

        var result = partner.RemoveTranslation(En, Tr, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Single(partner.Translations);
    }

    [Fact]
    public void RemoveTranslation_WhenMissing_Fails()
    {
        var partner = Partner.Create(LogoMediaId, null, 1, Tr, "Örnek Partner", null, UserId, Now).Value;

        var result = partner.RemoveTranslation(En, Tr, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Partner.TranslationNotFound", result.Error.Code);
    }

    [Fact]
    public void Activate_SetsIsActiveTrue()
    {
        var partner = Partner.Create(LogoMediaId, null, 1, Tr, "Örnek Partner", null, UserId, Now).Value;
        partner.Deactivate(UserId, Now);

        var result = partner.Activate(UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.True(partner.IsActive);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var partner = Partner.Create(LogoMediaId, null, 1, Tr, "Örnek Partner", null, UserId, Now).Value;

        var result = partner.Deactivate(UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.False(partner.IsActive);
    }

    [Fact]
    public void Touch_RegeneratesRowVersion()
    {
        var partner = Partner.Create(LogoMediaId, null, 1, Tr, "Örnek Partner", null, UserId, Now).Value;
        var originalRowVersion = partner.RowVersion;

        partner.Update(LogoMediaId, null, 2, UserId, Now);

        Assert.NotEqual(originalRowVersion, partner.RowVersion);
    }
}
