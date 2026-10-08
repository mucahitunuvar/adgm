namespace GenclikMerkezi.Modules.Website.Features.GetContentItemRevisions;

// ADR-024 §4 (Faz 5 Görev 7): the list row - "satırda anlık görüntü yok" (no SnapshotJson here;
// GetContentItemRevisionByNumber is the only read that returns it).
public sealed record ContentItemRevisionSummaryResponse(
    int RevisionNumber,
    DateTime SavedAtUtc,
    Guid SavedByUserId,
    string Kind,
    IReadOnlyList<string> ChangedLanguages,
    bool IsPublishedSnapshot);
