using FluentValidation;

namespace GenclikMerkezi.Modules.Employment.Features.AddEmploymentNote;

public sealed class AddEmploymentNoteCommandValidator : AbstractValidator<AddEmploymentNoteCommand>
{
    public AddEmploymentNoteCommandValidator()
    {
        RuleFor(c => c.EmploymentId).NotEmpty();
        RuleFor(c => c.Content).NotEmpty().MaximumLength(4000);
    }
}
