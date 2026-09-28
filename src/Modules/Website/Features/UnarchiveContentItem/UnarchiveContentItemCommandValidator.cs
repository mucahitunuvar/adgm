using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UnarchiveContentItem;

public sealed class UnarchiveContentItemCommandValidator : AbstractValidator<UnarchiveContentItemCommand>
{
    public UnarchiveContentItemCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
