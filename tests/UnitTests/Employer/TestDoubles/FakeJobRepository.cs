using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Employer.TestDoubles;

public sealed class FakeJobRepository : IJobRepository
{
    private readonly List<Job> _jobs = [];
    private readonly Dictionary<Guid, Guid> _companyCareerAdvisorIds = [];

    // SearchPublicJobsAsync'in gerçek Jobs+Companies join'ini (JobRepository) taklit etmek için -
    // GetPendingReviewByAdvisorIdAsync'in RegisterCompanyCareerAdvisor deseniyle aynı gerekçe.
    private readonly Dictionary<Guid, (string Name, bool HasLogo, CompanyStatus Status)> _companies = [];

    public IReadOnlyCollection<Job> Jobs => _jobs.AsReadOnly();

    public void RegisterCompany(Guid companyId, string name, bool hasLogo, CompanyStatus status)
    {
        _companies[companyId] = (name, hasLogo, status);
    }

    public Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_jobs.FirstOrDefault(j => j.Id == id));

    public Task<Job?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        Task.FromResult(_jobs.FirstOrDefault(j => j.Slug == slug));

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

    public Task<int> GetPublishedCountByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var count = _jobs.Count(j => j.CompanyId == companyId && j.Status == JobStatus.Published);
        return Task.FromResult(count);
    }

    public Task<PagedResult<PublicJobListItem>> SearchPublicJobsAsync(
        PublicJobSearchFilter filter, PagedRequest paging, CancellationToken cancellationToken = default)
    {
        var matches = _jobs
            .Where(j => j.Status == JobStatus.Published)
            .Select(j => (Job: j, Company: _companies.GetValueOrDefault(j.CompanyId)))
            .Where(x => x.Company.Name is not null && x.Company.Status == CompanyStatus.Approved)
            .Where(x => filter.ProvinceId is null || x.Job.ProvinceId == filter.ProvinceId)
            .Where(x => filter.EmploymentTypeId is null || x.Job.EmploymentTypeId == filter.EmploymentTypeId)
            .Where(x => filter.WorkLocationTypeId is null || x.Job.WorkLocationTypeId == filter.WorkLocationTypeId)
            .Where(x => filter.PositionId is null || x.Job.PositionId == filter.PositionId)
            .Where(x => filter.DepartmentId is null || x.Job.DepartmentId == filter.DepartmentId)
            .Where(x => filter.CompanyId is null || x.Job.CompanyId == filter.CompanyId)
            .Where(x => filter.IsForDisabledCandidates is null || x.Job.IsForDisabledCandidates == filter.IsForDisabledCandidates)
            .Where(x => string.IsNullOrWhiteSpace(filter.Search)
                || x.Job.Title.Contains(filter.Search, StringComparison.OrdinalIgnoreCase)
                || x.Company.Name!.Contains(filter.Search, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.Job.PublishedAtUtc)
            .ThenByDescending(x => x.Job.Id)
            .ToList();

        var page = matches
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .Select(x => new PublicJobListItem(
                x.Job.Id, x.Job.Slug, x.Job.Title, x.Job.CompanyId, x.Company.Name!, x.Company.HasLogo,
                x.Job.ProvinceId, x.Job.EmploymentTypeId, x.Job.WorkLocationTypeId, x.Job.PositionId,
                x.Job.IsForDisabledCandidates, x.Job.PublishedAtUtc!.Value, x.Job.DescriptionHtml))
            .ToList();

        return Task.FromResult(new PagedResult<PublicJobListItem>(page, matches.Count, paging.Page, paging.PageSize));
    }

    public Task<IReadOnlyList<Job>> GetNeedingSlugBackfillAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Job> matches = _jobs.Where(j => j.Slug is null && j.PublishedAtUtc is not null).ToList();
        return Task.FromResult(matches);
    }

    public void Add(Job job)
    {
        _jobs.Add(job);
    }
}
