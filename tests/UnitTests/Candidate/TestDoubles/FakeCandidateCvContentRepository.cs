using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Domain;

namespace GenclikMerkezi.UnitTests.Candidate.TestDoubles;

public sealed class FakeCandidateCvContentRepository : ICandidateCvContentRepository
{
    private readonly List<CandidateCvContent> _candidateCvContents = [];

    public IReadOnlyCollection<CandidateCvContent> CandidateCvContents => _candidateCvContents.AsReadOnly();

    public Task<CandidateCvContent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_candidateCvContents.FirstOrDefault(c => c.Id == id));

    public Task<CandidateCvContent?> GetByCandidateCvIdAsync(Guid candidateCvId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_candidateCvContents.FirstOrDefault(c => c.CandidateCvId == candidateCvId));

    public void Add(CandidateCvContent candidateCvContent)
    {
        _candidateCvContents.Add(candidateCvContent);
    }
}
