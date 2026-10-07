using FluentValidation;

namespace GenclikMerkezi.Modules.Employer.Features.SetCompanyLogoVisibility;

public sealed class SetCompanyLogoVisibilityCommandValidator : AbstractValidator<SetCompanyLogoVisibilityCommand>
{
    public SetCompanyLogoVisibilityCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
