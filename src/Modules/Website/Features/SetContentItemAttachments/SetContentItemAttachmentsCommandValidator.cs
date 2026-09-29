using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemAttachments;

public sealed class SetContentItemAttachmentsCommandValidator : AbstractValidator<SetContentItemAttachmentsCommand>
{
    public SetContentItemAttachmentsCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.Items).NotNull();
        RuleForEach(c => c.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.MediaAssetId).NotEmpty();
            item.RuleFor(i => i.SortOrder).GreaterThanOrEqualTo(0);
            item.RuleFor(i => i.Translations).NotNull();
        });
    }
}
