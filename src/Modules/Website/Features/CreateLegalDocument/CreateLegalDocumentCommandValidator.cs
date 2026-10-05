using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.CreateLegalDocument;

public sealed class CreateLegalDocumentCommandValidator : AbstractValidator<CreateLegalDocumentCommand>
{
    public CreateLegalDocumentCommandValidator()
    {
        RuleFor(c => c.Key).NotEmpty();
        RuleFor(c => c.Kind).NotEmpty();
        RuleFor(c => c.DefaultLanguageTitle).NotEmpty();
    }
}
