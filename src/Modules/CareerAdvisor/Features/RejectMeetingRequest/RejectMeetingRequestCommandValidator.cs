using FluentValidation;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.RejectMeetingRequest;

public sealed class RejectMeetingRequestCommandValidator : AbstractValidator<RejectMeetingRequestCommand>
{
    public RejectMeetingRequestCommandValidator()
    {
        RuleFor(c => c.MeetingRequestId).NotEmpty();
    }
}
