using FluentValidation;

namespace GenclikMerkezi.Modules.Candidate.Features.ConfirmMeeting;

public sealed class ConfirmMeetingCommandValidator : AbstractValidator<ConfirmMeetingCommand>
{
    public ConfirmMeetingCommandValidator()
    {
        RuleFor(c => c.CandidateCvId).NotEmpty();
        RuleFor(c => c.MeetingRequestId).NotEmpty();
    }
}
