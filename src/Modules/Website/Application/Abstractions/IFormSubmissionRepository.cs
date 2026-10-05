using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IFormSubmissionRepository
{
    Task<FormSubmission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(FormSubmission formSubmission);

    // ADR-024 §12.2 (Faz 3 Görev 5): the admin list, joined against FormDefinition for its Key (the list
    // response never exposes FormDefinitionId).
    Task<PagedResult<FormSubmissionListItem>> SearchAsync(
        FormSubmissionSearchFilter filter, PagedRequest pagedRequest, CancellationToken cancellationToken = default);

    // §12.2 (Arşiv): ArchiveEligibleSinceUtc <= eligibleBeforeUtc and not yet archived.
    Task<IReadOnlyList<FormSubmission>> GetDueForArchiveAsync(DateTime eligibleBeforeUtc, CancellationToken cancellationToken = default);

    // §12.2 (Anonimleştirme): SubmittedAtUtc + the owning FormDefinition's RetentionDays <= nowUtc, not
    // yet anonymized, capped at maxCount ("Job, toplu işlemlerde tek seferde en fazla 500 başvuru işler").
    Task<IReadOnlyList<FormSubmission>> GetDueForAnonymizationAsync(DateTime nowUtc, int maxCount, CancellationToken cancellationToken = default);

    // Already-anonymized submissions that still have an undeleted file reference - both this run's
    // newly-anonymized submissions and any left over from a previous run's failed storage delete.
    Task<IReadOnlyList<FormSubmission>> GetWithPendingFileDeletionsAsync(int maxCount, CancellationToken cancellationToken = default);
}
