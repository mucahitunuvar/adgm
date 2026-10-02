using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed class FeatureMosaicBlockTypeDefinition : BlockTypeDefinition<FeatureMosaicBlockSettings, FeatureMosaicBlockTexts>
{
    public override string Key => "feature-mosaic";

    public override IReadOnlyList<PageLayoutTargetKind> AllowedTargets => [PageLayoutTargetKind.Home, PageLayoutTargetKind.Content];

    protected override Result ValidateSettings(FeatureMosaicBlockSettings settings)
    {
        if (settings.Items.Count is < 1 or > 5)
        {
            return Result.Failure(Error.Validation("feature-mosaic.ItemCountInvalid", "feature-mosaic must have between 1 and 5 items."));
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
                return Result.Failure(Error.Validation("feature-mosaic.LinkRequired", "Each feature-mosaic item requires a link."));
            }
        }

        return Result.Success();
    }

    protected override Result ValidateTexts(FeatureMosaicBlockSettings settings, FeatureMosaicBlockTexts texts)
    {
        if (texts.Items.Count != settings.Items.Count)
        {
            return Result.Failure(Error.Validation(
                "feature-mosaic.ItemCountMismatch", "feature-mosaic's texts must have the same number of items as its settings."));
        }

        return Result.Success();
    }

    protected override BlockReferenceSet GetReferences(FeatureMosaicBlockSettings settings, IReadOnlyList<FeatureMosaicBlockTexts> texts)
    {
        var imageMediaIds = settings.Items.Where(i => i.ImageMediaId is not null).Select(i => i.ImageMediaId!.Value).ToList();
        var contentItemIds = new List<Guid>();
        var contentTypeListingIds = new List<Guid>();

        foreach (var item in settings.Items)
        {
            LinkTargetDtoMapper.AppendReferences(item.Link, contentItemIds, contentTypeListingIds);
        }

        return BlockReferenceSet.Empty with
        {
            ImageMediaIds = imageMediaIds,
            ContentItemIds = contentItemIds,
            ContentTypeListingIds = contentTypeListingIds,
        };
    }
}
