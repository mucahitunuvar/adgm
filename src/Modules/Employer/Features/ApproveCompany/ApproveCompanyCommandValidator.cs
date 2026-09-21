using FluentValidation;

namespace GenclikMerkezi.Modules.Employer.Features.ApproveCompany;

public sealed class ApproveCompanyCommandValidator : AbstractValidator<ApproveCompanyCommand>
{
    public ApproveCompanyCommandValidator()
    {
        RuleFor(c => c.CompanyId).NotEmpty();
    }
}
