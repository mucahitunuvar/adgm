using GenclikMerkezi.Contracts.Notification;
using GenclikMerkezi.Modules.CareerAdvisor.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.ConfirmMeetingRequest;

public sealed class ConfirmMeetingRequestCommandHandler(
    IMeetingRequestRepository meetingRequestRepository,
    ICareerAdvisorRepository careerAdvisorRepository,
    INotificationModuleContract notificationModuleContract,
    [FromKeyedServices(CareerAdvisorModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ConfirmMeetingRequestCommand, Result>
{
    public async Task<Result> Handle(ConfirmMeetingRequestCommand request, CancellationToken cancellationToken)
    {
        var meetingRequest = await meetingRequestRepository.GetByIdAsync(request.MeetingRequestId, cancellationToken);

        if (meetingRequest is null)
        {
            return Result.Failure(
                Error.NotFound("MeetingRequest.NotFound", "The specified meeting request could not be found."));
        }

        if (meetingRequest.CandidateUserId != request.CandidateUserId)
        {
            return Result.Failure(
                Error.Forbidden("MeetingRequest.NotOwner", "You may only confirm your own meeting request."));
        }

        var confirmResult = meetingRequest.Confirm(DateTime.UtcNow);

        if (confirmResult.IsFailure)
        {
            return confirmResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var careerAdvisor = await careerAdvisorRepository.GetByIdAsync(meetingRequest.CareerAdvisorId, cancellationToken);

        if (careerAdvisor is not null)
        {
            await notificationModuleContract.SendAsync(
                careerAdvisor.UserId,
                careerAdvisor.Email,
                "Görüşme Onaylandı",
                "Adayınız önerdiğiniz görüşme tarihini onayladı.",
                cancellationToken);
        }

        return Result.Success();
    }
}
