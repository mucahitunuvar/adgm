using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Contracts.Notification;
using GenclikMerkezi.Modules.CareerAdvisor.Application.Abstractions;
using GenclikMerkezi.Modules.CareerAdvisor.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.AddCandidateNote;

public sealed class AddCandidateNoteCommandHandler(
    ICurrentUserContext currentUserContext,
    ICareerAdvisorRepository careerAdvisorRepository,
    ICandidateNoteRepository candidateNoteRepository,
    IIdentityService identityService,
    INotificationModuleContract notificationModuleContract,
    [FromKeyedServices(CareerAdvisorModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<AddCandidateNoteCommand, Result<AddCandidateNoteResponse>>
{
    public async Task<Result<AddCandidateNoteResponse>> Handle(
        AddCandidateNoteCommand request, CancellationToken cancellationToken)
    {
        // Savunmacı: endpoint zaten "CareerAdvisor" rolüyle sınırlı, ama not, çağıranın KENDİ
        // CareerAdvisor kaydına stamp'lenmeli - istemciden gelen bir CareerAdvisorId'ye güvenilmez.
        var careerAdvisor = await careerAdvisorRepository.GetByUserIdAsync(currentUserContext.UserId!.Value, cancellationToken);

        if (careerAdvisor is null)
        {
            return Result.Failure<AddCandidateNoteResponse>(
                Error.Forbidden("CandidateNote.NotACareerAdvisor", "Only a career advisor may add a candidate note."));
        }

        var noteType = Enum.Parse<NoteType>(request.NoteType, ignoreCase: true);

        var note = CandidateNote.Create(request.CandidateCvId, careerAdvisor.Id, noteType, request.Content, DateTime.UtcNow);
        candidateNoteRepository.Add(note);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Not kaydı commit olduktan SONRA (ADR-022 §3) - best-effort, kritik veri tutarlılığı
        // gerektirmez.
        if (noteType == NoteType.IsGorusmesi)
        {
            var candidateProfile = await identityService.GetUserProfileAsync(request.CandidateUserId, cancellationToken);

            if (candidateProfile is not null)
            {
                var message =
                    $"Sayın {candidateProfile.FirstName} {candidateProfile.LastName}, kariyer danışmanınız " +
                    $"sizinle ilgili bir görüşme notu ekledi: \"{request.Content}\". Sistemden onaylayın.";

                await notificationModuleContract.SendAsync(
                    candidateProfile.UserId, candidateProfile.Email, "Görüşme Planlandı", message, cancellationToken);
            }
        }

        return Result.Success(new AddCandidateNoteResponse(note.Id));
    }
}
