using FluentValidation;
using GenclikMerkezi.Modules.Support.Domain;

namespace GenclikMerkezi.Modules.Support.Features.ChangeSupportTicketPriority;

public sealed class ChangeSupportTicketPriorityCommandValidator : AbstractValidator<ChangeSupportTicketPriorityCommand>
{
    public ChangeSupportTicketPriorityCommandValidator()
    {
        RuleFor(c => c.SupportTicketId).NotEmpty();

        RuleFor(c => c.Priority)
            .NotEmpty()
            .Must(value => Enum.TryParse<SupportTicketPriority>(value, ignoreCase: true, out _))
            .WithMessage("Priority must be one of: Dusuk, Orta, Kritik, Acil.");
    }
}
