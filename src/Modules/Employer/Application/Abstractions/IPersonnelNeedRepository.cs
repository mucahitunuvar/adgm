using GenclikMerkezi.Modules.Employer.Domain;

namespace GenclikMerkezi.Modules.Employer.Application.Abstractions;

public interface IPersonnelNeedRepository
{
    Task<PersonnelNeed?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PersonnelNeed>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);

    // Danışmanın kendi havuzu: Status == KendiHavuzunda ve PersonnelNeed.CompanyId'nin
    // Company.CareerAdvisorId'si bu danışmana eşit olan firmalar - IJobRepository.
    // GetPendingReviewByAdvisorIdAsync join deseniyle aynı.
    Task<IReadOnlyList<PersonnelNeed>> GetOwnPoolByAdvisorIdAsync(Guid careerAdvisorId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PersonnelNeed>> GetGeneralPoolAsync(CancellationToken cancellationToken = default);

    void Add(PersonnelNeed personnelNeed);
}
