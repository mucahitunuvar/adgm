using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §10 (Faz 5 Görev 4): a controllable stand-in for Employer's real PublishedJobModuleContract
// (which otherwise requires a full Company-approval/Job-review workflow to produce any data) - lets
// EmployerJobSearchSourceFlowTests drive exactly which jobs "are published" without touching Employer
// at all. Paginates its in-memory Jobs list the same way the real contract does (stable ordering,
// page/pageSize clamped), so a 250-job pagination test exercises the real paging contract shape.
public sealed class FakePublishedJobModuleContract : IPublishedJobModuleContract
{
    public List<PublishedJobSummary> Jobs { get; } = [];

    public Task<PagedResult<PublishedJobSummary>> GetPublishedJobSummariesAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = Math.Clamp(pageSize, 1, 200);
        var items = Jobs.Skip((normalizedPage - 1) * normalizedPageSize).Take(normalizedPageSize).ToList();

        return Task.FromResult(new PagedResult<PublishedJobSummary>(items, Jobs.Count, normalizedPage, normalizedPageSize));
    }
}
