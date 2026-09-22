using FluentValidation;

namespace GenclikMerkezi.Modules.Interview.Features.RecordInterviewResult;

public sealed class RecordInterviewResultCommandValidator : AbstractValidator<RecordInterviewResultCommand>
{
    public RecordInterviewResultCommandValidator()
    {
        RuleFor(c => c.InterviewId).NotEmpty();
        RuleFor(c => c.Outcome).IsInEnum();
        RuleFor(c => c.ResultNotes).MaximumLength(1000).When(c => c.ResultNotes is not null);
    }
}
