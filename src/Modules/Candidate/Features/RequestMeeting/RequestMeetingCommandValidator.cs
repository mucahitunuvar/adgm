using FluentValidation;

namespace GenclikMerkezi.Modules.Candidate.Features.RequestMeeting;

public sealed class RequestMeetingCommandValidator : AbstractValidator<RequestMeetingCommand>
{
    public RequestMeetingCommandValidator()
    {
        RuleFor(c => c.CandidateCvId).NotEmpty();
    }
}
