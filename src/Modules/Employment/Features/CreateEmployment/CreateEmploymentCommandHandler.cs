using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.Employment.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employment.Features.CreateEmployment;

// CreateCandidateSuggestionCommandHandler'daki "yalnızca kendi adayını önerebilir" kontrolünün
// birebir aynısı: yalnızca adayın güncel danışmanı onun için bir Employment kaydı açabilir.
public sealed class CreateEmploymentCommandHandler(
    IEmploymentRepository employmentRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICandidateModuleContract candidateModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(EmploymentModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateEmploymentCommand, Result<CreateEmploymentResponse>>
{
    public async Task<Result<CreateEmploymentResponse>> Handle(CreateEmploymentCommand request, CancellationToken cancellationToken)
    {
        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (callerAdvisorId is null)
        {
            return Result.Failure<CreateEmploymentResponse>(
                Error.Forbidden("Employment.NotACareerAdvisor", "Only an active career advisor may create an employment record."));
        }

        var candidateAdvisorId = await candidateModuleContract.GetCareerAdvisorIdForCandidateAsync(
            request.CandidateCvId, cancellationToken);

        if (candidateAdvisorId != callerAdvisorId)
        {
            return Result.Failure<CreateEmploymentResponse>(
                Error.Forbidden("Employment.NotOwnCandidate", "You may only record employment for your own candidates."));
        }

        var employment = Domain.Employment.Create(
            request.CandidateCvId, request.CompanyId, request.PositionId, request.InterviewId,
            request.StartDateUtc, callerAdvisorId.Value, DateTime.UtcNow);

        employmentRepository.Add(employment);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateEmploymentResponse(employment.Id));
    }
}
