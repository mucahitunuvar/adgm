using GenclikMerkezi.Modules.Candidate.Domain;

namespace GenclikMerkezi.Modules.Candidate.Application.Abstractions;

public interface ICandidateCvContentRepository
{
    Task<CandidateCvContent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CandidateCvContent?> GetByCandidateCvIdAsync(Guid candidateCvId, CancellationToken cancellationToken = default);

    void Add(CandidateCvContent candidateCvContent);
}
