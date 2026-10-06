using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.ReactivateEventSchedule;

public sealed class ReactivateEventScheduleCommandValidator : AbstractValidator<ReactivateEventScheduleCommand>
{
    public ReactivateEventScheduleCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
