using GenclikMerkezi.Modules.Website.Application.LinkTargets;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Application.LinkTargets;

public class LinkTargetResolverTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LanguageCode En = LanguageCode.Create("en").Value;
    private static readonly SeoMetadata EmptySeo = SeoMetadata.CreateEmpty();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeContentTypeRepository _contentTypeRepository = new();

    private LinkTargetResolver CreateResolver() => new(_contentItemRepository, _contentTypeRepository);

    private ContentType CreateContentType(bool isActive = true, bool hasDetailPage = true, bool hasListingPage = false)
    {
        var contentType = ContentType.Create(
            ContentTypeKey.Create("news").Value, "cards", "article", ContentTypeSortMode.PublishDateDesc, 1,
            ContentTypeFeatureFlags.None with { HasDetailPage = hasDetailPage, HasListingPage = hasListingPage },
            Tr, "Haber", hasListingPage ? "haberler" : null, EmptySeo, UserId, Now).Value;

        if (!isActive)
        {
            contentType.Deactivate(UserId, Now);
        }

        _contentTypeRepository.Add(contentType);
        return contentType;
    }

    private ContentItem CreatePublishedItem(ContentType contentType, Guid? parentId = null, LanguageCode? languageCode = null)
    {
        var item = ContentItem.Create(
            contentType.Id, parentId, false, 1, false, null, null, languageCode ?? Tr, "Başlık", null, "haberler", [], null, "<p/>", EmptySeo,
            UserId, Now).Value;
        item.Publish(null, null, true, true, languageCode ?? Tr, UserId, Now);
        _contentItemRepository.Add(item);
        return item;
    }

    [Fact]
    public async Task ResolveAsync_Content_WhenVisibleAndTranslated_ReturnsHref()
    {
        var contentType = CreateContentType();
        var item = CreatePublishedItem(contentType);
        var target = LinkTarget.ForContent(item.Id).Value;

        var resolution = await CreateResolver().ResolveAsync(target, Tr, Tr, Now);

        Assert.True(resolution.IsResolved);
        Assert.NotNull(resolution.Href);
    }

    [Fact]
    public async Task ResolveAsync_Content_WhenNotFound_ReturnsUnresolved()
    {
        var target = LinkTarget.ForContent(Guid.NewGuid()).Value;

        var resolution = await CreateResolver().ResolveAsync(target, Tr, Tr, Now);

        Assert.False(resolution.IsResolved);
        Assert.Equal(LinkTargetUnresolvedReason.ContentNotFound, resolution.UnresolvedReason);
    }

    [Fact]
    public async Task ResolveAsync_Content_WhenContentTypeInactive_ReturnsUnresolved()
    {
        var contentType = CreateContentType(isActive: false);
        var item = CreatePublishedItem(contentType);
        var target = LinkTarget.ForContent(item.Id).Value;

        var resolution = await CreateResolver().ResolveAsync(target, Tr, Tr, Now);

        Assert.False(resolution.IsResolved);
        Assert.Equal(LinkTargetUnresolvedReason.ContentTypeInactive, resolution.UnresolvedReason);
    }

    [Fact]
    public async Task ResolveAsync_Content_WhenContentTypeHasNoDetailPage_ReturnsUnresolved()
    {
        var contentType = CreateContentType(hasDetailPage: false);
        var item = CreatePublishedItem(contentType);
        var target = LinkTarget.ForContent(item.Id).Value;

        var resolution = await CreateResolver().ResolveAsync(target, Tr, Tr, Now);

        Assert.False(resolution.IsResolved);
        Assert.Equal(LinkTargetUnresolvedReason.ContentTypeHasNoDetailPage, resolution.UnresolvedReason);
    }

    [Fact]
    public async Task ResolveAsync_Content_WhenNoTranslationInRequestedLanguage_ReturnsUnresolved()
    {
        var contentType = CreateContentType();
        var item = CreatePublishedItem(contentType);
        var target = LinkTarget.ForContent(item.Id).Value;

        var resolution = await CreateResolver().ResolveAsync(target, En, Tr, Now);

        Assert.False(resolution.IsResolved);
        Assert.Equal(LinkTargetUnresolvedReason.ContentTranslationMissing, resolution.UnresolvedReason);
    }

    [Fact]
    public async Task ResolveAsync_Content_WhenAncestorNotVisible_ReturnsContentNotVisible()
    {
        var contentType = CreateContentType();
        var parent = ContentItem.Create(
            contentType.Id, null, false, 1, false, null, null, Tr, "Üst", null, "haberler", [], null, "<p/>", EmptySeo, UserId, Now).Value;
        _contentItemRepository.Add(parent);
        var child = CreatePublishedItem(contentType, parent.Id);
        var target = LinkTarget.ForContent(child.Id).Value;

        var resolution = await CreateResolver().ResolveAsync(target, Tr, Tr, Now);

        Assert.False(resolution.IsResolved);
        Assert.Equal(LinkTargetUnresolvedReason.ContentNotVisible, resolution.UnresolvedReason);
    }

    [Fact]
    public async Task ResolveAsync_ContentTypeListing_WhenActiveWithListingPage_ReturnsHref()
    {
        var contentType = CreateContentType(hasListingPage: true);
        var target = LinkTarget.ForContentTypeListing(contentType.Id).Value;

        var resolution = await CreateResolver().ResolveAsync(target, Tr, Tr, Now);

        Assert.True(resolution.IsResolved);
        Assert.Equal("/haberler", resolution.Href);
    }

    [Fact]
    public async Task ResolveAsync_ContentTypeListing_WhenNoListingPage_ReturnsUnresolved()
    {
        var contentType = CreateContentType(hasListingPage: false);
        var target = LinkTarget.ForContentTypeListing(contentType.Id).Value;

        var resolution = await CreateResolver().ResolveAsync(target, Tr, Tr, Now);

        Assert.False(resolution.IsResolved);
        Assert.Equal(LinkTargetUnresolvedReason.ContentTypeHasNoListingPage, resolution.UnresolvedReason);
    }

    [Fact]
    public async Task ResolveAsync_ContentTypeListing_WhenNotFound_ReturnsUnresolved()
    {
        var target = LinkTarget.ForContentTypeListing(Guid.NewGuid()).Value;

        var resolution = await CreateResolver().ResolveAsync(target, Tr, Tr, Now);

        Assert.False(resolution.IsResolved);
        Assert.Equal(LinkTargetUnresolvedReason.ContentTypeNotFound, resolution.UnresolvedReason);
    }

    [Fact]
    public async Task ResolveAsync_InternalPath_AlwaysResolves()
    {
        var target = LinkTarget.ForInternalPath("/portal/giris").Value;

        var resolution = await CreateResolver().ResolveAsync(target, En, Tr, Now);

        Assert.True(resolution.IsResolved);
        Assert.Equal("/en/portal/giris", resolution.Href);
    }

    [Fact]
    public async Task ResolveAsync_ExternalUrl_AlwaysResolvesToItself()
    {
        var target = LinkTarget.ForExternalUrl("https://example.com/campaign").Value;

        var resolution = await CreateResolver().ResolveAsync(target, Tr, Tr, Now);

        Assert.True(resolution.IsResolved);
        Assert.Equal("https://example.com/campaign", resolution.Href);
    }

    [Fact]
    public async Task ResolveManyAsync_WithMultipleContentTargetsAcrossDifferentAncestorChains_ResolvesEachIndependently()
    {
        var contentType = CreateContentType();
        var firstItem = CreatePublishedItem(contentType);
        var secondItem = CreatePublishedItem(contentType);
        var targets = new[] { LinkTarget.ForContent(firstItem.Id).Value, LinkTarget.ForContent(secondItem.Id).Value };

        var resolutions = await CreateResolver().ResolveManyAsync(targets, Tr, Tr, Now);

        Assert.True(resolutions[targets[0]].IsResolved);
        Assert.True(resolutions[targets[1]].IsResolved);
    }
}
