using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Contracts.Notification;
using GenclikMerkezi.Modules.Support.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Support.Features.CloseSupportTicket;

public sealed class CloseSupportTicketCommandHandler(
    ISupportTicketRepository supportTicketRepository,
    IIdentityService identityService,
    INotificationModuleContract notificationModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(SupportModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CloseSupportTicketCommand, Result>
{
    public async Task<Result> Handle(CloseSupportTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await supportTicketRepository.GetByIdAsync(request.SupportTicketId, cancellationToken);

        if (ticket is null)
        {
            return Result.Failure(Error.NotFound("SupportTicket.NotFound", "The specified support ticket could not be found."));
        }

        var userId = currentUserContext.UserId!.Value;

        if (ticket.AssignedToUserId != userId)
        {
            return Result.Failure(
                Error.Forbidden("SupportTicket.NotAssignee", "Only the ticket's current assignee may close it."));
        }

        var closeResult = ticket.Close(userId, request.Reason, DateTime.UtcNow);

        if (closeResult.IsFailure)
        {
            return closeResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var openerProfile = await identityService.GetUserProfileAsync(ticket.OpenedByUserId, cancellationToken);

        if (openerProfile is not null)
        {
            await notificationModuleContract.SendAsync(
                openerProfile.UserId,
                openerProfile.Email,
                "Destek Talebiniz Kapatıldı",
                $"\"{ticket.Subject}\" konulu destek talebiniz kapatıldı.",
                cancellationToken);
        }

        return Result.Success();
    }
}
