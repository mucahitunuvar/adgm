using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.PublishLegalDocumentDraft;

public sealed class PublishLegalDocumentDraftCommandValidator : AbstractValidator<PublishLegalDocumentDraftCommand>
{
    public PublishLegalDocumentDraftCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
