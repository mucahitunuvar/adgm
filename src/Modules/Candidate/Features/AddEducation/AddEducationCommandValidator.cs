using FluentValidation;
using GenclikMerkezi.Modules.Candidate.Domain;

namespace GenclikMerkezi.Modules.Candidate.Features.AddEducation;

public sealed class AddEducationCommandValidator : AbstractValidator<AddEducationCommand>
{
    public AddEducationCommandValidator()
    {
        RuleFor(c => c.CompletionStatus)
            .Must(status => Enum.TryParse<EducationCompletionStatus>(status, ignoreCase: true, out _))
            .WithMessage("CompletionStatus must be one of: Continuing, Graduated, Dropped.");

        RuleFor(c => c.SchoolNameFreeText).MaximumLength(300).When(c => c.SchoolNameFreeText is not null);
        RuleFor(c => c.Description).MaximumLength(2000).When(c => c.Description is not null);
    }
}
