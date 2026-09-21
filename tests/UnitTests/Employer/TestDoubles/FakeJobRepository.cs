using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;

namespace GenclikMerkezi.UnitTests.Employer.TestDoubles;

public sealed class FakeJobRepository : IJobRepository
{
    private readonly List<Job> _jobs = [];
    private readonly Dictionary<Guid, Guid> _companyCareerAdvisorIds = [];

    public IReadOnlyCollection<Job> Jobs => _jobs.AsReadOnly();

    public Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_jobs.FirstOrDefault(j => j.Id == id));

    public Task<IReadOnlyList<Job>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Job> matches = _jobs.Where(j => j.CompanyId == companyId).ToList();
        return Task.FromResult(matches);
    }

    public Task<IReadOnlyList<Job>> GetPublishedAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Job> matches = _jobs.Where(j => j.Status == JobStatus.Published).ToList();
        return Task.FromResult(matches);
    }

    // Testlerde gerçek bir Company/CareerAdvisor join'i olmadığı için, hangi CompanyId'nin hangi
    // CareerAdvisorId'ye ait olduğunu bu fake üzerinde ayrıca kaydetmek gerekiyor.
    public void RegisterCompanyCareerAdvisor(Guid companyId, Guid careerAdvisorId)
    {
        _companyCareerAdvisorIds[companyId] = careerAdvisorId;
    }

    public Task<IReadOnlyList<Job>> GetPendingReviewByAdvisorIdAsync(
        Guid careerAdvisorId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Job> matches = _jobs
            .Where(j => j.Status == JobStatus.UnderReview
                && _companyCareerAdvisorIds.TryGetValue(j.CompanyId, out var advisorId)
                && advisorId == careerAdvisorId)
            .ToList();
        return Task.FromResult(matches);
    }

    public void Add(Job job)
    {
        _jobs.Add(job);
    }
}
