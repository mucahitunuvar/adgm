using GenclikMerkezi.Modules.Candidate.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Candidate.Application.Abstractions;

public interface ICandidateSearchIndexRepository
{
    Task<CandidateSearchIndex?> GetByCandidateCvIdAsync(Guid candidateCvId, CancellationToken cancellationToken = default);

    void Add(CandidateSearchIndex index);

    Task<PagedResult<CandidateSearchIndexSummary>> SearchAsync(
        CandidateSearchIndexFilter filter, CancellationToken cancellationToken = default);
}
