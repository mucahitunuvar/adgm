using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §4 (Faz 5 Görev 7): called by every handler that mutates a ContentItem's text/SEO/tag/
// category state, in the SAME Unit of Work as the mutation itself (ContentItemRevision lives in the
// same WebsiteDbContext, so both are flushed by that handler's one SaveChangesAsync call - mirrors
// ISearchIndexUpdater's own contract exactly). Takes the already-mutated ContentItem instance (its
// in-memory state after the handler's domain call, not a re-fetch) so the snapshot reflects what is
// about to be saved.
public interface IContentRevisionRecorder
{
    Task RecordAsync(
        ContentItem contentItem,
        ContentItemRevisionKind kind,
        IReadOnlyList<LanguageCode> changedLanguages,
        Guid actingUserId,
        DateTime now,
        CancellationToken cancellationToken = default);
}
