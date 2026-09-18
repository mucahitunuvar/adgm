using FluentValidation;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;

public sealed class CreateCareerAdvisorCommandValidator : AbstractValidator<CreateCareerAdvisorCommand>
{
    public CreateCareerAdvisorCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(c => c.Password).NotEmpty().MinimumLength(8);
        RuleFor(c => c.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.LastName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.PhoneNumber).MaximumLength(20).When(c => c.PhoneNumber is not null);
    }
}
