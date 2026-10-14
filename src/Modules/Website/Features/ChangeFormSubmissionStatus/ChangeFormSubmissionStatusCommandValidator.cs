using FluentValidation;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Parsing;

namespace GenclikMerkezi.Modules.Website.Features.ChangeFormSubmissionStatus;

public sealed class ChangeFormSubmissionStatusCommandValidator : AbstractValidator<ChangeFormSubmissionStatusCommand>
{
    public ChangeFormSubmissionStatusCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.Status)
            .NotEmpty()
            .Must(status => EnumParsing.TryParseDefined<FormSubmissionStatus>(status, out _, ignoreCase: false))
            .WithMessage("Status must be a known form submission status.");
    }
}
