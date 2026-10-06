using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.MarkEventRegistrationNoShow;

public sealed class MarkEventRegistrationNoShowCommandValidator : AbstractValidator<MarkEventRegistrationNoShowCommand>
{
    public MarkEventRegistrationNoShowCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
