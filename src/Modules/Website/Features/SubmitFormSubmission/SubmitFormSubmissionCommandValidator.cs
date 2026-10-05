using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.SubmitFormSubmission;

public sealed class SubmitFormSubmissionCommandValidator : AbstractValidator<SubmitFormSubmissionCommand>
{
    public SubmitFormSubmissionCommandValidator()
    {
        RuleFor(c => c.FormKey).NotEmpty();
        RuleFor(c => c.Answers).NotNull();
        RuleFor(c => c.Files).NotNull();
        RuleFor(c => c.ExplicitConsents).NotNull();
    }
}
