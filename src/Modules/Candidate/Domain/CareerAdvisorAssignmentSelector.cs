namespace GenclikMerkezi.Modules.Candidate.Domain;

// En-az-yüklü danışman seçimi (ADR-022 §2). Görev 3'teki ReassignOrphanedCandidatesCommand'ın da
// aynı mantığı tekrar çalıştırması gerektiği için (master prompt'ta açıkça belirtilmiş, spekülatif
// değil) burada paylaşılan, saf bir hesaplayıcı olarak tutuluyor - CandidateCvCompletionCalculator/
// CandidateSearchIndexProjector ile aynı Domain-katmanı statik hesaplayıcı deseni.
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
