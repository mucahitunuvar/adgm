using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.Employment.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employment.Features.GetEmploymentsForCandidate;

// Tek query, rol-bazlı ownership dalı (iki ayrı endpoint yerine): çağıran ya adayın kendisi (kendi
// CandidateCvId'si eşleşiyorsa, GetMyInterviewsAsCandidateQueryHandler deseni) ya da adayın GÜNCEL
// danışmanı (EndEmploymentCommandHandler'daki taze-danışman kontrolü) olabilir. İkisi de değilse
// Forbidden - endpoint bu yüzden spesifik bir rol talep etmez, sahiplik burada çözülür.
public sealed class GetEmploymentsForCandidateQueryHandler(
    IEmploymentRepository employmentRepository,
    ICandidateModuleContract candidateModuleContract,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<GetEmploymentsForCandidateQuery, Result<IReadOnlyList<EmploymentResponse>>>
{
    public async Task<Result<IReadOnlyList<EmploymentResponse>>> Handle(
        GetEmploymentsForCandidateQuery request, CancellationToken cancellationToken)
    {
        var isAuthorized = await IsCallerTheCandidateAsync(request.CandidateCvId, cancellationToken)
            || await IsCallerTheCurrentAdvisorAsync(request.CandidateCvId, cancellationToken);

        if (!isAuthorized)
        {
            return Result.Failure<IReadOnlyList<EmploymentResponse>>(Error.Forbidden(
                "Employment.NotAuthorized", "You may only view your own employment history or your own candidates'."));
        }

        var employments = await employmentRepository.GetByCandidateCvIdAsync(request.CandidateCvId, cancellationToken);

        IReadOnlyList<EmploymentResponse> response = employments.Select(EmploymentResponse.FromDomain).ToList();

        return Result.Success(response);
    }

    private async Task<bool> IsCallerTheCandidateAsync(Guid candidateCvId, CancellationToken cancellationToken)
    {
        var candidate = await candidateModuleContract.GetCandidateCvByUserIdAsync(currentUserContext.UserId!.Value, cancellationToken);

        return candidate is not null && candidate.Id == candidateCvId;
    }

    private async Task<bool> IsCallerTheCurrentAdvisorAsync(Guid candidateCvId, CancellationToken cancellationToken)
    {
        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (callerAdvisorId is null)
        {
            return false;
        }

        var currentAdvisorId = await candidateModuleContract.GetCareerAdvisorIdForCandidateAsync(candidateCvId, cancellationToken);

        return currentAdvisorId == callerAdvisorId;
    }
}
