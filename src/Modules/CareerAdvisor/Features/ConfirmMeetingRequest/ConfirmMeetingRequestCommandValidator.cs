using FluentValidation;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.ConfirmMeetingRequest;

public sealed class ConfirmMeetingRequestCommandValidator : AbstractValidator<ConfirmMeetingRequestCommand>
{
    public ConfirmMeetingRequestCommandValidator()
    {
        RuleFor(c => c.MeetingRequestId).NotEmpty();
        RuleFor(c => c.CandidateUserId).NotEmpty();
    }
}
