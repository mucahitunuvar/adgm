using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.ReplaceHomeDraftBlocks;

public sealed class ReplaceHomeDraftBlocksCommandValidator : AbstractValidator<ReplaceHomeDraftBlocksCommand>
{
    public ReplaceHomeDraftBlocksCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotNull();
        RuleFor(c => c.Blocks).NotNull();

        RuleForEach(c => c.Blocks).ChildRules(block =>
        {
            block.RuleFor(b => b.BlockTypeKey).NotEmpty();
            block.RuleFor(b => b.SortOrder).GreaterThanOrEqualTo(0);
        });
    }
}
