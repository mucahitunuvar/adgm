using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Contracts.Employer;

// ICompanyModuleContract'ın devamı (ADR-016 Decision 2, Option C). Görev 3'te hiçbir modül bunu
// tüketmiyordu - Genel Havuz sayfası (CareerAdvisor Görev 6) artık gerçek tüketicisi; Matching
// modülü (Görev 7) CloseAsync'i kullanacak.
public interface IPersonnelNeedModuleContract
{
    // CareerAdvisor Görev 6: sayfalı (ARCHITECTURE.md §9) - en yeni önce (PooledAtUtc descending).
    Task<PagedResult<PersonnelNeedSummary>> GetGeneralPoolAsync(
        PagedRequest request, CancellationToken cancellationToken = default);

    // Matching Görev 7: CandidateSuggestion'ın reviewer-zinciri (Job/PersonnelNeed review deseniyle
    // aynı) için CompanyId'yi çözmek, ve Accept/Reject'te ihtiyacı bulmak için - durumdan bağımsız
    // döner (Status Contracts'a taşınmaz, IsInGeneralPoolAsync'in ayrı var olma nedeni).
    Task<PersonnelNeedSummary?> GetByIdAsync(Guid personnelNeedId, CancellationToken cancellationToken = default);

    // Matching Görev 7: CreateCandidateSuggestionCommand'ın "ihtiyaç Genel Havuz'da mı" ön koşulu -
    // PersonnelNeedStatus.GenelHavuzda kontrolü modül içinde yapılır, yalnızca bool döner (Contracts,
    // Employer.Domain'e bağımlı olamaz).
    Task<bool> IsInGeneralPoolAsync(Guid personnelNeedId, CancellationToken cancellationToken = default);

    Task<Result> CloseAsync(
        Guid personnelNeedId, Guid closedByAdvisorId, Guid? fulfilledByCandidateCvId, CancellationToken cancellationToken = default);
}
