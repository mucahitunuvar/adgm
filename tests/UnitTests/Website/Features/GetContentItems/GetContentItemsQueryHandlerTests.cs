using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.GetContentItems;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.GetContentItems;

public class GetContentItemsQueryHandlerTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly SeoMetadata EmptySeo = SeoMetadata.CreateEmpty();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ContentTypeId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 9, 29, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeContentCategoryRepository _contentCategoryRepository = new();
    private readonly FakeSiteLanguageRepository _siteLanguageRepository = new();

    public GetContentItemsQueryHandlerTests()
    {
        var defaultLanguage = SiteLanguage.Create(Tr, "Türkçe", 1, UserId, Now);
        defaultLanguage.MarkAsDefault(UserId, Now);
        _siteLanguageRepository.Add(defaultLanguage);
    }

    private GetContentItemsQueryHandler CreateHandler() => new(_contentItemRepository, _contentCategoryRepository, _siteLanguageRepository);

    private static ContentItem CreateItem(
        string title, DateTime createdAtUtc, int sortOrder = 1, Guid? contentTypeId = null)
    {
        return ContentItem.Create(
            contentTypeId ?? ContentTypeId, null, false, sortOrder, false, null, null,
            Tr, title, null, "haberler", [], null, "<p>gövde</p>", EmptySeo, UserId, createdAtUtc).Value;
    }

    [Theory]
    [InlineData("999")]
    [InlineData("Draft,Published")] // 0|1 = 1 = Published - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedStatus_Fails(string status)
    {
        var result = await CreateHandler().Handle(
            new GetContentItemsQuery(null, status, null, null, null, null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.InvalidStatusFilter", result.Error.Code);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("Title,CreatedAt")]
    public async Task Handle_WithInvalidOrderBy_Fails(string orderBy)
    {
        var result = await CreateHandler().Handle(
            new GetContentItemsQuery(null, null, null, null, null, null, OrderBy: orderBy), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.InvalidOrderBy", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithInvalidDirection_Fails()
    {
        var result = await CreateHandler().Handle(
            new GetContentItemsQuery(null, null, null, null, null, null, OrderBy: "Title", Direction: "Sideways"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.InvalidSortDirection", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithDirectionButNoOrderBy_StillValidatesDirection()
    {
        var result = await CreateHandler().Handle(
            new GetContentItemsQuery(null, null, null, null, null, null, Direction: "Sideways"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.InvalidSortDirection", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithPublishedFromAfterPublishedTo_Fails()
    {
        var result = await CreateHandler().Handle(
            new GetContentItemsQuery(
                null, null, null, null, null, null,
                PublishedFromUtc: Now, PublishedToUtc: Now.AddDays(-1)),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.InvalidPublishedDateRange", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithCreatedFromAfterCreatedTo_Fails()
    {
        var result = await CreateHandler().Handle(
            new GetContentItemsQuery(
                null, null, null, null, null, null,
                CreatedFromUtc: Now, CreatedToUtc: Now.AddDays(-1)),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.InvalidCreatedDateRange", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithCategoryId_IncludesItemsInCategoryAndItsDirectChildrenOnly()
    {
        var root = ContentCategory.Create(ContentTypeId, null, 1, Tr, "Kök", "kok", EmptySeo, UserId, Now).Value;
        var child = ContentCategory.Create(ContentTypeId, root.Id, 1, Tr, "Alt", "alt", EmptySeo, UserId, Now).Value;
        var unrelated = ContentCategory.Create(ContentTypeId, null, 2, Tr, "İlgisiz", "ilgisiz", EmptySeo, UserId, Now).Value;
        _contentCategoryRepository.Add(root);
        _contentCategoryRepository.Add(child);
        _contentCategoryRepository.Add(unrelated);

        var inRoot = CreateItem("Kökte", Now);
        inRoot.SetCategories([root.Id], UserId, Now);
        var inChild = CreateItem("Altta", Now);
        inChild.SetCategories([child.Id], UserId, Now);
        var inUnrelated = CreateItem("İlgisizde", Now);
        inUnrelated.SetCategories([unrelated.Id], UserId, Now);
        var uncategorized = CreateItem("Kategorisiz", Now);
        _contentItemRepository.Seed(inRoot);
        _contentItemRepository.Seed(inChild);
        _contentItemRepository.Seed(inUnrelated);
        _contentItemRepository.Seed(uncategorized);

        var result = await CreateHandler().Handle(
            new GetContentItemsQuery(null, null, null, null, null, null, CategoryId: root.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Contains(result.Value.Items, i => i.Title == "Kökte");
        Assert.Contains(result.Value.Items, i => i.Title == "Altta");
    }

    [Fact]
    public async Task Handle_WithNonExistentCategoryId_ReturnsEmptyRatherThanUnfiltered()
    {
        var item = CreateItem("Herhangi", Now);
        _contentItemRepository.Seed(item);

        var result = await CreateHandler().Handle(
            new GetContentItemsQuery(null, null, null, null, null, null, CategoryId: Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.TotalCount);
    }

    [Fact]
    public async Task Handle_WithTagId_FiltersByTagOnTheResolvedLanguage()
    {
        var tagId = Guid.NewGuid();
        var tagged = CreateItem("Etiketli", Now);
        tagged.SetTranslationTags(Tr, [tagId], UserId, Now);
        var untagged = CreateItem("Etiketsiz", Now);
        _contentItemRepository.Seed(tagged);
        _contentItemRepository.Seed(untagged);

        var result = await CreateHandler().Handle(
            new GetContentItemsQuery(null, null, null, null, null, null, TagId: tagId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Equal("Etiketli", result.Value.Items.Single().Title);
    }

    [Fact]
    public async Task Handle_WithPublishedDateRange_IsHalfOpenAndUsesEffectivePublishDate()
    {
        var early = CreateItem("Erken", Now);
        early.Publish(null, null, true, true, Tr, UserId, Now.AddDays(-10)); // PublishedAtUtc = Now-10d
        var late = CreateItem("Geç", Now);
        late.Publish(null, null, true, true, Tr, UserId, Now); // PublishedAtUtc = Now
        var unpublished = CreateItem("Yayınlanmamış", Now);
        _contentItemRepository.Seed(early);
        _contentItemRepository.Seed(late);
        _contentItemRepository.Seed(unpublished);

        // [Now-5d, Now) - half-open: "late" (== Now) must be excluded, "early" (Now-10d) excluded too.
        var result = await CreateHandler().Handle(
            new GetContentItemsQuery(
                null, null, null, null, null, null,
                PublishedFromUtc: Now.AddDays(-5), PublishedToUtc: Now),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.TotalCount);

        var inclusiveOfLate = await CreateHandler().Handle(
            new GetContentItemsQuery(
                null, null, null, null, null, null,
                PublishedFromUtc: Now.AddDays(-5), PublishedToUtc: Now.AddSeconds(1)),
            CancellationToken.None);

        Assert.Equal(1, inclusiveOfLate.Value.TotalCount);
        Assert.Equal("Geç", inclusiveOfLate.Value.Items.Single().Title);
    }

    [Fact]
    public async Task Handle_WithCreatedDateRange_IsHalfOpen()
    {
        var early = CreateItem("Erken", Now.AddDays(-10));
        var late = CreateItem("Geç", Now);
        _contentItemRepository.Seed(early);
        _contentItemRepository.Seed(late);

        var result = await CreateHandler().Handle(
            new GetContentItemsQuery(
                null, null, null, null, null, null,
                CreatedFromUtc: Now.AddDays(-5), CreatedToUtc: Now),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.TotalCount); // "late" (== Now) excluded by the half-open upper bound.

        var inclusive = await CreateHandler().Handle(
            new GetContentItemsQuery(
                null, null, null, null, null, null,
                CreatedFromUtc: Now.AddDays(-5), CreatedToUtc: Now.AddSeconds(1)),
            CancellationToken.None);

        Assert.Equal("Geç", inclusive.Value.Items.Single().Title);
    }

    [Fact]
    public async Task Handle_WithOrderByTitleDescending_SortsAccordingly()
    {
        _contentItemRepository.Seed(CreateItem("A Başlık", Now, sortOrder: 2));
        _contentItemRepository.Seed(CreateItem("B Başlık", Now, sortOrder: 1));

        var result = await CreateHandler().Handle(
            new GetContentItemsQuery(null, null, null, null, null, null, OrderBy: "Title", Direction: "Desc"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(["B Başlık", "A Başlık"], result.Value.Items.Select(i => i.Title));
    }

    [Fact]
    public async Task Handle_WithoutOrderBy_KeepsDefaultSortOrderThenIdBehavior()
    {
        _contentItemRepository.Seed(CreateItem("İkinci", Now, sortOrder: 2));
        _contentItemRepository.Seed(CreateItem("Birinci", Now, sortOrder: 1));

        var result = await CreateHandler().Handle(
            new GetContentItemsQuery(null, null, null, null, null, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(["Birinci", "İkinci"], result.Value.Items.Select(i => i.Title));
    }
}
