using FluentValidation;
using GenclikMerkezi.Modules.Support.Domain;

namespace GenclikMerkezi.Modules.Support.Features.CreateSupportTicket;

public sealed class CreateSupportTicketCommandValidator : AbstractValidator<CreateSupportTicketCommand>
{
    public CreateSupportTicketCommandValidator()
    {
        RuleFor(c => c.Subject).NotEmpty().MaximumLength(200);

        RuleFor(c => c.Priority)
            .NotEmpty()
            .Must(value => Enum.TryParse<SupportTicketPriority>(value, ignoreCase: true, out _))
            .WithMessage("Priority must be one of: Dusuk, Orta, Kritik, Acil.");
    }
}
