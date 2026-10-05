using FluentValidation;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Features.AddFormSubmissionNote;

public sealed class AddFormSubmissionNoteCommandValidator : AbstractValidator<AddFormSubmissionNoteCommand>
{
    public AddFormSubmissionNoteCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.Text).NotEmpty().MaximumLength(FormSubmissionInternalNote.MaxTextLength);
    }
}
