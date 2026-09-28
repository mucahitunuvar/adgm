using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentItem;

public sealed class UpdateContentItemCommandValidator : AbstractValidator<UpdateContentItemCommand>
{
    public UpdateContentItemCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);
    }
}
