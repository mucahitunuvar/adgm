using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Employer.Application.Abstractions;

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Görev 3 (Employer public jobs master prompt): Slug yalnızca ilk yayında atandığı ve sonra
    // değişmediği için (Görev 1) benzersiz bir arama anahtarıdır - taslak ilanlarda null olduğundan
    // bu metot yalnızca slug'ı olan (yani en az bir kez yayınlanmış) ilanları bulabilir.
    Task<Job?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

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

    // Görev 3 (Employer public jobs master prompt): Jobs+Companies tek SQL join'i (AsNoTracking +
    // projeksiyon) ile yalnızca public kurallara uyan (Published ilan + Approved firma) satırları
    // sayfalı döner - GetPendingReviewByAdvisorIdAsync'in companyIds alt sorgusu deseniyle aynı,
    // ama burada firma adı/logo bilgisi de doğrudan projekte ediliyor (handler'da N+1 firma okuması
    // olmasın diye).
    Task<PagedResult<PublicJobListItem>> SearchPublicJobsAsync(
        PublicJobSearchFilter filter, PagedRequest paging, CancellationToken cancellationToken = default);

    void Add(Job job);
}
