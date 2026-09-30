using FluentValidation;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Features.ReplaceMenuItems;

public sealed class ReplaceMenuItemsCommandValidator : AbstractValidator<ReplaceMenuItemsCommand>
{
    public ReplaceMenuItemsCommandValidator()
    {
        RuleFor(c => c.Location).NotEmpty();
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.Items).NotNull();

        RuleForEach(c => c.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.TempId).NotEmpty();
            item.RuleFor(i => i.SortOrder).GreaterThanOrEqualTo(0);
            item.RuleFor(i => i.IconKey).MaximumLength(MenuItem.MaxIconKeyLength);
            item.RuleFor(i => i.Translations).NotNull().NotEmpty();

            item.RuleForEach(i => i.Translations).ChildRules(translation =>
            {
                translation.RuleFor(t => t.LanguageCode).NotEmpty();
                translation.RuleFor(t => t.Label).NotEmpty().MaximumLength(MenuItemTranslation.MaxLabelLength);
            });

            item.When(i => i.Link is not null, () =>
            {
                item.RuleFor(i => i.Link!.Kind).NotEmpty();
            });
        });
    }
}
