using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface INotFoundLogRepository
{
    Task<NotFoundLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<NotFoundLog?> GetByPathAsync(LanguageCode languageCode, string path, CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    // Default sort is HitCount descending (ADR-024 §15) - the whole point of this list is "which
    // missing paths get hit the most, so an admin can turn the worst offenders into redirects first".
    Task<PagedResult<NotFoundLog>> SearchAsync(PagedRequest pagedRequest, CancellationToken cancellationToken = default);

    // A bulk delete (EF Core ExecuteDelete, not load-then-remove-per-row) - the daily cleanup job can
    // touch thousands of rows and has no business reason to materialize any of them.
    Task<int> DeleteStaleAsync(DateTime olderThanUtc, int maxHitCount, CancellationToken cancellationToken = default);

    void Add(NotFoundLog notFoundLog);

    void Remove(NotFoundLog notFoundLog);

    // Detaches a NotFoundLog whose insert failed a unique-index race (a concurrent request logged the
    // same (LanguageCode, Path) first) from the change tracker, so a later SaveChangesAsync on the same
    // UnitOfWork does not try to insert it again (fix: race-safe not-found logging).
    void DetachFailedAdd(NotFoundLog notFoundLog);
}
