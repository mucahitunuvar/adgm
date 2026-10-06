using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpsertEventSchedule;

public sealed class UpsertEventScheduleCommandValidator : AbstractValidator<UpsertEventScheduleCommand>
{
    public UpsertEventScheduleCommandValidator()
    {
        RuleFor(c => c.ContentItemId).NotEmpty();
        RuleFor(c => c.Format).NotEmpty();
        RuleFor(c => c.Translations).NotEmpty();
        RuleForEach(c => c.Translations).ChildRules(translation => translation.RuleFor(t => t.LanguageCode).NotEmpty());
    }
}
