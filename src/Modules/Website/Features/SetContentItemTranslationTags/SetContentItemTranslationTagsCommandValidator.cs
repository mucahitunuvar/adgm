using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemTranslationTags;

public sealed class SetContentItemTranslationTagsCommandValidator : AbstractValidator<SetContentItemTranslationTagsCommand>
{
    public SetContentItemTranslationTagsCommandValidator()
    {
        RuleFor(c => c.LanguageCode).NotEmpty();
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.TagNames).NotNull();
    }
}
