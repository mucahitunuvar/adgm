using FluentValidation;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.CreateMeetingRequest;

public sealed class CreateMeetingRequestCommandValidator : AbstractValidator<CreateMeetingRequestCommand>
{
    public CreateMeetingRequestCommandValidator()
    {
        RuleFor(c => c.CandidateCvId).NotEmpty();
        RuleFor(c => c.CandidateUserId).NotEmpty();
        RuleFor(c => c.CareerAdvisorId).NotEmpty();
    }
}
