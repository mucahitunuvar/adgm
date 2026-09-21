using GenclikMerkezi.Modules.Employer.Domain;

namespace GenclikMerkezi.Modules.Employer.Application.Abstractions;

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Job>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Job>> GetPublishedAsync(CancellationToken cancellationToken = default);

    // Danışmanın inceleme kuyruğu: Status == UnderReview ve Job.CompanyId'nin Company.CareerAdvisorId'si
    // bu danışmana eşit olan firmalar - Jobs ve Companies aynı EmployerDbContext'te olduğu için tek bir
    // SQL join'e çevrilir (ayrı sorgu + bellekte filtreleme değil).
    Task<IReadOnlyList<Job>> GetPendingReviewByAdvisorIdAsync(Guid careerAdvisorId, CancellationToken cancellationToken = default);

    void Add(Job job);
}
