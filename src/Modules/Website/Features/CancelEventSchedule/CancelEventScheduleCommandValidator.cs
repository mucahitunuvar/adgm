using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.CancelEventSchedule;

public sealed class CancelEventScheduleCommandValidator : AbstractValidator<CancelEventScheduleCommand>
{
    public CancelEventScheduleCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
