using FluentValidation;

namespace GenclikMerkezi.Modules.Support.Features.TransferSupportTicket;

public sealed class TransferSupportTicketCommandValidator : AbstractValidator<TransferSupportTicketCommand>
{
    public TransferSupportTicketCommandValidator()
    {
        RuleFor(c => c.SupportTicketId).NotEmpty();
        RuleFor(c => c.NewAssigneeUserId).NotEmpty();
    }
}
