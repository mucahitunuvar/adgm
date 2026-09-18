using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Contracts.Notification;
using GenclikMerkezi.Modules.CareerAdvisor.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.RejectMeetingRequest;

public sealed class RejectMeetingRequestCommandHandler(
    ICurrentUserContext currentUserContext,
    ICareerAdvisorRepository careerAdvisorRepository,
    IMeetingRequestRepository meetingRequestRepository,
    IIdentityService identityService,
    INotificationModuleContract notificationModuleContract,
    [FromKeyedServices(CareerAdvisorModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<RejectMeetingRequestCommand, Result>
{
    public async Task<Result> Handle(RejectMeetingRequestCommand request, CancellationToken cancellationToken)
    {
        var careerAdvisor = await careerAdvisorRepository.GetByUserIdAsync(currentUserContext.UserId!.Value, cancellationToken);

        if (careerAdvisor is null)
        {
            return Result.Failure(
                Error.Forbidden("MeetingRequest.NotACareerAdvisor", "Only a career advisor may reject a meeting request."));
        }

        var meetingRequest = await meetingRequestRepository.GetByIdAsync(request.MeetingRequestId, cancellationToken);

        if (meetingRequest is null)
        {
            return Result.Failure(
                Error.NotFound("MeetingRequest.NotFound", "The specified meeting request could not be found."));
        }

        if (meetingRequest.CareerAdvisorId != careerAdvisor.Id)
        {
            return Result.Failure(
                Error.Forbidden("MeetingRequest.NotOwner", "You may only act on your own meeting requests."));
        }

        var rejectResult = meetingRequest.Reject();

        if (rejectResult.IsFailure)
        {
            return rejectResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var candidateProfile = await identityService.GetUserProfileAsync(meetingRequest.CandidateUserId, cancellationToken);

        if (candidateProfile is not null)
        {
            await notificationModuleContract.SendAsync(
                candidateProfile.UserId,
                candidateProfile.Email,
                "Görüşme Talebi Reddedildi",
                "Görüşme talebiniz danışmanınız tarafından reddedildi.",
                cancellationToken);
        }

        return Result.Success();
    }
}
