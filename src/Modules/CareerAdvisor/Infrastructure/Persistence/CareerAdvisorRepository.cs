using GenclikMerkezi.Modules.CareerAdvisor.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.CareerAdvisor.Infrastructure.Persistence;

public sealed class CareerAdvisorRepository(CareerAdvisorDbContext dbContext) : ICareerAdvisorRepository
{
    public Task<Domain.CareerAdvisor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.CareerAdvisors.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<Domain.CareerAdvisor?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return dbContext.CareerAdvisors.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    public void Add(Domain.CareerAdvisor careerAdvisor)
    {
        dbContext.CareerAdvisors.Add(careerAdvisor);
    }
}
