using GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;
using GenclikMerkezi.Modules.CareerDevelopment.Domain;

namespace GenclikMerkezi.UnitTests.CareerDevelopment.TestDoubles;

public sealed class FakeTrainingRecommendationRepository : ITrainingRecommendationRepository
{
    private readonly List<TrainingRecommendation> _trainingRecommendations = [];

    public IReadOnlyCollection<TrainingRecommendation> TrainingRecommendations => _trainingRecommendations.AsReadOnly();

    public Task<TrainingRecommendation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_trainingRecommendations.FirstOrDefault(t => t.Id == id));

    public Task<IReadOnlyList<TrainingRecommendation>> GetByCandidateCvIdAsync(
        Guid candidateCvId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TrainingRecommendation> matches =
            _trainingRecommendations.Where(t => t.CandidateCvId == candidateCvId).ToList();
        return Task.FromResult(matches);
    }

    public void Add(TrainingRecommendation trainingRecommendation)
    {
        _trainingRecommendations.Add(trainingRecommendation);
    }
}
