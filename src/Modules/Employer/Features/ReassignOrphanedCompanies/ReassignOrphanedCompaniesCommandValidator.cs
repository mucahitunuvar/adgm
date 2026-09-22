using FluentValidation;

namespace GenclikMerkezi.Modules.Employer.Features.ReassignOrphanedCompanies;

public sealed class ReassignOrphanedCompaniesCommandValidator : AbstractValidator<ReassignOrphanedCompaniesCommand>
{
    public ReassignOrphanedCompaniesCommandValidator()
    {
        RuleFor(c => c.CareerAdvisorId).NotEmpty();
    }
}
