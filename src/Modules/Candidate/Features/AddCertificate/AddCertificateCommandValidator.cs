using FluentValidation;

namespace GenclikMerkezi.Modules.Candidate.Features.AddCertificate;

public sealed class AddCertificateCommandValidator : AbstractValidator<AddCertificateCommand>
{
    public AddCertificateCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(300);
        RuleFor(c => c.IssuingInstitution).NotEmpty().MaximumLength(300);
        RuleFor(c => c.Description).MaximumLength(2000).When(c => c.Description is not null);
    }
}
