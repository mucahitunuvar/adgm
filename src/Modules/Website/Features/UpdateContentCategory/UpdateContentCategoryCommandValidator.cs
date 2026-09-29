using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentCategory;

public sealed class UpdateContentCategoryCommandValidator : AbstractValidator<UpdateContentCategoryCommand>
{
    public UpdateContentCategoryCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);
    }
}
