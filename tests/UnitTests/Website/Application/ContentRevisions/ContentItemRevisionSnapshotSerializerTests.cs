using GenclikMerkezi.Modules.Website.Application.ContentRevisions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Application.ContentRevisions;

// ADR-024 §4 (Faz 5 Görev 7): determinism matters here - ContentRevisionRecorder's hash-based dedup
// depends on two calls against logically identical state producing byte-identical JSON.
public class ContentItemRevisionSnapshotSerializerTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LanguageCode En = LanguageCode.Create("en").Value;
    private static readonly SeoMetadata EmptySeo = SeoMetadata.CreateEmpty();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ContentTypeId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private static ContentItem CreateItem() =>
        ContentItem.Create(
            ContentTypeId, null, false, 1, false, null, null, Tr, "Başlık", "slug", "haberler", [], "Özet", "<p>gövde</p>", EmptySeo,
            UserId, Now).Value;

    [Fact]
    public void BuildFromContentItem_TagOrderDoesNotAffectSerializedJson()
    {
        var tagA = Guid.NewGuid();
        var tagB = Guid.NewGuid();

        var itemAscending = CreateItem();
        itemAscending.SetTranslationTags(Tr, [tagA, tagB], UserId, Now);

        var itemDescending = CreateItem();
        itemDescending.SetTranslationTags(Tr, [tagB, tagA], UserId, Now);

        var jsonAscending = ContentItemRevisionSnapshotSerializer.Serialize(ContentItemRevisionSnapshotSerializer.BuildFromContentItem(itemAscending));
        var jsonDescending = ContentItemRevisionSnapshotSerializer.Serialize(ContentItemRevisionSnapshotSerializer.BuildFromContentItem(itemDescending));

        Assert.Equal(jsonAscending, jsonDescending);
    }

    [Fact]
    public void BuildFromContentItem_OrdersTranslationsByLanguageCodeRegardlessOfInsertionOrder()
    {
        var item = CreateItem();
        item.SetTranslation(En, "Title", "slug-en", "news", [], "Summary", "<p/>", EmptySeo, UserId, Now);

        var snapshot = ContentItemRevisionSnapshotSerializer.BuildFromContentItem(item);

        Assert.Equal("en", snapshot.Translations[0].LanguageCode);
        Assert.Equal("tr", snapshot.Translations[1].LanguageCode);
    }

    [Fact]
    public void BuildFromContentItem_CapturesOnlyTheDocumentedSeoSubset()
    {
        var seo = SeoMetadata.Create(
            "Meta Title", "Meta Description", "kw1,kw2", "Og Title", "Og Description", Guid.NewGuid(),
            "https://example.org/canon", true).Value;
        var item = ContentItem.Create(
            ContentTypeId, null, false, 1, false, null, null, Tr, "Başlık", "slug", "haberler", [], "Özet", "<p/>", seo, UserId, Now).Value;

        var snapshot = ContentItemRevisionSnapshotSerializer.BuildFromContentItem(item);
        var translation = snapshot.Translations[0];

        Assert.Equal("Meta Title", translation.MetaTitle);
        Assert.Equal("Meta Description", translation.MetaDescription);
        Assert.Equal("Og Title", translation.OgTitle);
        Assert.Equal("Og Description", translation.OgDescription);
        Assert.True(translation.NoIndex);
        Assert.Equal("https://example.org/canon", translation.CanonicalUrl);
    }

    [Fact]
    public void SerializeThenDeserialize_RoundTripsEveryField()
    {
        var item = CreateItem();
        item.SetTranslationTags(Tr, [Guid.NewGuid()], UserId, Now);
        item.SetCategories([Guid.NewGuid(), Guid.NewGuid()], UserId, Now);

        var original = ContentItemRevisionSnapshotSerializer.BuildFromContentItem(item);
        var roundTripped = ContentItemRevisionSnapshotSerializer.Deserialize(ContentItemRevisionSnapshotSerializer.Serialize(original));

        // record equality falls back to reference equality for list-typed properties, so each field is
        // asserted individually (Assert.Equal does structural, order-sensitive comparison for
        // sequences) rather than comparing the two ContentItemRevisionSnapshot instances directly.
        Assert.Equal(original.CategoryIds, roundTripped.CategoryIds);
        Assert.Equal(original.Translations.Count, roundTripped.Translations.Count);
        var originalTranslation = original.Translations[0];
        var roundTrippedTranslation = roundTripped.Translations[0];
        Assert.Equal(originalTranslation.LanguageCode, roundTrippedTranslation.LanguageCode);
        Assert.Equal(originalTranslation.Title, roundTrippedTranslation.Title);
        Assert.Equal(originalTranslation.Summary, roundTrippedTranslation.Summary);
        Assert.Equal(originalTranslation.Body, roundTrippedTranslation.Body);
        Assert.Equal(originalTranslation.MetaTitle, roundTrippedTranslation.MetaTitle);
        Assert.Equal(originalTranslation.CanonicalUrl, roundTrippedTranslation.CanonicalUrl);
        Assert.Equal(originalTranslation.TagIds, roundTrippedTranslation.TagIds);
    }
}
