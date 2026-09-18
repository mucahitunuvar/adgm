using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Domain;

namespace GenclikMerkezi.UnitTests.Candidate.TestDoubles;

public sealed class FakeCandidateCvRepository : ICandidateCvRepository
{
    private readonly List<CandidateCv> _candidateCvs = [];

    public IReadOnlyCollection<CandidateCv> CandidateCvs => _candidateCvs.AsReadOnly();

    public Task<CandidateCv?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_candidateCvs.FirstOrDefault(c => c.Id == id));

    public Task<CandidateCv?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_candidateCvs.FirstOrDefault(c => c.UserId == userId));

    public Task<IReadOnlyDictionary<Guid, int>> GetCandidateCountsByCareerAdvisorAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyDictionary<Guid, int> counts = _candidateCvs
            .Where(c => c.CareerAdvisorId is not null)
            .GroupBy(c => c.CareerAdvisorId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        return Task.FromResult(counts);
    }

    public Task<IReadOnlyList<CandidateCv>> GetByCareerAdvisorIdAsync(
        Guid careerAdvisorId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CandidateCv> matches = _candidateCvs.Where(c => c.CareerAdvisorId == careerAdvisorId).ToList();
        return Task.FromResult(matches);
    }

    public void Add(CandidateCv candidateCv)
    {
        _candidateCvs.Add(candidateCv);
    }
}
