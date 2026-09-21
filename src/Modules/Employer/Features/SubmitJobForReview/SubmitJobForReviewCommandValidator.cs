using FluentValidation;

namespace GenclikMerkezi.Modules.Employer.Features.SubmitJobForReview;

public sealed class SubmitJobForReviewCommandValidator : AbstractValidator<SubmitJobForReviewCommand>
{
    public SubmitJobForReviewCommandValidator()
    {
        RuleFor(c => c.JobId).NotEmpty();
    }
}
