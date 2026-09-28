using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.CreateContentType;

public sealed class CreateContentTypeCommandValidator : AbstractValidator<CreateContentTypeCommand>
{
    public CreateContentTypeCommandValidator()
    {
        RuleFor(c => c.Key).NotEmpty();
        RuleFor(c => c.ListTemplate).NotEmpty();
        RuleFor(c => c.DetailTemplate).NotEmpty();
        RuleFor(c => c.SortMode).NotEmpty();
        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);
        RuleFor(c => c.DefaultLanguageName).NotEmpty();
        RuleFor(c => c.Seo).NotNull();
    }
}
