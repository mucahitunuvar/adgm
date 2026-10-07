using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §10 (Faz 5 Görev 1): pull-model port for indexing published content from another
// module/project without that module depending on Website (ADR-016) - mirrors
// IPublishedJobModuleContract's own pull shape (ADR-023 §6). The Host project registers zero or more
// implementations (e.g. Görev 4's EmployerJobSearchSource); SyncExternalSearchSourcesJob (Görev 4)
// resolves IEnumerable<IExternalSearchSource> and does nothing if none are registered - a project
// with no such module simply runs with an empty source list.
public interface IExternalSearchSource
{
    string SourceKey { get; }

    // Pagination must be stable (same ordering key across calls) so a full sync (Görev 4) can walk
    // every page without skipping or duplicating a document - the same requirement
    // IPublishedJobModuleContract already places on its own callers (ADR-023 §6).
    Task<PagedResult<ExternalSearchDocument>> GetPublishedDocumentsAsync(
        int page, int pageSize, CancellationToken cancellationToken = default);
}
