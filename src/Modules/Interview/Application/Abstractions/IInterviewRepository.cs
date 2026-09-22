namespace GenclikMerkezi.Modules.Interview.Application.Abstractions;

// Domain.Interview her yerde tam nitelikli - modülün kendi kök ad alanı da "Interview" olduğu için
// (GenclikMerkezi.Modules.Interview), bir "using ...Domain;" + çıplak "Interview" C#'ın üye-arama
// önceliği yüzünden belirsizliğe yol açar (CareerAdvisor modülünün ICareerAdvisorRepository.cs'teki
// aynı desen - modül adı da "CareerAdvisor", aggregate de "CareerAdvisor").
public interface IInterviewRepository
{
    Task<Domain.Interview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Domain.Interview>> GetByCandidateCvIdAsync(Guid candidateCvId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Domain.Interview>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);

    // Danışmanın iş kuyruğu: Status TalepEdildi veya Planlandi olan, kendi organize ettiği görüşmeler
    // - GetInterviewsToOrganize query'si için (IJobRepository.GetPendingReviewByAdvisorIdAsync deseni).
    Task<IReadOnlyList<Domain.Interview>> GetPendingByOrganizingAdvisorIdAsync(
        Guid organizingAdvisorId, CancellationToken cancellationToken = default);

    void Add(Domain.Interview interview);
}
