using FluentValidation;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CompleteDevelopmentPlan;

public sealed class CompleteDevelopmentPlanCommandValidator : AbstractValidator<CompleteDevelopmentPlanCommand>
{
    public CompleteDevelopmentPlanCommandValidator()
    {
        RuleFor(c => c.DevelopmentPlanId).NotEmpty();
    }
}
