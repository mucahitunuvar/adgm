using GenclikMerkezi.Modules.Candidate.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Candidate.Application.Abstractions;

public interface ICandidateCvRepository
{
    Task<CandidateCv?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CandidateCv?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(CandidateCv candidateCv);

    Task<PagedResult<CandidateCvSummary>> SearchAsync(CandidateCvSearchFilter filter, CancellationToken cancellationToken = default);
}
