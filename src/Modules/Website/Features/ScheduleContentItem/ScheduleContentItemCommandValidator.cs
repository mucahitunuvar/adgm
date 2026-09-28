using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.ScheduleContentItem;

public sealed class ScheduleContentItemCommandValidator : AbstractValidator<ScheduleContentItemCommand>
{
    public ScheduleContentItemCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
