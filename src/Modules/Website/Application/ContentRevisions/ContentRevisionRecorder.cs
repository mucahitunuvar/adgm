using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.ContentRevisions;

// ADR-024 §4 (Faz 5 Görev 7). "İçerik anlık görüntüsü bir önceki revizyonun ContentHash değeriyle
// aynıysa kayıt oluşturulmaz" - but only for Kind != Published: a publish must always leave behind a
// revision flagged IsPublishedSnapshot (the retention job's "yayınlanmış sürümün revizyonu her zaman
// korunur" guarantee needs one to point at), even when the content happens to be byte-identical to the
// last Edited/Created revision.
public sealed class ContentRevisionRecorder(IContentItemRevisionRepository repository) : IContentRevisionRecorder
{
    public async Task RecordAsync(
        ContentItem contentItem,
        ContentItemRevisionKind kind,
        IReadOnlyList<LanguageCode> changedLanguages,
        Guid actingUserId,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var snapshot = ContentItemRevisionSnapshotSerializer.BuildFromContentItem(contentItem);
        var snapshotJson = ContentItemRevisionSnapshotSerializer.Serialize(snapshot);
        var contentHash = ContentItemRevisionHasher.ComputeHash(snapshotJson);

        var latest = await repository.GetLatestAsync(contentItem.Id, cancellationToken);
        if (kind != ContentItemRevisionKind.Published && latest is not null && latest.ContentHash == contentHash)
        {
            return;
        }

        var revisionNumber = (latest?.RevisionNumber ?? 0) + 1;
        var changedLanguageCodes = changedLanguages.Select(l => l.Value).ToList();

        var revision = ContentItemRevision.Create(
            contentItem.Id, revisionNumber, now, actingUserId, kind, changedLanguageCodes, contentHash, snapshotJson);

        repository.Add(revision);
    }
}
