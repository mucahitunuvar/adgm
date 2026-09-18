using FluentValidation;
using GenclikMerkezi.Modules.CareerAdvisor.Domain;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.AddCandidateNote;

public sealed class AddCandidateNoteCommandValidator : AbstractValidator<AddCandidateNoteCommand>
{
    public AddCandidateNoteCommandValidator()
    {
        RuleFor(c => c.CandidateCvId).NotEmpty();
        RuleFor(c => c.CandidateUserId).NotEmpty();

        RuleFor(c => c.NoteType)
            .NotEmpty()
            .Must(value => Enum.TryParse<NoteType>(value, ignoreCase: true, out _))
            .WithMessage("NoteType must be one of: Genel, IsGorusmesi.");

        RuleFor(c => c.Content).NotEmpty().MaximumLength(4000);
    }
}
