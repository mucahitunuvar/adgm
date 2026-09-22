using FluentValidation;

namespace GenclikMerkezi.Modules.Interview.Features.CancelInterview;

public sealed class CancelInterviewCommandValidator : AbstractValidator<CancelInterviewCommand>
{
    public CancelInterviewCommandValidator()
    {
        RuleFor(c => c.InterviewId).NotEmpty();
    }
}
