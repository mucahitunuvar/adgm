namespace GenclikMerkezi.Modules.Website.Application.ContentRevisions;

// ADR-024 §4 (Faz 5 Görev 7): the full captured state for one ContentItem at one point in time -
// every language's translation snapshot plus the content-level CategoryIds. Serialized to/from
// ContentItemRevision.SnapshotJson by ContentItemRevisionSnapshotSerializer; never persisted as its
// own table.
public sealed record ContentItemRevisionSnapshot(
    IReadOnlyList<ContentItemRevisionSnapshotTranslation> Translations, IReadOnlyList<Guid> CategoryIds);
