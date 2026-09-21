using FluentValidation;

namespace GenclikMerkezi.Modules.Employer.Features.ClosePersonnelNeed;

public sealed class ClosePersonnelNeedCommandValidator : AbstractValidator<ClosePersonnelNeedCommand>
{
    public ClosePersonnelNeedCommandValidator()
    {
        RuleFor(c => c.PersonnelNeedId).NotEmpty();
        RuleFor(c => c.ClosedByAdvisorId).NotEmpty();
    }
}
