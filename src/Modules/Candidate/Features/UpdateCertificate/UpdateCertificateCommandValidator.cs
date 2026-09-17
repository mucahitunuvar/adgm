using FluentValidation;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCertificate;

public sealed class UpdateCertificateCommandValidator : AbstractValidator<UpdateCertificateCommand>
{
    public UpdateCertificateCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(300);
        RuleFor(c => c.IssuingInstitution).NotEmpty().MaximumLength(300);
        RuleFor(c => c.Description).MaximumLength(2000).When(c => c.Description is not null);
    }
}
