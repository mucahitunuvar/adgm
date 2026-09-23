using FluentValidation;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateDevelopmentPlan;

public sealed class CreateDevelopmentPlanCommandValidator : AbstractValidator<CreateDevelopmentPlanCommand>
{
    public CreateDevelopmentPlanCommandValidator()
    {
        RuleFor(c => c.CandidateCvId).NotEmpty();
        RuleFor(c => c.Description).NotEmpty().MaximumLength(2000);
    }
}
