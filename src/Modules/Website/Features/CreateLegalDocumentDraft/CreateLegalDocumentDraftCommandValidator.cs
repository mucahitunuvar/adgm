using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.CreateLegalDocumentDraft;

public sealed class CreateLegalDocumentDraftCommandValidator : AbstractValidator<CreateLegalDocumentDraftCommand>
{
    public CreateLegalDocumentDraftCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
