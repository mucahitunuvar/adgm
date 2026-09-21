namespace GenclikMerkezi.Modules.Employer.Domain;

// En-az-yüklü danışman seçimi (ADR-022 §2). Candidate.Domain.CareerAdvisorAssignmentSelector ile
// birebir aynı saf hesaplayıcı - master prompt bilinçli olarak SharedKernel'e taşımak yerine
// kopyalamaya izin veriyor (kapsamı genişletmemek için), Candidate tarafındaki mevcut implementasyon
// ve testleri etkilenmiyor.
public static class CareerAdvisorAssignmentSelector
{
    public static Guid? SelectLeastLoaded(
        IReadOnlyList<Guid> activeCareerAdvisorIds, IReadOnlyDictionary<Guid, int> workloadCounts)
    {
        if (activeCareerAdvisorIds.Count == 0)
        {
            return null;
        }

        // Eşitlikte Guid'e göre sıralama anlamsız ama deterministik bir tie-break sağlar; ADR-022 §2
        // zaten eşzamanlı kayıtlarda hafif dengesizlik riskini kabul ediyor.
        return activeCareerAdvisorIds
            .OrderBy(id => workloadCounts.GetValueOrDefault(id, 0))
            .ThenBy(id => id)
            .First();
    }
}
