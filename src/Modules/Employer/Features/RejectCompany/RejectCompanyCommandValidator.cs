using FluentValidation;

namespace GenclikMerkezi.Modules.Employer.Features.RejectCompany;

public sealed class RejectCompanyCommandValidator : AbstractValidator<RejectCompanyCommand>
{
    public RejectCompanyCommandValidator()
    {
        RuleFor(c => c.CompanyId).NotEmpty();
        RuleFor(c => c.Reason).NotEmpty().MaximumLength(1000);
    }
}
