using FluentValidation;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateSkillGap;

public sealed class CreateSkillGapCommandValidator : AbstractValidator<CreateSkillGapCommand>
{
    public CreateSkillGapCommandValidator()
    {
        RuleFor(c => c.CandidateCvId).NotEmpty();
        RuleFor(c => c.SkillId).NotEmpty();
        RuleFor(c => c.Notes).MaximumLength(1000);
    }
}
