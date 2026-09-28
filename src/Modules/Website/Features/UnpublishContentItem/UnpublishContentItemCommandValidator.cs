using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UnpublishContentItem;

public sealed class UnpublishContentItemCommandValidator : AbstractValidator<UnpublishContentItemCommand>
{
    public UnpublishContentItemCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
