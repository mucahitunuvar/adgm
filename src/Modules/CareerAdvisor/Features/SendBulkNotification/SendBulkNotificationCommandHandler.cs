using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Contracts.Notification;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.SendBulkNotification;

public sealed class SendBulkNotificationCommandHandler(
    IIdentityService identityService, INotificationModuleContract notificationModuleContract)
    : IRequestHandler<SendBulkNotificationCommand, Result>
{
    public async Task<Result> Handle(SendBulkNotificationCommand request, CancellationToken cancellationToken)
    {
        var recipients = new List<NotificationRecipient>();

        foreach (var candidateUserId in request.CandidateUserIds)
        {
            var profile = await identityService.GetUserProfileAsync(candidateUserId, cancellationToken);

            // Best-effort: profili çözülemeyen bir alıcı sessizce atlanır, tüm gönderimi
            // engellemez (ADR-022 §3/§6).
            if (profile is not null)
            {
                recipients.Add(new NotificationRecipient(profile.UserId, profile.Email));
            }
        }

        await notificationModuleContract.SendBulkAsync(recipients, request.Subject, request.Message, cancellationToken);

        return Result.Success();
    }
}
