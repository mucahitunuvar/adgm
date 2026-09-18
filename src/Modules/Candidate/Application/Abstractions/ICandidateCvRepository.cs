using GenclikMerkezi.Modules.Candidate.Domain;

namespace GenclikMerkezi.Modules.Candidate.Application.Abstractions;

public interface ICandidateCvRepository
{
    Task<CandidateCv?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CandidateCv?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(CandidateCv candidateCv);
}
