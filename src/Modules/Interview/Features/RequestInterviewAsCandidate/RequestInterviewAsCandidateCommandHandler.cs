using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.Interview.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Interview.Features.RequestInterviewAsCandidate;

public sealed class RequestInterviewAsCandidateCommandHandler(
    IInterviewRepository interviewRepository,
    ICandidateModuleContract candidateModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(InterviewModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<RequestInterviewAsCandidateCommand, Result<RequestInterviewAsCandidateResponse>>
{
    public async Task<Result<RequestInterviewAsCandidateResponse>> Handle(
        RequestInterviewAsCandidateCommand request, CancellationToken cancellationToken)
    {
        var candidate = await candidateModuleContract.GetCandidateCvByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (candidate is null)
        {
            return Result.Failure<RequestInterviewAsCandidateResponse>(
                Error.NotFound("CandidateCv.NotFound", "No candidate CV was found for the current user."));
        }

        if (candidate.CareerAdvisorId is null)
        {
            return Result.Failure<RequestInterviewAsCandidateResponse>(Error.Conflict(
                "Interview.NoOrganizingAdvisor", "An interview cannot be organized without an assigned career advisor."));
        }

        var interview = Domain.Interview.Create(
            candidate.Id, request.CompanyId, candidate.CareerAdvisorId.Value,
            Domain.InterviewRequestedByRole.Candidate, DateTime.UtcNow);

        interviewRepository.Add(interview);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new RequestInterviewAsCandidateResponse(interview.Id));
    }
}
