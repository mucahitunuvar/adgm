using GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;
using GenclikMerkezi.Modules.CareerDevelopment.Domain;

namespace GenclikMerkezi.UnitTests.CareerDevelopment.TestDoubles;

public sealed class FakeAdvisorRecommendationRepository : IAdvisorRecommendationRepository
{
    private readonly List<AdvisorRecommendation> _advisorRecommendations = [];

    public IReadOnlyCollection<AdvisorRecommendation> AdvisorRecommendations => _advisorRecommendations.AsReadOnly();

    public Task<AdvisorRecommendation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_advisorRecommendations.FirstOrDefault(a => a.Id == id));

    public Task<IReadOnlyList<AdvisorRecommendation>> GetByCandidateCvIdAsync(
        Guid candidateCvId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AdvisorRecommendation> matches =
            _advisorRecommendations.Where(a => a.CandidateCvId == candidateCvId).ToList();
        return Task.FromResult(matches);
    }

    public void Add(AdvisorRecommendation advisorRecommendation)
    {
        _advisorRecommendations.Add(advisorRecommendation);
    }
}
