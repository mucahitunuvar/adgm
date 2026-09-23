using FluentValidation;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CancelDevelopmentPlan;

public sealed class CancelDevelopmentPlanCommandValidator : AbstractValidator<CancelDevelopmentPlanCommand>
{
    public CancelDevelopmentPlanCommandValidator()
    {
        RuleFor(c => c.DevelopmentPlanId).NotEmpty();
    }
}
