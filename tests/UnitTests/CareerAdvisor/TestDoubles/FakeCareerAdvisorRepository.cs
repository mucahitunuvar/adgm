using GenclikMerkezi.Modules.CareerAdvisor.Application.Abstractions;

namespace GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;

public sealed class FakeCareerAdvisorRepository : ICareerAdvisorRepository
{
    private readonly List<GenclikMerkezi.Modules.CareerAdvisor.Domain.CareerAdvisor> _careerAdvisors = [];

    public IReadOnlyCollection<GenclikMerkezi.Modules.CareerAdvisor.Domain.CareerAdvisor> CareerAdvisors => _careerAdvisors.AsReadOnly();

    public Task<GenclikMerkezi.Modules.CareerAdvisor.Domain.CareerAdvisor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_careerAdvisors.FirstOrDefault(c => c.Id == id));

    public Task<GenclikMerkezi.Modules.CareerAdvisor.Domain.CareerAdvisor?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_careerAdvisors.FirstOrDefault(c => c.UserId == userId));

    public void Add(GenclikMerkezi.Modules.CareerAdvisor.Domain.CareerAdvisor careerAdvisor)
    {
        _careerAdvisors.Add(careerAdvisor);
    }
}
