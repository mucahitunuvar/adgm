using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.Interview.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Interview.Features.CancelInterview;

// ScheduleInterviewCommandHandler'daki reviewer-doğrulama deseninin aynısı.
public sealed class CancelInterviewCommandHandler(
    IInterviewRepository interviewRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(InterviewModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CancelInterviewCommand, Result>
{
    public async Task<Result> Handle(CancelInterviewCommand request, CancellationToken cancellationToken)
    {
        var interview = await interviewRepository.GetByIdAsync(request.InterviewId, cancellationToken);

        if (interview is null)
        {
            return Result.Failure(Error.NotFound("Interview.NotFound", "The specified interview could not be found."));
        }

        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (callerAdvisorId is null || callerAdvisorId != interview.OrganizingAdvisorId)
        {
            return Result.Failure(Error.Forbidden(
                "Interview.NotOrganizingAdvisor", "Only the interview's organizing career advisor may cancel it."));
        }

        var cancelResult = interview.Cancel();

        if (cancelResult.IsFailure)
        {
            return cancelResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
