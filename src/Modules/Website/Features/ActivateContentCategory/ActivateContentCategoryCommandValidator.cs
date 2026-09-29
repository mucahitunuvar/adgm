using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.ActivateContentCategory;

public sealed class ActivateContentCategoryCommandValidator : AbstractValidator<ActivateContentCategoryCommand>
{
    public ActivateContentCategoryCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
