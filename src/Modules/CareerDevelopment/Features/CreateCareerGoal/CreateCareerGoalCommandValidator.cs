using FluentValidation;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateCareerGoal;

public sealed class CreateCareerGoalCommandValidator : AbstractValidator<CreateCareerGoalCommand>
{
    public CreateCareerGoalCommandValidator()
    {
        RuleFor(c => c.CandidateCvId).NotEmpty();
        RuleFor(c => c.Description).NotEmpty().MaximumLength(2000);
    }
}
