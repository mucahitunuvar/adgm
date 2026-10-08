using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4 (Faz 5 Görev 7): a point-in-time snapshot of one ContentItem's text/SEO/tag/category
// fields - a plain record table, not a separate aggregate root (nothing downstream reacts to a
// revision being saved, same reasoning SearchDocument's own remarks give for being a plain Entity).
// SnapshotJson is produced and interpreted entirely in the Application layer
// (ContentRevisionRecorder/ContentItemRevisionSnapshot) - this type never parses it, only stores it,
// so Domain stays free of any serialization framework dependency (AGENTS.md §7).
public sealed class ContentItemRevision : Entity
{
    public const int MaxContentHashLength = 64;

    private readonly List<string> _changedLanguages = [];

    public Guid ContentItemId { get; private set; }

    // Increasing per ContentItemId, assigned by the caller (ContentRevisionRecorder reads the current
    // highest number for this item and adds one) - never reused, even across deletions by the
    // retention job, so a revision number always identifies the same point in time it always did.
    public int RevisionNumber { get; private set; }

    public DateTime SavedAtUtc { get; private set; }

    public Guid SavedByUserId { get; private set; }

    public ContentItemRevisionKind Kind { get; private set; }

    public IReadOnlyList<string> ChangedLanguages => _changedLanguages.AsReadOnly();

    // Only ever true together with Kind == Published (ContentRevisionRecorder's invariant) - the
    // retention job keeps the newest such revision regardless of the normal 50-revision cap.
    public bool IsPublishedSnapshot { get; private set; }

    public string ContentHash { get; private set; } = string.Empty;

    public string SnapshotJson { get; private set; } = string.Empty;

    private ContentItemRevision(
        Guid id,
        Guid contentItemId,
        int revisionNumber,
        DateTime savedAtUtc,
        Guid savedByUserId,
        ContentItemRevisionKind kind,
        IReadOnlyList<string> changedLanguages,
        bool isPublishedSnapshot,
        string contentHash,
        string snapshotJson)
        : base(id)
    {
        ContentItemId = contentItemId;
        RevisionNumber = revisionNumber;
        SavedAtUtc = savedAtUtc;
        SavedByUserId = savedByUserId;
        Kind = kind;
        _changedLanguages.AddRange(changedLanguages);
        IsPublishedSnapshot = isPublishedSnapshot;
        ContentHash = contentHash;
        SnapshotJson = snapshotJson;
    }

    private ContentItemRevision()
    {
    }

    public static ContentItemRevision Create(
        Guid contentItemId,
        int revisionNumber,
        DateTime savedAtUtc,
        Guid savedByUserId,
        ContentItemRevisionKind kind,
        IReadOnlyList<string> changedLanguages,
        string contentHash,
        string snapshotJson) =>
        new(
            Guid.NewGuid(), contentItemId, revisionNumber, savedAtUtc, savedByUserId, kind, changedLanguages,
            isPublishedSnapshot: kind == ContentItemRevisionKind.Published, contentHash, snapshotJson);
}
