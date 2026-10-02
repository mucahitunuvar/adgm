using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateImpactMetricTranslation;

public sealed class UpdateImpactMetricTranslationCommandValidator : AbstractValidator<UpdateImpactMetricTranslationCommand>
{
    public UpdateImpactMetricTranslationCommandValidator()
    {
        RuleFor(c => c.LanguageCode).NotEmpty();
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.Label).NotEmpty();
    }
}
