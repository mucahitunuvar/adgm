using FluentValidation;

namespace GenclikMerkezi.Modules.Employer.Features.AdminReassignCompany;

public sealed class AdminReassignCompanyCommandValidator : AbstractValidator<AdminReassignCompanyCommand>
{
    public AdminReassignCompanyCommandValidator()
    {
        RuleFor(c => c.CompanyId).NotEmpty();
    }
}
