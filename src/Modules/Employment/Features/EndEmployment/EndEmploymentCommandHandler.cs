using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.Employment.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employment.Features.EndEmployment;

// Yetki, CreateEmploymentCommandHandler'daki kontrolün aynısı - ancak Employment.CreatedByAdvisorId'ye
// değil, ICandidateModuleContract'tan TAZE çözülen danışmana göre: danışman, kayıt oluşturulduğundan
// beri değişmiş olabilir (deaktivasyon/yeniden atama - ADR-022 §3), yalnızca adayın GÜNCEL danışmanı
// bu işlemi yapabilmeli.
public sealed class EndEmploymentCommandHandler(
    IEmploymentRepository employmentRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICandidateModuleContract candidateModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(EmploymentModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<EndEmploymentCommand, Result>
{
    public async Task<Result> Handle(EndEmploymentCommand request, CancellationToken cancellationToken)
    {
        var employment = await employmentRepository.GetByIdAsync(request.EmploymentId, cancellationToken);

        if (employment is null)
        {
            return Result.Failure(Error.NotFound("Employment.NotFound", "The specified employment record could not be found."));
        }

        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        var currentAdvisorId = await candidateModuleContract.GetCareerAdvisorIdForCandidateAsync(
            employment.CandidateCvId, cancellationToken);

        if (callerAdvisorId is null || callerAdvisorId != currentAdvisorId)
        {
            return Result.Failure(Error.Forbidden(
                "Employment.NotCurrentAdvisor", "Only the candidate's current career advisor may end this employment."));
        }

        var endResult = employment.EndEmployment(request.DepartureReason, request.EndDateUtc);

        if (endResult.IsFailure)
        {
            return endResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
