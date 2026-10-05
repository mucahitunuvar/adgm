using GenclikMerkezi.Modules.Website.Application.Popups;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Application.Popups;

public class PopupTargetingReferenceValidatorTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeSiteLanguageRepository _siteLanguageRepository = new();

    [Fact]
    public async Task ValidateAsync_AllPages_Succeeds()
    {
        var result = await PopupTargetingReferenceValidator.ValidateAsync(
            PopupTargeting.CreateAllPages(), _siteLanguageRepository, _contentItemRepository, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ValidateAsync_Contents_WithMissingContentItem_Fails()
    {
        var targeting = PopupTargeting.CreateForContents([Guid.NewGuid()]).Value;

        var result = await PopupTargetingReferenceValidator.ValidateAsync(
            targeting, _siteLanguageRepository, _contentItemRepository, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Popup.ContentItemNotFound", result.Error.Code);
    }

    [Fact]
    public async Task ValidateAsync_Contents_WithExistingContentItem_Succeeds()
    {
        var contentType = ContentType.Create(
            ContentTypeKey.Create("news").Value, "list", "detail", ContentTypeSortMode.Manual, 1, ContentTypeFeatureFlags.None,
            LanguageCode.Create("tr").Value, "Ad", null, SeoMetadata.CreateEmpty(), UserId, Now).Value;
        var item = ContentItem.Create(
            contentType.Id, null, false, 1, false, null, null, LanguageCode.Create("tr").Value, "Başlık", null, "haberler", [], null, "<p/>",
            SeoMetadata.CreateEmpty(), UserId, Now).Value;
        _contentItemRepository.Add(item);

        var targeting = PopupTargeting.CreateForContents([item.Id]).Value;

        var result = await PopupTargetingReferenceValidator.ValidateAsync(
            targeting, _siteLanguageRepository, _contentItemRepository, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ValidateAsync_Paths_WithLanguagePrefix_Fails()
    {
        _siteLanguageRepository.Add(SiteLanguage.Create(LanguageCode.Create("tr").Value, "Türkçe", 1, UserId, Now));
        _siteLanguageRepository.Add(SiteLanguage.Create(LanguageCode.Create("en").Value, "English", 2, UserId, Now));

        var targeting = PopupTargeting.CreateForPaths(["/en/haberler"]).Value;

        var result = await PopupTargetingReferenceValidator.ValidateAsync(
            targeting, _siteLanguageRepository, _contentItemRepository, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PopupTargeting.PathContainsLanguagePrefix", result.Error.Code);
    }

    [Fact]
    public async Task ValidateAsync_Paths_WithoutLanguagePrefix_Succeeds()
    {
        _siteLanguageRepository.Add(SiteLanguage.Create(LanguageCode.Create("tr").Value, "Türkçe", 1, UserId, Now));
        _siteLanguageRepository.Add(SiteLanguage.Create(LanguageCode.Create("en").Value, "English", 2, UserId, Now));

        var targeting = PopupTargeting.CreateForPaths(["/haberler/*"]).Value;

        var result = await PopupTargetingReferenceValidator.ValidateAsync(
            targeting, _siteLanguageRepository, _contentItemRepository, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
