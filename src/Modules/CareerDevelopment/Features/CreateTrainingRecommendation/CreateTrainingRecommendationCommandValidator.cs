using FluentValidation;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateTrainingRecommendation;

public sealed class CreateTrainingRecommendationCommandValidator : AbstractValidator<CreateTrainingRecommendationCommand>
{
    public CreateTrainingRecommendationCommandValidator()
    {
        RuleFor(c => c.CandidateCvId).NotEmpty();
        RuleFor(c => c.TrainingId).NotEmpty();
        RuleFor(c => c.Notes).MaximumLength(1000);
    }
}
