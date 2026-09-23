using FluentValidation;

namespace GenclikMerkezi.Modules.Support.Features.CloseSupportTicket;

public sealed class CloseSupportTicketCommandValidator : AbstractValidator<CloseSupportTicketCommand>
{
    public CloseSupportTicketCommandValidator()
    {
        RuleFor(c => c.SupportTicketId).NotEmpty();
        RuleFor(c => c.Reason).MaximumLength(1000).When(c => c.Reason is not null);
    }
}
