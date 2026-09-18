using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Modules.CareerAdvisor.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;

public sealed class CreateCareerAdvisorCommandHandler(
    IIdentityService identityService,
    ICareerAdvisorRepository careerAdvisorRepository,
    [FromKeyedServices(CareerAdvisorModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCareerAdvisorCommand, Result<CreateCareerAdvisorResponse>>
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public async Task<Result<CreateCareerAdvisorResponse>> Handle(
        CreateCareerAdvisorCommand request, CancellationToken cancellationToken)
    {
        var createUserResult = await identityService.CreateStaffUserAsync(
            request.Email, request.Password, request.FirstName, request.LastName, request.PhoneNumber, CareerAdvisorRole, cancellationToken);

        if (createUserResult.IsFailure)
        {
            return Result.Failure<CreateCareerAdvisorResponse>(createUserResult.Error);
        }

        var userId = createUserResult.Value;

        try
        {
            var careerAdvisor = Domain.CareerAdvisor.Create(
                userId, request.FirstName, request.LastName, request.Email, request.PhoneNumber, DateTime.UtcNow);
            careerAdvisorRepository.Add(careerAdvisor);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new CreateCareerAdvisorResponse(userId, careerAdvisor.Id));
        }
        catch
        {
            await identityService.DeactivateUserAsync(userId, cancellationToken);
            throw;
        }
    }
}
