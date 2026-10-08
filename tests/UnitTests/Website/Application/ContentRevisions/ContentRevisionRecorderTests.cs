using GenclikMerkezi.Modules.Website.Application.ContentRevisions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Application.ContentRevisions;

// ADR-024 §4 (Faz 5 Görev 7): the hash-dedup rule in isolation - a Published snapshot is always
// recorded, every other kind is skipped when nothing in the captured fields actually changed.
public class ContentRevisionRecorderTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly SeoMetadata EmptySeo = SeoMetadata.CreateEmpty();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ContentTypeId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeContentItemRevisionRepository _repository = new();
    private readonly ContentRevisionRecorder _recorder;

    public ContentRevisionRecorderTests()
    {
        _recorder = new ContentRevisionRecorder(_repository);
    }

    private static ContentItem CreateItem() =>
        ContentItem.Create(
            ContentTypeId, null, false, 1, false, null, null, Tr, "Başlık", "slug", "haberler", [], "Özet", "<p>gövde</p>", EmptySeo,
            UserId, Now).Value;

    [Fact]
    public async Task RecordAsync_FirstCall_CreatesRevisionNumberOne()
    {
        var item = CreateItem();

        await _recorder.RecordAsync(item, ContentItemRevisionKind.Created, [Tr], UserId, Now, CancellationToken.None);

        var latest = await _repository.GetLatestAsync(item.Id, CancellationToken.None);
        Assert.NotNull(latest);
        Assert.Equal(1, latest!.RevisionNumber);
        Assert.Equal(ContentItemRevisionKind.Created, latest.Kind);
        Assert.False(latest.IsPublishedSnapshot);
    }

    [Fact]
    public async Task RecordAsync_EditedWithNoActualChange_IsSkipped()
    {
        var item = CreateItem();
        await _recorder.RecordAsync(item, ContentItemRevisionKind.Created, [Tr], UserId, Now, CancellationToken.None);

        // Nothing on `item` changed between the two calls.
        await _recorder.RecordAsync(item, ContentItemRevisionKind.Edited, [Tr], UserId, Now.AddMinutes(5), CancellationToken.None);

        var latest = await _repository.GetLatestAsync(item.Id, CancellationToken.None);
        Assert.Equal(1, latest!.RevisionNumber);
    }

    [Fact]
    public async Task RecordAsync_EditedWithActualChange_CreatesNewRevision()
    {
        var item = CreateItem();
        await _recorder.RecordAsync(item, ContentItemRevisionKind.Created, [Tr], UserId, Now, CancellationToken.None);

        item.SetTranslation(Tr, "Yeni Başlık", "slug", "haberler", [], "Özet", "<p>gövde</p>", EmptySeo, UserId, Now.AddMinutes(5));
        await _recorder.RecordAsync(item, ContentItemRevisionKind.Edited, [Tr], UserId, Now.AddMinutes(5), CancellationToken.None);

        var latest = await _repository.GetLatestAsync(item.Id, CancellationToken.None);
        Assert.Equal(2, latest!.RevisionNumber);
    }

    [Fact]
    public async Task RecordAsync_Published_AlwaysCreatesNewRevisionEvenWithoutChange()
    {
        var item = CreateItem();
        await _recorder.RecordAsync(item, ContentItemRevisionKind.Created, [Tr], UserId, Now, CancellationToken.None);

        // No change to `item`, but Published must never be hash-deduped.
        await _recorder.RecordAsync(item, ContentItemRevisionKind.Published, [], UserId, Now.AddMinutes(5), CancellationToken.None);

        var latest = await _repository.GetLatestAsync(item.Id, CancellationToken.None);
        Assert.Equal(2, latest!.RevisionNumber);
        Assert.Equal(ContentItemRevisionKind.Published, latest.Kind);
        Assert.True(latest.IsPublishedSnapshot);
    }
}
