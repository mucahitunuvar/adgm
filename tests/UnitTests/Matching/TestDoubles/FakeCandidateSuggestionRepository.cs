using GenclikMerkezi.Modules.Matching.Application.Abstractions;
using GenclikMerkezi.Modules.Matching.Domain;

namespace GenclikMerkezi.UnitTests.Matching.TestDoubles;

public sealed class FakeCandidateSuggestionRepository : IMatchingCandidateSuggestionRepository
{
    private readonly List<CandidateSuggestion> _suggestions = [];

    public IReadOnlyCollection<CandidateSuggestion> Suggestions => _suggestions.AsReadOnly();

    public Task<CandidateSuggestion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_suggestions.FirstOrDefault(s => s.Id == id));

    public Task<IReadOnlyList<CandidateSuggestion>> GetByPersonnelNeedIdAsync(
        Guid personnelNeedId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CandidateSuggestion> matches = _suggestions.Where(s => s.PersonnelNeedId == personnelNeedId).ToList();
        return Task.FromResult(matches);
    }

    public Task<IReadOnlyList<CandidateSuggestion>> GetBySuggestingAdvisorIdAsync(
        Guid suggestingAdvisorId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CandidateSuggestion> matches = _suggestions.Where(s => s.SuggestingAdvisorId == suggestingAdvisorId).ToList();
        return Task.FromResult(matches);
    }

    public Task<bool> ExistsForPersonnelNeedAndCandidateAsync(
        Guid personnelNeedId, Guid candidateCvId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_suggestions.Any(s => s.PersonnelNeedId == personnelNeedId && s.CandidateCvId == candidateCvId));

    public void Add(CandidateSuggestion candidateSuggestion)
    {
        _suggestions.Add(candidateSuggestion);
    }
}
