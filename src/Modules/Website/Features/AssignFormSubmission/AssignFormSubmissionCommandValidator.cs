using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.AssignFormSubmission;

public sealed class AssignFormSubmissionCommandValidator : AbstractValidator<AssignFormSubmissionCommand>
{
    public AssignFormSubmissionCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
