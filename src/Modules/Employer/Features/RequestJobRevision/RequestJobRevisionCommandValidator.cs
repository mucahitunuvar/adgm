using FluentValidation;

namespace GenclikMerkezi.Modules.Employer.Features.RequestJobRevision;

public sealed class RequestJobRevisionCommandValidator : AbstractValidator<RequestJobRevisionCommand>
{
    public RequestJobRevisionCommandValidator()
    {
        RuleFor(c => c.JobId).NotEmpty();
        RuleFor(c => c.Notes).NotEmpty().MaximumLength(1000);
    }
}
