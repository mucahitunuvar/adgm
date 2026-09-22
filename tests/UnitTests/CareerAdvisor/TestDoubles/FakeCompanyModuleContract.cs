using GenclikMerkezi.Contracts.Employer;

namespace GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;

public sealed class FakeCompanyModuleContract : ICompanyModuleContract
{
    private readonly List<CompanySummary> _companies = [];

    public void Seed(CompanySummary company) => _companies.Add(company);

    public Task<CompanySummary?> GetCompanyByIdAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_companies.FirstOrDefault(c => c.Id == companyId));

    public Task<CompanySummary?> GetCompanyByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_companies.FirstOrDefault(c => c.UserId == userId));

    public Task<Guid?> GetCareerAdvisorIdForCompanyAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_companies.FirstOrDefault(c => c.Id == companyId)?.CareerAdvisorId);

    public Task<IReadOnlyCollection<CompanySummary>> GetCompaniesByIdsAsync(
        IReadOnlyCollection<Guid> companyIds, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<CompanySummary> matched = _companies.Where(c => companyIds.Contains(c.Id)).ToList();
        return Task.FromResult(matched);
    }
}
