using FluentValidation;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.ProposeMeetingTime;

public sealed class ProposeMeetingTimeCommandValidator : AbstractValidator<ProposeMeetingTimeCommand>
{
    public ProposeMeetingTimeCommandValidator()
    {
        RuleFor(c => c.MeetingRequestId).NotEmpty();
        RuleFor(c => c.ProposedDateTimeUtc).NotEmpty();
    }
}
