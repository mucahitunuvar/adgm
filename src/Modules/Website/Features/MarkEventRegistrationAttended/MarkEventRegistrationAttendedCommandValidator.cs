using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.MarkEventRegistrationAttended;

public sealed class MarkEventRegistrationAttendedCommandValidator : AbstractValidator<MarkEventRegistrationAttendedCommand>
{
    public MarkEventRegistrationAttendedCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
