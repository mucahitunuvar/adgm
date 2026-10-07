using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §10 (Faz 5 Görev 2): called by every handler that mutates a ContentItem's searchable
// state, in the SAME Unit of Work as the mutation itself (SearchDocument lives in the same
// WebsiteDbContext, so both are flushed by that handler's one SaveChangesAsync call - this interface
// never calls SaveChangesAsync itself, same rule every repository already follows, AGENTS.md §17).
// Methods take the already-loaded ContentItem instance (not just its id) because a handler that just
// created it has not necessarily flushed it yet - re-fetching by id would find nothing.
public interface ISearchIndexUpdater
{
    // This item alone, every language it has - used by handlers that cannot affect any other item
    // (creation, a single translation's own add/update/delete, duplication).
    Task ReindexAsync(ContentItem contentItem, CancellationToken cancellationToken = default);

    // This item, then every descendant (hierarchy depth up to 3) - used by handlers that can change
    // this item's own visibility or path, either of which a descendant's own visibility depends on
    // (ADR-024 §4.4: "içerik ancak kendisi ve tüm ataları görünürse görünür"). Capped at BatchSize
    // content items per call; the remainder is continued by a Hangfire job
    // (ISearchIndexBatchScheduler.ScheduleSubtreeContinuation).
    Task ReindexWithDescendantsAsync(ContentItem contentItem, CancellationToken cancellationToken = default);

    // Removes every SearchDocument for this content item (every language) - permanent delete only; a
    // merely trashed (soft-deleted) item is simply invisible, which ReindexAsync/
    // ReindexWithDescendantsAsync already handle like any other invisible item.
    Task RemoveAsync(Guid contentItemId, CancellationToken cancellationToken = default);

    // Every content item of this type re-evaluated - the type's IsSearchable, IsActive or any other
    // flag affecting indexing just changed. Same batch cap/continuation as ReindexWithDescendantsAsync.
    Task ReindexContentTypeAsync(Guid contentTypeId, CancellationToken cancellationToken = default);

    // A SiteLanguage was deactivated: every content item's document in that language is removed
    // outright (an inactive language is never publicly visible, so there is nothing to re-evaluate -
    // only something to remove). Same batch cap/continuation.
    Task RemoveLanguageAsync(LanguageCode languageCode, CancellationToken cancellationToken = default);
}
