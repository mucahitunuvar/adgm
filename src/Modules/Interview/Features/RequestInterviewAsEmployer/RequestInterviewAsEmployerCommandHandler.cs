using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Modules.Interview.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Interview.Features.RequestInterviewAsEmployer;

public sealed class RequestInterviewAsEmployerCommandHandler(
    IInterviewRepository interviewRepository,
    ICompanyModuleContract companyModuleContract,
    ICandidateModuleContract candidateModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(InterviewModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<RequestInterviewAsEmployerCommand, Result<RequestInterviewAsEmployerResponse>>
{
    public async Task<Result<RequestInterviewAsEmployerResponse>> Handle(
        RequestInterviewAsEmployerCommand request, CancellationToken cancellationToken)
    {
        var company = await companyModuleContract.GetCompanyByUserIdAsync(currentUserContext.UserId!.Value, cancellationToken);

        if (company is null)
        {
            return Result.Failure<RequestInterviewAsEmployerResponse>(
                Error.NotFound("Company.NotFound", "No company was found for the current user."));
        }

        var organizingAdvisorId = await candidateModuleContract.GetCareerAdvisorIdForCandidateAsync(
            request.CandidateCvId, cancellationToken);

        if (organizingAdvisorId is null)
        {
            return Result.Failure<RequestInterviewAsEmployerResponse>(Error.Conflict(
                "Interview.NoOrganizingAdvisor", "An interview cannot be organized without an assigned career advisor."));
        }

        var interview = Domain.Interview.Create(
            request.CandidateCvId, company.Id, organizingAdvisorId.Value,
            Domain.InterviewRequestedByRole.Employer, DateTime.UtcNow);

        interviewRepository.Add(interview);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new RequestInterviewAsEmployerResponse(interview.Id));
    }
}
