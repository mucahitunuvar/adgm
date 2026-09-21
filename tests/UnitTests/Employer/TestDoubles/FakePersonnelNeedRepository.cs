using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;

namespace GenclikMerkezi.UnitTests.Employer.TestDoubles;

public sealed class FakePersonnelNeedRepository : IPersonnelNeedRepository
{
    private readonly List<PersonnelNeed> _personnelNeeds = [];
    private readonly Dictionary<Guid, Guid> _companyCareerAdvisorIds = [];

    public IReadOnlyCollection<PersonnelNeed> PersonnelNeeds => _personnelNeeds.AsReadOnly();

    public Task<PersonnelNeed?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_personnelNeeds.FirstOrDefault(p => p.Id == id));

    public Task<IReadOnlyList<PersonnelNeed>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PersonnelNeed> matches = _personnelNeeds.Where(p => p.CompanyId == companyId).ToList();
        return Task.FromResult(matches);
    }

    // FakeJobRepository ile aynı desen: testlerde gerçek bir Company/CareerAdvisor join'i olmadığı
    // için hangi CompanyId'nin hangi CareerAdvisorId'ye ait olduğunu ayrıca kaydetmek gerekiyor.
    public void RegisterCompanyCareerAdvisor(Guid companyId, Guid careerAdvisorId)
    {
        _companyCareerAdvisorIds[companyId] = careerAdvisorId;
    }

    public Task<IReadOnlyList<PersonnelNeed>> GetOwnPoolByAdvisorIdAsync(
        Guid careerAdvisorId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PersonnelNeed> matches = _personnelNeeds
            .Where(p => p.Status == PersonnelNeedStatus.KendiHavuzunda
                && _companyCareerAdvisorIds.TryGetValue(p.CompanyId, out var advisorId)
                && advisorId == careerAdvisorId)
            .ToList();
        return Task.FromResult(matches);
    }

    public Task<IReadOnlyList<PersonnelNeed>> GetGeneralPoolAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PersonnelNeed> matches = _personnelNeeds.Where(p => p.Status == PersonnelNeedStatus.GenelHavuzda).ToList();
        return Task.FromResult(matches);
    }

    public void Add(PersonnelNeed personnelNeed)
    {
        _personnelNeeds.Add(personnelNeed);
    }
}
