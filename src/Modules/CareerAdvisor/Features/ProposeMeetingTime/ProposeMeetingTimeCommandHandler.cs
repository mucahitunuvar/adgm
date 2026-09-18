using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Contracts.Notification;
using GenclikMerkezi.Modules.CareerAdvisor.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.ProposeMeetingTime;

public sealed class ProposeMeetingTimeCommandHandler(
    ICurrentUserContext currentUserContext,
    ICareerAdvisorRepository careerAdvisorRepository,
    IMeetingRequestRepository meetingRequestRepository,
    IIdentityService identityService,
    INotificationModuleContract notificationModuleContract,
    [FromKeyedServices(CareerAdvisorModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ProposeMeetingTimeCommand, Result>
{
    public async Task<Result> Handle(ProposeMeetingTimeCommand request, CancellationToken cancellationToken)
    {
        var careerAdvisor = await careerAdvisorRepository.GetByUserIdAsync(currentUserContext.UserId!.Value, cancellationToken);

        if (careerAdvisor is null)
        {
            return Result.Failure(
                Error.Forbidden("MeetingRequest.NotACareerAdvisor", "Only a career advisor may propose a meeting time."));
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

        var proposeResult = meetingRequest.ProposeTime(request.ProposedDateTimeUtc);

        if (proposeResult.IsFailure)
        {
            return proposeResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Aday bilgisine CareerAdvisor'ın kendi verisinden ulaşılamıyor (ADR-022 §1) - Identity'nin
        // public contract'ı üzerinden çekiliyor, best-effort (ADR-022 §3/§6).
        var candidateProfile = await identityService.GetUserProfileAsync(meetingRequest.CandidateUserId, cancellationToken);

        if (candidateProfile is not null)
        {
            var message = $"Kariyer danışmanınız {request.ProposedDateTimeUtc:g} (UTC) için görüşme önerdi. Sistemden onaylayın.";

            await notificationModuleContract.SendAsync(
                candidateProfile.UserId, candidateProfile.Email, "Görüşme Tarihi Önerildi", message, cancellationToken);
        }

        return Result.Success();
    }
}
