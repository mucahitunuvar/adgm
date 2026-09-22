using FluentValidation;
using GenclikMerkezi.Modules.Interview.Domain;

namespace GenclikMerkezi.Modules.Interview.Features.RecordInterviewResult;

public sealed class RecordInterviewResultCommandValidator : AbstractValidator<RecordInterviewResultCommand>
{
    public RecordInterviewResultCommandValidator()
    {
        RuleFor(c => c.InterviewId).NotEmpty();

        RuleFor(c => c.Outcome)
            .NotEmpty()
            .Must(value => Enum.TryParse<InterviewResult>(value, ignoreCase: true, out _))
            .WithMessage("Outcome must be one of: Olumlu, Olumsuz, Beklemede, TekrarGorusmeGerekli.");

        RuleFor(c => c.ResultNotes).MaximumLength(1000).When(c => c.ResultNotes is not null);
    }
}
