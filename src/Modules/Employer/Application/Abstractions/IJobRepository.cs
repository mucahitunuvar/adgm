using GenclikMerkezi.Modules.Employer.Domain;

namespace GenclikMerkezi.Modules.Employer.Application.Abstractions;

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Job>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Job>> GetPublishedAsync(CancellationToken cancellationToken = default);

    // Görev 2 (Employer public jobs master prompt) public firma profili için: GetByCompanyIdAsync'in
    // tüm Job graph'ini (child collection'larla) yüklemesi yerine, yalnızca sayım için tek bir
    // projeksiyon sorgusu.
    Task<int> GetPublishedCountByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);

    // Danışmanın inceleme kuyruğu: Status == UnderReview ve Job.CompanyId'nin Company.CareerAdvisorId'si
    // bu danışmana eşit olan firmalar - Jobs ve Companies aynı EmployerDbContext'te olduğu için tek bir
    // SQL join'e çevrilir (ayrı sorgu + bellekte filtreleme değil).
    Task<IReadOnlyList<Job>> GetPendingReviewByAdvisorIdAsync(Guid careerAdvisorId, CancellationToken cancellationToken = default);

    // Görev 1 (master prompt) BackfillJobSlugsCommand için: Slug eklenmeden önce yayınlanmış
    // (PublishedAtUtc dolu) ama henüz slug'ı olmayan ilanlar. Şu an Published/SuspendedByAdmin
    // dışında bir durumda PublishedAtUtc dolu olamaz (Approve'dan sonra hiçbir geçiş onu temizlemez),
    // bu yüzden ayrı bir Status filtresine gerek yok.
    Task<IReadOnlyList<Job>> GetNeedingSlugBackfillAsync(CancellationToken cancellationToken = default);

    void Add(Job job);
}
