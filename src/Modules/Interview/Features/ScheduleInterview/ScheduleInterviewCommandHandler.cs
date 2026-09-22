using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.Interview.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Interview.Features.ScheduleInterview;

// ApproveJobCommandHandler'daki reviewer-doğrulama deseninin sadeleştirilmiş hali: Interview,
// OrganizingAdvisorId'yi doğrudan kendi üzerinde tuttuğu için (Job'un Company üzerinden dolaylı
// CareerAdvisorId'sinin aksine) yalnızca tek bir contract çağrısı yeterli.
public sealed class ScheduleInterviewCommandHandler(
    IInterviewRepository interviewRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(InterviewModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ScheduleInterviewCommand, Result>
{
    public async Task<Result> Handle(ScheduleInterviewCommand request, CancellationToken cancellationToken)
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
                "Interview.NotOrganizingAdvisor", "Only the interview's organizing career advisor may schedule it."));
        }

        var scheduleResult = interview.Schedule(request.ScheduledAtUtc);

        if (scheduleResult.IsFailure)
        {
            return scheduleResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
