using GenclikMerkezi.Modules.CareerAdvisor.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.CareerAdvisor.Application.Abstractions;

public interface ICandidateNoteRepository
{
    void Add(CandidateNote candidateNote);

    Task<PagedResult<CandidateNote>> GetByCandidateCvIdAsync(
        Guid candidateCvId, PagedRequest pagedRequest, CancellationToken cancellationToken = default);
}
