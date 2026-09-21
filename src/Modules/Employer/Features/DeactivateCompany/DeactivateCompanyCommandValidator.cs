using FluentValidation;

namespace GenclikMerkezi.Modules.Employer.Features.DeactivateCompany;

public sealed class DeactivateCompanyCommandValidator : AbstractValidator<DeactivateCompanyCommand>
{
    public DeactivateCompanyCommandValidator()
    {
        RuleFor(c => c.CompanyId).NotEmpty();
    }
}
