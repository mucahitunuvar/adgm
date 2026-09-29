using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.CreateContentCategory;

public sealed class CreateContentCategoryCommandValidator : AbstractValidator<CreateContentCategoryCommand>
{
    public CreateContentCategoryCommandValidator()
    {
        RuleFor(c => c.ContentTypeId).NotEmpty();
        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);
        RuleFor(c => c.DefaultLanguageName).NotEmpty();
        RuleFor(c => c.Seo).NotNull();
    }
}
