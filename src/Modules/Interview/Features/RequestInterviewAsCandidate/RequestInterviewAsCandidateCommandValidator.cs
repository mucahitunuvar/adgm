using FluentValidation;

namespace GenclikMerkezi.Modules.Interview.Features.RequestInterviewAsCandidate;

public sealed class RequestInterviewAsCandidateCommandValidator : AbstractValidator<RequestInterviewAsCandidateCommand>
{
    public RequestInterviewAsCandidateCommandValidator()
    {
        RuleFor(c => c.CompanyId).NotEmpty();
    }
}
