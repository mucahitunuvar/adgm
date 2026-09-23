using GenclikMerkezi.Modules.CareerDevelopment.Domain;

namespace GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;

public interface IDevelopmentPlanRepository
{
    Task<DevelopmentPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DevelopmentPlan>> GetByCandidateCvIdAsync(Guid candidateCvId, CancellationToken cancellationToken = default);

    void Add(DevelopmentPlan developmentPlan);
}
