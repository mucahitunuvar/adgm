using GenclikMerkezi.Contracts.Candidate;

namespace GenclikMerkezi.UnitTests.Matching.TestDoubles;

public sealed class FakeCandidateModuleContract : ICandidateModuleContract
{
    private readonly Dictionary<Guid, CandidateCvSummary> _candidateCvs = [];

    public void Seed(CandidateCvSummary candidateCv) => _candidateCvs[candidateCv.Id] = candidateCv;

    public Task<CandidateCvSummary?> GetCandidateCvByIdAsync(Guid candidateCvId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_candidateCvs.TryGetValue(candidateCvId, out var summary) ? summary : null);

    public Task<Guid?> GetCareerAdvisorIdForCandidateAsync(Guid candidateCvId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_candidateCvs.TryGetValue(candidateCvId, out var summary) ? summary.CareerAdvisorId : null);
}
