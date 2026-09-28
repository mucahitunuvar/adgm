using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.CreateContentItem;

public sealed class CreateContentItemCommandValidator : AbstractValidator<CreateContentItemCommand>
{
    public CreateContentItemCommandValidator()
    {
        RuleFor(c => c.ContentTypeId).NotEmpty();
        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);
        RuleFor(c => c.DefaultLanguageTitle).NotEmpty();
        RuleFor(c => c.Seo).NotNull();
    }
}
