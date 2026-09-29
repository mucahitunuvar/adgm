using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemCategories;

public sealed class SetContentItemCategoriesCommandValidator : AbstractValidator<SetContentItemCategoriesCommand>
{
    public SetContentItemCategoriesCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.CategoryIds).NotNull();
    }
}
