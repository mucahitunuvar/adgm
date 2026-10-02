using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.ReplaceContentDraftBlocks;

public sealed class ReplaceContentDraftBlocksCommandValidator : AbstractValidator<ReplaceContentDraftBlocksCommand>
{
    public ReplaceContentDraftBlocksCommandValidator()
    {
        RuleFor(c => c.ContentItemId).NotEmpty();
        RuleFor(c => c.RowVersion).NotNull();
        RuleFor(c => c.Blocks).NotNull();

        RuleForEach(c => c.Blocks).ChildRules(block =>
        {
            block.RuleFor(b => b.BlockTypeKey).NotEmpty();
            block.RuleFor(b => b.SortOrder).GreaterThanOrEqualTo(0);
        });
    }
}
