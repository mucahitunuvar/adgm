using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.SendBulkCandidateNotification;

// Görev 8: yetkilendirme/alıcı-listesi çözümleme burada yapılır (CandidateCv.CareerAdvisorId
// verisine yalnızca Candidate modülü sahip), gerçek gönderim ICareerAdvisorModuleContract
// üzerinden CareerAdvisor modülüne (ve oradan Notification'a) delege edilir - Görev 5'teki
// RequestMeetingCommand ile aynı Candidate → CareerAdvisor yönü.
public sealed class SendBulkCandidateNotificationCommandHandler(
    ICandidateCvRepository candidateCvRepository,
    ICurrentUserContext currentUserContext,
    ICareerAdvisorModuleContract careerAdvisorModuleContract)
    : IRequestHandler<SendBulkCandidateNotificationCommand, Result<SendBulkCandidateNotificationResponse>>
{
    public async Task<Result<SendBulkCandidateNotificationResponse>> Handle(
        SendBulkCandidateNotificationCommand request, CancellationToken cancellationToken)
    {
        var advisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (advisorId is null)
        {
            return Result.Failure<SendBulkCandidateNotificationResponse>(
                Error.Forbidden("CareerAdvisor.NotAnAdvisor", "Only an active career advisor may send bulk notifications."));
        }

        var ownCandidates = await candidateCvRepository.GetByCareerAdvisorIdAsync(advisorId.Value, cancellationToken);

        // Alt küme istenmişse, yalnızca gerçekten bu danışmana ait olanlar hedeflenir - sahip
        // olunmayan/var olmayan id'ler sessizce elenir (kaynağın varlığını ifşa etmemek için).
        if (request.CandidateCvIds is { Count: > 0 })
        {
            var requestedIds = request.CandidateCvIds.ToHashSet();
            ownCandidates = ownCandidates.Where(c => requestedIds.Contains(c.Id)).ToList();
        }

        if (ownCandidates.Count == 0)
        {
            return Result.Failure<SendBulkCandidateNotificationResponse>(
                Error.Conflict("CandidateCv.NoRecipients", "Bildirim gönderilecek aday bulunamadı."));
        }

        var candidateUserIds = ownCandidates.Select(c => c.UserId).ToList();

        await careerAdvisorModuleContract.SendBulkNotificationAsync(
            candidateUserIds, request.Subject, request.Message, cancellationToken);

        return Result.Success(new SendBulkCandidateNotificationResponse(candidateUserIds.Count));
    }
}
