using GenclikMerkezi.Modules.Matching.Application.Abstractions;
using GenclikMerkezi.Modules.Matching.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Matching.Infrastructure.Persistence;

public sealed class MatchingCandidateSuggestionRepository(MatchingDbContext dbContext) : IMatchingCandidateSuggestionRepository
{
    public Task<CandidateSuggestion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.CandidateSuggestions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<CandidateSuggestion>> GetByPersonnelNeedIdAsync(
        Guid personnelNeedId, CancellationToken cancellationToken = default)
    {
        return await dbContext.CandidateSuggestions
            .Where(s => s.PersonnelNeedId == personnelNeedId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CandidateSuggestion>> GetBySuggestingAdvisorIdAsync(
        Guid suggestingAdvisorId, CancellationToken cancellationToken = default)
    {
        return await dbContext.CandidateSuggestions
            .AsNoTracking()
            .Where(s => s.SuggestingAdvisorId == suggestingAdvisorId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsForPersonnelNeedAndCandidateAsync(
        Guid personnelNeedId, Guid candidateCvId, CancellationToken cancellationToken = default)
    {
        return dbContext.CandidateSuggestions
            .AnyAsync(s => s.PersonnelNeedId == personnelNeedId && s.CandidateCvId == candidateCvId, cancellationToken);
    }

    public void Add(CandidateSuggestion candidateSuggestion)
    {
        dbContext.CandidateSuggestions.Add(candidateSuggestion);
    }
}
