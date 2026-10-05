using FluentValidation;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Features.ChangeFormSubmissionStatus;

public sealed class ChangeFormSubmissionStatusCommandValidator : AbstractValidator<ChangeFormSubmissionStatusCommand>
{
    public ChangeFormSubmissionStatusCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.Status)
            .NotEmpty()
            .Must(status => Enum.TryParse<FormSubmissionStatus>(status, out _))
            .WithMessage("Status must be a known form submission status.");
    }
}
