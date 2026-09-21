using FluentValidation;

namespace GenclikMerkezi.Modules.Employer.Features.SubmitPersonnelNeed;

public sealed class SubmitPersonnelNeedCommandValidator : AbstractValidator<SubmitPersonnelNeedCommand>
{
    public SubmitPersonnelNeedCommandValidator()
    {
        RuleFor(c => c.PersonnelNeedId).NotEmpty();
    }
}
