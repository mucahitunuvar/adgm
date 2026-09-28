using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class ContentTypeTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LanguageCode En = LanguageCode.Create("en").Value;
    private static readonly SeoMetadata EmptySeo = SeoMetadata.CreateEmpty();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 9, 24, 12, 0, 0, DateTimeKind.Utc);

    private static ContentTypeFeatureFlags NoFlags => ContentTypeFeatureFlags.None;

    private static Result<ContentType> CreateNews(ContentTypeFeatureFlags? flags = null, ContentTypeSortMode sortMode = ContentTypeSortMode.PublishDateDesc) =>
        ContentType.Create(
            ContentTypeKey.Create("news").Value, "cards", "article", sortMode, 1,
            flags ?? NoFlags with { HasListingPage = true }, Tr, "Haber", "haberler", EmptySeo, UserId, Now);

    [Fact]
    public void Create_WithValidInput_Succeeds()
    {
        var result = CreateNews();

        Assert.True(result.IsSuccess);
        Assert.Equal("news", result.Value.Key.Value);
        Assert.True(result.Value.IsActive);
        Assert.Single(result.Value.Translations);
        Assert.Equal("haberler", result.Value.Translations[0].RoutePrefix);
    }

    [Fact]
    public void Create_WithRequiresReview_Fails()
    {
        var result = CreateNews(NoFlags with { HasListingPage = true, RequiresReview = true });

        Assert.True(result.IsFailure);
        Assert.Equal("ContentType.RequiresReviewNotSupported", result.Error.Code);
    }

    [Fact]
    public void Create_WithEventDateAscButNotSupportsEvent_Fails()
    {
        var result = CreateNews(NoFlags with { HasListingPage = true }, ContentTypeSortMode.EventDateAsc);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentType.EventDateAscRequiresSupportsEvent", result.Error.Code);
    }

    [Fact]
    public void Create_WithEventDateAscAndSupportsEvent_Succeeds()
    {
        var result = CreateNews(NoFlags with { HasListingPage = true, SupportsEvent = true }, ContentTypeSortMode.EventDateAsc);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithHasListingPageAndEmptyRoutePrefix_Fails()
    {
        var result = ContentType.Create(
            ContentTypeKey.Create("page").Value, "list", "page", ContentTypeSortMode.Manual, 1,
            NoFlags with { HasListingPage = true }, Tr, "Sayfa", null, EmptySeo, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentType.ListingPageRequiresRoutePrefix", result.Error.Code);
    }

    [Fact]
    public void Create_WithoutHasListingPageAndEmptyRoutePrefix_Succeeds()
    {
        var result = ContentType.Create(
            ContentTypeKey.Create("page").Value, "list", "page", ContentTypeSortMode.Manual, 1,
            NoFlags, Tr, "Sayfa", null, EmptySeo, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Value.Translations[0].RoutePrefix);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("cards_grid")]
    [InlineData("cards grid")]
    public void Create_WithInvalidListTemplate_Fails(string? listTemplate)
    {
        var result = ContentType.Create(
            ContentTypeKey.Create("news").Value, listTemplate, "article", ContentTypeSortMode.Manual, 1,
            NoFlags, Tr, "Haber", "haberler", EmptySeo, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentType.ListTemplateInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithUppercaseListTemplate_NormalizesToLowercase()
    {
        var result = ContentType.Create(
            ContentTypeKey.Create("news").Value, "Cards", "article", ContentTypeSortMode.Manual, 1,
            NoFlags, Tr, "Haber", "haberler", EmptySeo, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("cards", result.Value.ListTemplate);
    }

    [Fact]
    public void Update_ChangesTemplatesSortModeSortOrderAndFlags()
    {
        var contentType = CreateNews().Value;
        var originalRowVersion = contentType.RowVersion;

        var result = contentType.Update("list", "story", ContentTypeSortMode.Manual, 2, NoFlags with { HasListingPage = true, SupportsTags = true }, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("list", contentType.ListTemplate);
        Assert.Equal("story", contentType.DetailTemplate);
        Assert.Equal(ContentTypeSortMode.Manual, contentType.SortMode);
        Assert.Equal(2, contentType.SortOrder);
        Assert.True(contentType.SupportsTags);
        Assert.NotEqual(originalRowVersion, contentType.RowVersion);
    }

    [Fact]
    public void Update_TurningOnHasListingPageWhileAnyTranslationHasEmptyRoutePrefix_Fails()
    {
        var contentType = ContentType.Create(
            ContentTypeKey.Create("page").Value, "list", "page", ContentTypeSortMode.Manual, 1,
            NoFlags, Tr, "Sayfa", null, EmptySeo, UserId, Now).Value;

        var result = contentType.Update("list", "page", ContentTypeSortMode.Manual, 1, NoFlags with { HasListingPage = true }, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentType.ListingPageRequiresRoutePrefix", result.Error.Code);
    }

    [Fact]
    public void SetTranslation_AddsNewLanguage()
    {
        var contentType = CreateNews().Value;

        var result = contentType.SetTranslation(En, "News", "news", EmptySeo, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, contentType.Translations.Count);
        Assert.Contains(contentType.Translations, t => t.LanguageCode == En && t.RoutePrefix == "news");
    }

    [Fact]
    public void SetTranslation_UpdatesExistingLanguage()
    {
        var contentType = CreateNews().Value;

        var result = contentType.SetTranslation(Tr, "Haberler", "guncel-haberler", EmptySeo, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Single(contentType.Translations);
        Assert.Equal("Haberler", contentType.Translations[0].Name);
        Assert.Equal("guncel-haberler", contentType.Translations[0].RoutePrefix);
    }

    [Fact]
    public void SetTranslation_WithEmptyRoutePrefixWhileHasListingPage_FailsAndLeavesPreviousValueIntact()
    {
        var contentType = CreateNews().Value;

        var result = contentType.SetTranslation(Tr, "Haberler", string.Empty, EmptySeo, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentType.ListingPageRequiresRoutePrefix", result.Error.Code);
        Assert.Equal("haberler", contentType.Translations[0].RoutePrefix);
        Assert.Equal("Haber", contentType.Translations[0].Name);
    }

    [Fact]
    public void RemoveTranslation_DefaultLanguage_Fails()
    {
        var contentType = CreateNews().Value;
        contentType.SetTranslation(En, "News", "news", EmptySeo, UserId, Now);

        var result = contentType.RemoveTranslation(Tr, Tr, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentType.CannotDeleteDefaultTranslation", result.Error.Code);
    }

    [Fact]
    public void RemoveTranslation_NonDefaultLanguage_Succeeds()
    {
        var contentType = CreateNews().Value;
        contentType.SetTranslation(En, "News", "news", EmptySeo, UserId, Now);

        var result = contentType.RemoveTranslation(En, Tr, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Single(contentType.Translations);
    }

    [Fact]
    public void ActivateDeactivate_ToggleIsActive()
    {
        var contentType = CreateNews().Value;

        contentType.Deactivate(UserId, Now);
        Assert.False(contentType.IsActive);

        contentType.Activate(UserId, Now);
        Assert.True(contentType.IsActive);
    }
}
