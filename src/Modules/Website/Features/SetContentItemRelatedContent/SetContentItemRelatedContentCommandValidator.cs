using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemRelatedContent;

public sealed class SetContentItemRelatedContentCommandValidator : AbstractValidator<SetContentItemRelatedContentCommand>
{
    public SetContentItemRelatedContentCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.RelatedContentItemIds).NotNull();
    }
}
