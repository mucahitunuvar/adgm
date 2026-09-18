using GenclikMerkezi.Modules.CareerAdvisor.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.DeactivateCareerAdvisor;

public sealed class DeactivateCareerAdvisorCommandHandler(
    ICareerAdvisorRepository careerAdvisorRepository,
    [FromKeyedServices(CareerAdvisorModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivateCareerAdvisorCommand, Result>
{
    public async Task<Result> Handle(DeactivateCareerAdvisorCommand request, CancellationToken cancellationToken)
    {
        var careerAdvisor = await careerAdvisorRepository.GetByIdAsync(request.CareerAdvisorId, cancellationToken);

        if (careerAdvisor is null)
        {
            return Result.Failure(
                Error.NotFound("CareerAdvisor.NotFound", "The specified career advisor could not be found."));
        }

        careerAdvisor.Deactivate(DateTime.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
