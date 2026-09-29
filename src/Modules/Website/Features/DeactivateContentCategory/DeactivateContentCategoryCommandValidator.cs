using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateContentCategory;

public sealed class DeactivateContentCategoryCommandValidator : AbstractValidator<DeactivateContentCategoryCommand>
{
    public DeactivateContentCategoryCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
