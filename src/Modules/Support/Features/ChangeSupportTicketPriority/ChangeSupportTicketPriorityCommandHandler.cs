using GenclikMerkezi.Modules.Support.Application.Abstractions;
using GenclikMerkezi.Modules.Support.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Support.Features.ChangeSupportTicketPriority;

public sealed class ChangeSupportTicketPriorityCommandHandler(
    ISupportTicketRepository supportTicketRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(SupportModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeSupportTicketPriorityCommand, Result>
{
    public async Task<Result> Handle(ChangeSupportTicketPriorityCommand request, CancellationToken cancellationToken)
    {
        var ticket = await supportTicketRepository.GetByIdAsync(request.SupportTicketId, cancellationToken);

        if (ticket is null)
        {
            return Result.Failure(Error.NotFound("SupportTicket.NotFound", "The specified support ticket could not be found."));
        }

        if (ticket.AssignedToUserId != currentUserContext.UserId)
        {
            return Result.Failure(
                Error.Forbidden("SupportTicket.NotAssignee", "Only the ticket's current assignee may change its priority."));
        }

        var priority = Enum.Parse<SupportTicketPriority>(request.Priority, ignoreCase: true);

        var changeResult = ticket.ChangePriority(priority);

        if (changeResult.IsFailure)
        {
            return changeResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
