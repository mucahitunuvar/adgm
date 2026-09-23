using FluentValidation;

namespace GenclikMerkezi.Modules.Support.Features.AddSupportTicketMessage;

public sealed class AddSupportTicketMessageCommandValidator : AbstractValidator<AddSupportTicketMessageCommand>
{
    public AddSupportTicketMessageCommandValidator()
    {
        RuleFor(c => c.SupportTicketId).NotEmpty();
        RuleFor(c => c.Content).NotEmpty().MaximumLength(4000);
    }
}
