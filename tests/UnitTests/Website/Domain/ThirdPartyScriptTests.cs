using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class ThirdPartyScriptTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LanguageCode En = LanguageCode.Create("en").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 6, 12, 0, 0, DateTimeKind.Utc);

    private static ThirdPartyScriptProvider Ga4() => ThirdPartyScriptProvider.CreateGoogleAnalytics4("G-ABCD1234").Value;

    [Fact]
    public void Create_WithValidInput_Succeeds()
    {
        var result = ThirdPartyScript.Create(
            Ga4(), ThirdPartyScriptCategory.Analytics, ThirdPartyScriptPlacement.Head, 1, Tr, "Google Analytics", "Ziyaretçi istatistikleri",
            UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsActive);
        Assert.Equal(ThirdPartyScriptCategory.Analytics, result.Value.Category);
        Assert.Equal(ThirdPartyScriptPlacement.Head, result.Value.Placement);
        Assert.Single(result.Value.Translations);
        Assert.Equal("Google Analytics", result.Value.Translations[0].Name);
    }

    [Fact]
    public void Create_WithEmptyDefaultLanguagePurpose_Fails()
    {
        var result = ThirdPartyScript.Create(
            Ga4(), ThirdPartyScriptCategory.Analytics, ThirdPartyScriptPlacement.Head, 1, Tr, "Google Analytics", "  ", UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ThirdPartyScriptTranslation.PurposeInvalid", result.Error.Code);
    }

    [Fact]
    public void Update_ChangesProviderCategoryPlacementAndSortOrder()
    {
        var script = ThirdPartyScript.Create(
            Ga4(), ThirdPartyScriptCategory.Analytics, ThirdPartyScriptPlacement.Head, 1, Tr, "Google Analytics", "Ziyaretçi istatistikleri",
            UserId, Now).Value;
        var gtm = ThirdPartyScriptProvider.CreateGoogleTagManager("GTM-ABCD12").Value;

        script.Update(gtm, ThirdPartyScriptCategory.Marketing, ThirdPartyScriptPlacement.BodyEnd, 5, UserId, Now);

        Assert.Equal(ThirdPartyScriptProviderKind.GoogleTagManager, script.Provider.Kind);
        Assert.Equal(ThirdPartyScriptCategory.Marketing, script.Category);
        Assert.Equal(ThirdPartyScriptPlacement.BodyEnd, script.Placement);
        Assert.Equal(5, script.SortOrder);
        Assert.NotNull(script.UpdatedAtUtc);
    }

    [Fact]
    public void SetTranslation_ForNewLanguage_AddsTranslation()
    {
        var script = ThirdPartyScript.Create(
            Ga4(), ThirdPartyScriptCategory.Analytics, ThirdPartyScriptPlacement.Head, 1, Tr, "Google Analytics", "Ziyaretçi istatistikleri",
            UserId, Now).Value;

        var result = script.SetTranslation(En, "Google Analytics", "Visitor statistics", UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, script.Translations.Count);
    }

    [Fact]
    public void RemoveTranslation_ForDefaultLanguage_Fails()
    {
        var script = ThirdPartyScript.Create(
            Ga4(), ThirdPartyScriptCategory.Analytics, ThirdPartyScriptPlacement.Head, 1, Tr, "Google Analytics", "Ziyaretçi istatistikleri",
            UserId, Now).Value;

        var result = script.RemoveTranslation(Tr, Tr, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ThirdPartyScript.CannotDeleteDefaultTranslation", result.Error.Code);
    }

    [Fact]
    public void RemoveTranslation_ForNonDefaultLanguage_Succeeds()
    {
        var script = ThirdPartyScript.Create(
            Ga4(), ThirdPartyScriptCategory.Analytics, ThirdPartyScriptPlacement.Head, 1, Tr, "Google Analytics", "Ziyaretçi istatistikleri",
            UserId, Now).Value;
        script.SetTranslation(En, "Google Analytics", "Visitor statistics", UserId, Now);

        var result = script.RemoveTranslation(En, Tr, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Single(script.Translations);
    }

    [Fact]
    public void Activate_SetsIsActiveTrue()
    {
        var script = ThirdPartyScript.Create(
            Ga4(), ThirdPartyScriptCategory.Analytics, ThirdPartyScriptPlacement.Head, 1, Tr, "Google Analytics", "Ziyaretçi istatistikleri",
            UserId, Now).Value;
        script.Deactivate(UserId, Now);

        script.Activate(UserId, Now);

        Assert.True(script.IsActive);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var script = ThirdPartyScript.Create(
            Ga4(), ThirdPartyScriptCategory.Analytics, ThirdPartyScriptPlacement.Head, 1, Tr, "Google Analytics", "Ziyaretçi istatistikleri",
            UserId, Now).Value;

        script.Deactivate(UserId, Now);

        Assert.False(script.IsActive);
    }
}
