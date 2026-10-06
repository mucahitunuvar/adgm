using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.CreateEventRegistration;

public sealed class CreateEventRegistrationCommandValidator : AbstractValidator<CreateEventRegistrationCommand>
{
    public CreateEventRegistrationCommandValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty();
        RuleFor(c => c.LastName).NotEmpty();
        RuleFor(c => c.Email).NotEmpty();
    }
}
