using FluentValidation;

namespace GenclikMerkezi.Modules.Employer.Features.UpdatePersonnelNeed;

public sealed class UpdatePersonnelNeedCommandValidator : AbstractValidator<UpdatePersonnelNeedCommand>
{
    public UpdatePersonnelNeedCommandValidator()
    {
        RuleFor(c => c.PersonnelNeedId).NotEmpty();
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
