namespace GenclikMerkezi.Modules.CareerAdvisor.Application.Abstractions;

public interface ICareerAdvisorRepository
{
    Task<Domain.CareerAdvisor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Domain.CareerAdvisor?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(Domain.CareerAdvisor careerAdvisor);
}
