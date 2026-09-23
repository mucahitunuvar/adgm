using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Contracts.Notification;
using GenclikMerkezi.Modules.Support.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Support.Features.TransferSupportTicket;

// Hedef kullanıcının kimliği doğrulanmaz (task tanımı, Kapsam Dışı): sahte bir id girilirse talep
// kimseye görünmez kalır - düşük risk, aşırı doğrulama eklenmiyor.
public sealed class TransferSupportTicketCommandHandler(
    ISupportTicketRepository supportTicketRepository,
    IIdentityService identityService,
    INotificationModuleContract notificationModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(SupportModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<TransferSupportTicketCommand, Result>
{
    public async Task<Result> Handle(TransferSupportTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await supportTicketRepository.GetByIdAsync(request.SupportTicketId, cancellationToken);

        if (ticket is null)
        {
            return Result.Failure(Error.NotFound("SupportTicket.NotFound", "The specified support ticket could not be found."));
        }

        if (ticket.AssignedToUserId != currentUserContext.UserId)
        {
            return Result.Failure(
                Error.Forbidden("SupportTicket.NotAssignee", "Only the ticket's current assignee may transfer it."));
        }

        var transferResult = ticket.Transfer(request.NewAssigneeUserId);

        if (transferResult.IsFailure)
        {
            return transferResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var newAssigneeProfile = await identityService.GetUserProfileAsync(request.NewAssigneeUserId, cancellationToken);

        if (newAssigneeProfile is not null)
        {
            await notificationModuleContract.SendAsync(
                newAssigneeProfile.UserId,
                newAssigneeProfile.Email,
                "Destek Talebi Size Devredildi",
                $"\"{ticket.Subject}\" konulu destek talebi size devredildi.",
                cancellationToken);
        }

        return Result.Success();
    }
}
