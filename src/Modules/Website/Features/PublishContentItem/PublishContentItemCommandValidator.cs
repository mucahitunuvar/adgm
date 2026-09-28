using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.PublishContentItem;

public sealed class PublishContentItemCommandValidator : AbstractValidator<PublishContentItemCommand>
{
    public PublishContentItemCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
