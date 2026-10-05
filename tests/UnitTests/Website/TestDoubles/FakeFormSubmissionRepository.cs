using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

// Job unit tests (ArchiveClosedFormSubmissionsJobTests/AnonymizeExpiredFormSubmissionsJobTests) only
// need GetDueForArchiveAsync/GetDueForAnonymizationAsync/GetWithPendingFileDeletionsAsync - the actual
// "SubmittedAtUtc + FormDefinition.RetentionDays" join lives in the real repository and is covered by
// FormSubmissionFlowTests (integration) instead, since this fake has no FormDefinition data to join
// against. GetDueForAnonymizationAsync here simply treats "due" as SubmittedAtUtc <= nowUtc, so a job
// test sets up the retention window itself by choosing SubmittedAtUtc relative to the "now" it passes.
public sealed class FakeFormSubmissionRepository : IFormSubmissionRepository
{
    private readonly List<FormSubmission> _submissions = [];

    public void Seed(FormSubmission submission) => _submissions.Add(submission);

    public Task<FormSubmission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_submissions.FirstOrDefault(s => s.Id == id));

    public void Add(FormSubmission formSubmission) => _submissions.Add(formSubmission);

    public Task<PagedResult<FormSubmissionListItem>> SearchAsync(
        FormSubmissionSearchFilter filter, PagedRequest pagedRequest, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not needed by job unit tests - covered by FormSubmissionFlowTests (integration).");

    public Task<IReadOnlyList<FormSubmission>> GetDueForArchiveAsync(DateTime eligibleBeforeUtc, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<FormSubmission> due = _submissions
            .Where(s => s.ArchivedAtUtc is null && s.ArchiveEligibleSinceUtc is not null && s.ArchiveEligibleSinceUtc <= eligibleBeforeUtc)
            .ToList();
        return Task.FromResult(due);
    }

    public Task<IReadOnlyList<FormSubmission>> GetDueForAnonymizationAsync(
        DateTime nowUtc, int maxCount, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<FormSubmission> due = _submissions
            .Where(s => s.AnonymizedAtUtc is null && s.SubmittedAtUtc <= nowUtc)
            .Take(maxCount)
            .ToList();
        return Task.FromResult(due);
    }

    public Task<IReadOnlyList<FormSubmission>> GetWithPendingFileDeletionsAsync(int maxCount, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<FormSubmission> withPending = _submissions
            .Where(s => s.PendingFileDeletions.Count > 0)
            .Take(maxCount)
            .ToList();
        return Task.FromResult(withPending);
    }
}
