using FluentValidation;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateAdvisorRecommendation;

public sealed class CreateAdvisorRecommendationCommandValidator : AbstractValidator<CreateAdvisorRecommendationCommand>
{
    public CreateAdvisorRecommendationCommandValidator()
    {
        RuleFor(c => c.CandidateCvId).NotEmpty();
        RuleFor(c => c.Content).NotEmpty().MaximumLength(4000);
    }
}
