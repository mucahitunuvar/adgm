using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;

namespace GenclikMerkezi.UnitTests.Employer.TestDoubles;

public sealed class FakeCompanyRepository : ICompanyRepository
{
    private readonly List<Company> _companies = [];

    public IReadOnlyCollection<Company> Companies => _companies.AsReadOnly();

    public Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_companies.FirstOrDefault(c => c.Id == id));

    public Task<Company?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_companies.FirstOrDefault(c => c.UserId == userId));

    public Task<IReadOnlyDictionary<Guid, int>> GetCompanyCountsByCareerAdvisorAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyDictionary<Guid, int> counts = _companies
            .Where(c => c.CareerAdvisorId is not null
                && c.Status != CompanyStatus.Rejected
                && c.Status != CompanyStatus.Deactivated)
            .GroupBy(c => c.CareerAdvisorId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        return Task.FromResult(counts);
    }

    public Task<IReadOnlyList<Company>> GetByCareerAdvisorIdAsync(Guid careerAdvisorId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Company> matches = _companies.Where(c => c.CareerAdvisorId == careerAdvisorId).ToList();
        return Task.FromResult(matches);
    }

    public void Add(Company company)
    {
        _companies.Add(company);
    }
}
