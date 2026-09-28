using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentType;

public sealed class UpdateContentTypeCommandValidator : AbstractValidator<UpdateContentTypeCommand>
{
    public UpdateContentTypeCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.ListTemplate).NotEmpty();
        RuleFor(c => c.DetailTemplate).NotEmpty();
        RuleFor(c => c.SortMode).NotEmpty();
        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);
    }
}
