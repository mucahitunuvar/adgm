using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Contracts.Notification;
using GenclikMerkezi.Modules.Support.Application.Abstractions;
using GenclikMerkezi.Modules.Support.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Support.Features.AddSupportTicketMessage;

public sealed class AddSupportTicketMessageCommandHandler(
    ISupportTicketRepository supportTicketRepository,
    ISupportTicketMessageRepository supportTicketMessageRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    IIdentityService identityService,
    INotificationModuleContract notificationModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(SupportModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<AddSupportTicketMessageCommand, Result<AddSupportTicketMessageResponse>>
{
    public async Task<Result<AddSupportTicketMessageResponse>> Handle(
        AddSupportTicketMessageCommand request, CancellationToken cancellationToken)
    {
        var ticket = await supportTicketRepository.GetByIdAsync(request.SupportTicketId, cancellationToken);

        if (ticket is null)
        {
            return Result.Failure<AddSupportTicketMessageResponse>(
                Error.NotFound("SupportTicket.NotFound", "The specified support ticket could not be found."));
        }

        var userId = currentUserContext.UserId!.Value;
        var isOpener = ticket.OpenedByUserId == userId;
        var isAssignee = ticket.AssignedToUserId == userId;

        if (!isOpener && !isAssignee)
        {
            return Result.Failure<AddSupportTicketMessageResponse>(Error.Forbidden(
                "SupportTicket.NotParticipant", "Only the ticket's opener or assignee may post a message."));
        }

        if (ticket.Status == SupportTicketStatus.Kapandi)
        {
            return Result.Failure<AddSupportTicketMessageResponse>(
                Error.Conflict("SupportTicket.AlreadyClosed", "Cannot post a message to a closed ticket."));
        }

        // SenderRole, istemciden değil çağıranın gerçek kimliğinden çözülür (AGENTS.md §26).
        SupportTicketMessageSenderRole senderRole;

        if (isOpener)
        {
            senderRole = ticket.OpenedByRole == SupportTicketOpenerRole.Candidate
                ? SupportTicketMessageSenderRole.Candidate
                : SupportTicketMessageSenderRole.Employer;
        }
        else
        {
            var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(userId, cancellationToken);
            senderRole = callerAdvisorId is not null ? SupportTicketMessageSenderRole.CareerAdvisor : SupportTicketMessageSenderRole.Admin;
        }

        var message = SupportTicketMessage.Create(ticket.Id, userId, senderRole, request.Content, DateTime.UtcNow);
        supportTicketMessageRepository.Add(message);

        // Karşı tarafa bildirim: açan yeniden açıyorsa (Cevaplandi -> Acik) ya da atanan yanıtlıyorsa
        // (Acik -> Cevaplandi) durum değişir; açan zaten Acik durumdayken ek mesaj yazarsa durum
        // değişmez ama atanmışsa yine de bilgilendirilir (task tanımı: "danışmana yeni mesaj var").
        Guid? recipientUserId;

        if (isAssignee)
        {
            ticket.MarkAnswered(DateTime.UtcNow);
            recipientUserId = ticket.OpenedByUserId;
        }
        else if (ticket.Status == SupportTicketStatus.Cevaplandi)
        {
            ticket.ReopenByFollowUp(DateTime.UtcNow);
            recipientUserId = ticket.AssignedToUserId;
        }
        else
        {
            recipientUserId = ticket.AssignedToUserId;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (recipientUserId is not null)
        {
            var recipientProfile = await identityService.GetUserProfileAsync(recipientUserId.Value, cancellationToken);

            if (recipientProfile is not null)
            {
                await notificationModuleContract.SendAsync(
                    recipientProfile.UserId,
                    recipientProfile.Email,
                    "Destek Talebinde Yeni Mesaj",
                    $"\"{ticket.Subject}\" konulu destek talebinize yeni bir mesaj eklendi.",
                    cancellationToken);
            }
        }

        return Result.Success(new AddSupportTicketMessageResponse(message.Id));
    }
}
