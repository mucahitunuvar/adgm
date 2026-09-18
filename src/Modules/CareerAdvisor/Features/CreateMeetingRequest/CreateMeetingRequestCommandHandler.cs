using GenclikMerkezi.Contracts.Notification;
using GenclikMerkezi.Modules.CareerAdvisor.Application.Abstractions;
using GenclikMerkezi.Modules.CareerAdvisor.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.CreateMeetingRequest;

public sealed class CreateMeetingRequestCommandHandler(
    ICareerAdvisorRepository careerAdvisorRepository,
    IMeetingRequestRepository meetingRequestRepository,
    INotificationModuleContract notificationModuleContract,
    [FromKeyedServices(CareerAdvisorModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateMeetingRequestCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateMeetingRequestCommand request, CancellationToken cancellationToken)
    {
        var careerAdvisor = await careerAdvisorRepository.GetByIdAsync(request.CareerAdvisorId, cancellationToken);

        if (careerAdvisor is null)
        {
            return Result.Failure<Guid>(
                Error.NotFound("CareerAdvisor.NotFound", "The specified career advisor could not be found."));
        }

        var meetingRequest = MeetingRequest.Create(
            request.CandidateCvId, request.CandidateUserId, request.CareerAdvisorId, DateTime.UtcNow);
        meetingRequestRepository.Add(meetingRequest);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Danışmanın kendi bilgisi zaten elimizde (cross-module çağrıya gerek yok) - not kaydı
        // commit olduktan SONRA (ADR-022 §3/§6), best-effort.
        await notificationModuleContract.SendAsync(
            careerAdvisor.UserId,
            careerAdvisor.Email,
            "Yeni Görüşme Talebi",
            "Bir adayınız sizden görüşme talep etti. Lütfen bir tarih önerin.",
            cancellationToken);

        return Result.Success(meetingRequest.Id);
    }
}
