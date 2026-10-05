using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UnarchiveFormSubmission;

public sealed class UnarchiveFormSubmissionCommandValidator : AbstractValidator<UnarchiveFormSubmissionCommand>
{
    public UnarchiveFormSubmissionCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
