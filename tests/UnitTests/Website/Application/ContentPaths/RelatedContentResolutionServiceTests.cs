using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Application.ContentPaths;

public class RelatedContentResolutionServiceTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LanguageCode En = LanguageCode.Create("en").Value;
    private static readonly SeoMetadata EmptySeo = SeoMetadata.CreateEmpty();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ContentTypeId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 9, 29, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeContentItemRepository _contentItemRepository = new();

    private RelatedContentResolutionService CreateService() => new(_contentItemRepository);

    private static ContentItem CreatePublishedItem(
        string title, DateTime publishedAtUtc, Guid? contentTypeId = null, LanguageCode? languageCode = null, IReadOnlyList<Guid>? categoryIds = null)
    {
        var item = ContentItem.Create(
            contentTypeId ?? ContentTypeId, null, false, 1, false, null, null,
            languageCode ?? Tr, title, null, "haberler", [], null, "<p>gövde</p>", EmptySeo, UserId, publishedAtUtc).Value;
        item.Publish(null, null, true, true, languageCode ?? Tr, UserId, publishedAtUtc);

        if (categoryIds is { Count: > 0 })
        {
            item.SetCategories(categoryIds, UserId, publishedAtUtc);
        }

        return item;
    }

    [Fact]
    public async Task ResolveAsync_WithVisibleManualRelatedItems_ReturnsThemInManualOrder()
    {
        var first = CreatePublishedItem("Birinci", Now.AddDays(-1));
        var second = CreatePublishedItem("İkinci", Now.AddDays(-2));
        _contentItemRepository.Seed(first);
        _contentItemRepository.Seed(second);

        var result = await CreateService().ResolveAsync(
            Guid.NewGuid(), ContentTypeId, [second.Id, first.Id], [], Tr, Now);

        Assert.Equal([second.Id, first.Id], result.Select(r => r.Id));
    }

    [Fact]
    public async Task ResolveAsync_WithManualItemsAllInvisible_FallsBackToCategoryMatch()
    {
        var categoryId = Guid.NewGuid();
        var draftManual = ContentItem.Create(
            ContentTypeId, null, false, 1, false, null, null, Tr, "Taslak", null, "haberler", [], null, "<p/>", EmptySeo, UserId, Now).Value;
        var categoryMatch = CreatePublishedItem("Kategori Eşleşmesi", Now.AddDays(-1), categoryIds: [categoryId]);
        _contentItemRepository.Seed(draftManual);
        _contentItemRepository.Seed(categoryMatch);

        var result = await CreateService().ResolveAsync(
            Guid.NewGuid(), ContentTypeId, [draftManual.Id], [categoryId], Tr, Now);

        Assert.Equal([categoryMatch.Id], result.Select(r => r.Id));
    }

    [Fact]
    public async Task ResolveAsync_WithNoManualAndSharedCategory_ReturnsSameTypeSameCategoryNewest()
    {
        var categoryId = Guid.NewGuid();
        var older = CreatePublishedItem("Eski", Now.AddDays(-10), categoryIds: [categoryId]);
        var newer = CreatePublishedItem("Yeni", Now.AddDays(-1), categoryIds: [categoryId]);
        var otherCategory = CreatePublishedItem("Farklı Kategori", Now.AddDays(-1), categoryIds: [Guid.NewGuid()]);
        _contentItemRepository.Seed(older);
        _contentItemRepository.Seed(newer);
        _contentItemRepository.Seed(otherCategory);

        var result = await CreateService().ResolveAsync(Guid.NewGuid(), ContentTypeId, [], [categoryId], Tr, Now);

        Assert.Equal([newer.Id, older.Id], result.Select(r => r.Id));
    }

    [Fact]
    public async Task ResolveAsync_WithNoCategoryMatch_FallsBackToSameTypeAny()
    {
        var sameType = CreatePublishedItem("Aynı Tür", Now.AddDays(-1));
        var otherType = CreatePublishedItem("Farklı Tür", Now.AddDays(-1), contentTypeId: Guid.NewGuid());
        _contentItemRepository.Seed(sameType);
        _contentItemRepository.Seed(otherType);

        var result = await CreateService().ResolveAsync(Guid.NewGuid(), ContentTypeId, [], [Guid.NewGuid()], Tr, Now);

        Assert.Equal([sameType.Id], result.Select(r => r.Id));
    }

    [Fact]
    public async Task ResolveAsync_ExcludesItemItself()
    {
        var contentItemId = Guid.NewGuid();
        var self = CreatePublishedItem("Kendisi", Now.AddDays(-1));
        _contentItemRepository.Seed(self);

        var result = await CreateService().ResolveAsync(self.Id, ContentTypeId, [], [], Tr, Now);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ResolveAsync_ExcludesItemsWithoutTranslationInRequestedLanguage()
    {
        var trOnly = CreatePublishedItem("Sadece Türkçe", Now.AddDays(-1), languageCode: Tr);
        _contentItemRepository.Seed(trOnly);

        var result = await CreateService().ResolveAsync(Guid.NewGuid(), ContentTypeId, [], [], En, Now);

        Assert.Empty(result);
    }
}
