using GenclikMerkezi.Modules.CareerDevelopment.Domain;

namespace GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;

public interface ICareerGoalRepository
{
    Task<CareerGoal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CareerGoal>> GetByCandidateCvIdAsync(Guid candidateCvId, CancellationToken cancellationToken = default);

    void Add(CareerGoal careerGoal);
}
