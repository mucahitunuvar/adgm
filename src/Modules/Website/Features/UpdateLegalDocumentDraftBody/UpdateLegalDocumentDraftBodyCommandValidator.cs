using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateLegalDocumentDraftBody;

public sealed class UpdateLegalDocumentDraftBodyCommandValidator : AbstractValidator<UpdateLegalDocumentDraftBodyCommand>
{
    public UpdateLegalDocumentDraftBodyCommandValidator()
    {
        RuleFor(c => c.LanguageCode).NotEmpty();
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
