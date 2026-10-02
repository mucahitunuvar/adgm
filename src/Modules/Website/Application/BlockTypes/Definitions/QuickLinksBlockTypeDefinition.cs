using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed class QuickLinksBlockTypeDefinition : BlockTypeDefinition<QuickLinksBlockSettings, QuickLinksBlockTexts>
{
    public override string Key => "quick-links";

    public override IReadOnlyList<PageLayoutTargetKind> AllowedTargets => [PageLayoutTargetKind.Home, PageLayoutTargetKind.Content];

    protected override Result ValidateSettings(QuickLinksBlockSettings settings)
    {
        if (settings.Items.Count is < 1 or > 8)
        {
            return Result.Failure(Error.Validation("quick-links.ItemCountInvalid", "quick-links must have between 1 and 8 items."));
        }

        foreach (var item in settings.Items)
        {
            var linkResult = LinkTargetDtoMapper.ToLinkTarget(item.Link);
            if (linkResult.IsFailure)
            {
                return Result.Failure(linkResult.Error);
            }

            if (linkResult.Value.IsEmpty)
            {
                return Result.Failure(Error.Validation("quick-links.LinkRequired", "Each quick-links item requires a link."));
            }
        }

        return Result.Success();
    }

    protected override Result ValidateTexts(QuickLinksBlockSettings settings, QuickLinksBlockTexts texts)
    {
        if (texts.Items.Count != settings.Items.Count)
        {
            return Result.Failure(Error.Validation(
                "quick-links.ItemCountMismatch", "quick-links' texts must have the same number of items as its settings."));
        }

        return Result.Success();
    }

    protected override BlockReferenceSet GetReferences(QuickLinksBlockSettings settings, IReadOnlyList<QuickLinksBlockTexts> texts)
    {
        var contentItemIds = new List<Guid>();
        var contentTypeListingIds = new List<Guid>();

        foreach (var item in settings.Items)
        {
            LinkTargetDtoMapper.AppendReferences(item.Link, contentItemIds, contentTypeListingIds);
        }

        return BlockReferenceSet.Empty with { ContentItemIds = contentItemIds, ContentTypeListingIds = contentTypeListingIds };
    }
}
