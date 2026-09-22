using FluentValidation;

namespace GenclikMerkezi.Modules.Interview.Features.ScheduleInterview;

public sealed class ScheduleInterviewCommandValidator : AbstractValidator<ScheduleInterviewCommand>
{
    public ScheduleInterviewCommandValidator()
    {
        RuleFor(c => c.InterviewId).NotEmpty();
        RuleFor(c => c.ScheduledAtUtc).NotEmpty();
    }
}
