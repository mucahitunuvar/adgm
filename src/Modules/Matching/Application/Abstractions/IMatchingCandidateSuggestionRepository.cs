using GenclikMerkezi.Modules.Matching.Domain;

namespace GenclikMerkezi.Modules.Matching.Application.Abstractions;

public interface IMatchingCandidateSuggestionRepository
{
    Task<CandidateSuggestion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Aynı PersonnelNeed'e yapılmış tüm öneriler - Accept'teki kardeş-öneri kaskad reddi ve
    // GetSuggestionsForPersonnelNeed query'si için.
    Task<IReadOnlyList<CandidateSuggestion>> GetByPersonnelNeedIdAsync(Guid personnelNeedId, CancellationToken cancellationToken = default);

    // Danışmanın kendi yaptığı öneriler - GetMySuggestions query'si için.
    Task<IReadOnlyList<CandidateSuggestion>> GetBySuggestingAdvisorIdAsync(Guid suggestingAdvisorId, CancellationToken cancellationToken = default);

    // Yinelenen öneriyi engellemek için - durumdan bağımsız (bkz. plan).
    Task<bool> ExistsForPersonnelNeedAndCandidateAsync(
        Guid personnelNeedId, Guid candidateCvId, CancellationToken cancellationToken = default);

    void Add(CandidateSuggestion candidateSuggestion);
}
