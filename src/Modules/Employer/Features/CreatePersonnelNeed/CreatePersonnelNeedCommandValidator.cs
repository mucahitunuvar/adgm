using FluentValidation;

namespace GenclikMerkezi.Modules.Employer.Features.CreatePersonnelNeed;

// Şekil kontrolü - ReferenceData FK'ları (CreateJobCommandValidator ile aynı gerekçe) var olma
// kontrolüne tabi tutulmuyor.
public sealed class CreatePersonnelNeedCommandValidator : AbstractValidator<CreatePersonnelNeedCommand>
{
    public CreatePersonnelNeedCommandValidator()
    {
        RuleFor(c => c.EmploymentTypeId).NotEmpty();
        RuleFor(c => c.WorkLocationTypeId).NotEmpty();
        RuleFor(c => c.PositionId).NotEmpty();
        RuleFor(c => c.DepartmentId).NotEmpty();
        RuleFor(c => c.Quantity).GreaterThan(0);
        RuleFor(c => c.ProvinceId).NotEmpty();
        RuleFor(c => c.ExperienceLevelId).NotEmpty();
        RuleFor(c => c.DetailsText).MaximumLength(4000).When(c => c.DetailsText is not null);
    }
}
