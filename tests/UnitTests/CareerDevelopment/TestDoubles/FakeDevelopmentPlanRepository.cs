using GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;
using GenclikMerkezi.Modules.CareerDevelopment.Domain;

namespace GenclikMerkezi.UnitTests.CareerDevelopment.TestDoubles;

public sealed class FakeDevelopmentPlanRepository : IDevelopmentPlanRepository
{
    private readonly List<DevelopmentPlan> _developmentPlans = [];

    public IReadOnlyCollection<DevelopmentPlan> DevelopmentPlans => _developmentPlans.AsReadOnly();

    public Task<DevelopmentPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_developmentPlans.FirstOrDefault(p => p.Id == id));

    public Task<IReadOnlyList<DevelopmentPlan>> GetByCandidateCvIdAsync(Guid candidateCvId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DevelopmentPlan> matches = _developmentPlans.Where(p => p.CandidateCvId == candidateCvId).ToList();
        return Task.FromResult(matches);
    }

    public void Add(DevelopmentPlan developmentPlan)
    {
        _developmentPlans.Add(developmentPlan);
    }
}
