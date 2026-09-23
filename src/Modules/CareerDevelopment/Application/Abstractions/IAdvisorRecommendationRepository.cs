using GenclikMerkezi.Modules.CareerDevelopment.Domain;

namespace GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;

public interface IAdvisorRecommendationRepository
{
    Task<AdvisorRecommendation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdvisorRecommendation>> GetByCandidateCvIdAsync(
        Guid candidateCvId, CancellationToken cancellationToken = default);

    void Add(AdvisorRecommendation advisorRecommendation);
}
