using FluentValidation;

namespace GenclikMerkezi.Modules.Employer.Features.CreateJob;

// Şekil kontrolü - ReferenceData FK'ları (RegisterEmployerCommandValidator ile aynı gerekçe) var
// olma kontrolüne tabi tutulmuyor.
public sealed class CreateJobCommandValidator : AbstractValidator<CreateJobCommand>
{
    public CreateJobCommandValidator()
    {
        RuleFor(c => c.Title).NotEmpty().MaximumLength(200);
        RuleFor(c => c.EmploymentTypeId).NotEmpty();
        RuleFor(c => c.WorkLocationTypeId).NotEmpty();
        RuleFor(c => c.PositionId).NotEmpty();
        RuleFor(c => c.DepartmentId).NotEmpty();
        RuleFor(c => c.ProvinceId).NotEmpty();
        RuleFor(c => c.DescriptionHtml).MaximumLength(4000).When(c => c.DescriptionHtml is not null);
        RuleFor(c => c.ExperienceLevelId).NotEmpty();

        RuleForEach(c => c.LanguageRequirements).ChildRules(requirement =>
        {
            requirement.RuleFor(r => r.LanguageId).NotEmpty();
            requirement.RuleFor(r => r.LanguageLevelId).NotEmpty();
        });
    }
}
