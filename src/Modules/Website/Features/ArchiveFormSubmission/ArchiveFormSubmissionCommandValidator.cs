using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.ArchiveFormSubmission;

public sealed class ArchiveFormSubmissionCommandValidator : AbstractValidator<ArchiveFormSubmissionCommand>
{
    public ArchiveFormSubmissionCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
