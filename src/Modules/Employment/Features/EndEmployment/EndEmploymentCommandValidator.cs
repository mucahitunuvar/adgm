using FluentValidation;

namespace GenclikMerkezi.Modules.Employment.Features.EndEmployment;

public sealed class EndEmploymentCommandValidator : AbstractValidator<EndEmploymentCommand>
{
    public EndEmploymentCommandValidator()
    {
        RuleFor(c => c.EmploymentId).NotEmpty();
        RuleFor(c => c.DepartureReason).NotEmpty().MaximumLength(1000);
        RuleFor(c => c.EndDateUtc).NotEmpty();
    }
}
