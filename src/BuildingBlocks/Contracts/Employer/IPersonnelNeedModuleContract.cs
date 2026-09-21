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

    Task<Result> CloseAsync(
        Guid personnelNeedId, Guid closedByAdvisorId, Guid? fulfilledByCandidateCvId, CancellationToken cancellationToken = default);
}
