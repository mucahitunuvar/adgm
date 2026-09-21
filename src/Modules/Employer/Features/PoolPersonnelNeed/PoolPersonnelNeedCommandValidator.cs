using FluentValidation;

namespace GenclikMerkezi.Modules.Employer.Features.PoolPersonnelNeed;

public sealed class PoolPersonnelNeedCommandValidator : AbstractValidator<PoolPersonnelNeedCommand>
{
    public PoolPersonnelNeedCommandValidator()
    {
        RuleFor(c => c.PersonnelNeedId).NotEmpty();
    }
}
