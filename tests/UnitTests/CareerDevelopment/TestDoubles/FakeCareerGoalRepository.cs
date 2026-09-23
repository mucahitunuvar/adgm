using GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;
using GenclikMerkezi.Modules.CareerDevelopment.Domain;

namespace GenclikMerkezi.UnitTests.CareerDevelopment.TestDoubles;

public sealed class FakeCareerGoalRepository : ICareerGoalRepository
{
    private readonly List<CareerGoal> _careerGoals = [];

    public IReadOnlyCollection<CareerGoal> CareerGoals => _careerGoals.AsReadOnly();

    public Task<CareerGoal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_careerGoals.FirstOrDefault(g => g.Id == id));

    public Task<IReadOnlyList<CareerGoal>> GetByCandidateCvIdAsync(Guid candidateCvId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CareerGoal> matches = _careerGoals.Where(g => g.CandidateCvId == candidateCvId).ToList();
        return Task.FromResult(matches);
    }

    public void Add(CareerGoal careerGoal)
    {
        _careerGoals.Add(careerGoal);
    }
}
