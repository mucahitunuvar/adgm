using GenclikMerkezi.Modules.CareerDevelopment.Domain;

namespace GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;

public interface ITrainingRecommendationRepository
{
    Task<TrainingRecommendation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TrainingRecommendation>> GetByCandidateCvIdAsync(
        Guid candidateCvId, CancellationToken cancellationToken = default);

    void Add(TrainingRecommendation trainingRecommendation);
}
