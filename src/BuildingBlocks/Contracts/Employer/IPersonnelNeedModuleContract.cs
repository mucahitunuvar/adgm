using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Contracts.Employer;

// ICompanyModuleContract'ın devamı (ADR-016 Decision 2, Option C). Görev 3 kapsamında hiçbir modül
// bunu tüketmiyor - Genel Havuz sayfası (CareerAdvisor Görev 6) ve Matching modülü (Görev 7) için
// hazırlık; ADR-023'ün "cross-module contract'ı hazırlıyoruz ama içini doldurmuyoruz" notu.
public interface IPersonnelNeedModuleContract
{
    Task<IReadOnlyList<PersonnelNeedSummary>> GetGeneralPoolAsync(CancellationToken cancellationToken = default);

    Task<Result> CloseAsync(
        Guid personnelNeedId, Guid closedByAdvisorId, Guid? fulfilledByCandidateCvId, CancellationToken cancellationToken = default);
}
