using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.DeleteContentItem;

public sealed class DeleteContentItemCommandValidator : AbstractValidator<DeleteContentItemCommand>
{
    public DeleteContentItemCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
