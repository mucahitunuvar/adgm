using FluentValidation;

namespace GenclikMerkezi.Modules.Interview.Features.RequestInterviewAsEmployer;

public sealed class RequestInterviewAsEmployerCommandValidator : AbstractValidator<RequestInterviewAsEmployerCommand>
{
    public RequestInterviewAsEmployerCommandValidator()
    {
        RuleFor(c => c.CandidateCvId).NotEmpty();
    }
}
