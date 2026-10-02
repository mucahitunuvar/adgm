using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed class VideoFeatureBlockTypeDefinition : BlockTypeDefinition<VideoFeatureBlockSettings, VideoFeatureBlockTexts>
{
    public override string Key => "video-feature";

    public override IReadOnlyList<PageLayoutTargetKind> AllowedTargets => [PageLayoutTargetKind.Home, PageLayoutTargetKind.Content];

    protected override Result ValidateSettings(VideoFeatureBlockSettings settings)
    {
        if (settings.VideoId == Guid.Empty)
        {
            return Result.Failure(Error.Validation("video-feature.VideoIdRequired", "A video id is required."));
        }

        return Result.Success();
    }

    protected override Result ValidateTexts(VideoFeatureBlockSettings settings, VideoFeatureBlockTexts texts)
    {
        if (string.IsNullOrWhiteSpace(texts.Title))
        {
            return Result.Failure(Error.Validation("video-feature.TitleRequired", "A title is required."));
        }

        var linkResult = LinkTargetDtoMapper.ToLinkTarget(texts.Link);
        if (linkResult.IsFailure)
        {
            return Result.Failure(linkResult.Error);
        }

        // Mirrors Slide's "ButtonLabel varsa LinkTarget zorunlu" rule (§2): a link label with no link
        // target would render a button that goes nowhere.
        if (!string.IsNullOrEmpty(texts.LinkLabel) && linkResult.Value.IsEmpty)
        {
            return Result.Failure(Error.Validation("video-feature.LinkLabelRequiresLink", "A link label requires a link target."));
        }

        return Result.Success();
    }

    protected override BlockReferenceSet GetReferences(VideoFeatureBlockSettings settings, IReadOnlyList<VideoFeatureBlockTexts> texts)
    {
        var contentItemIds = new List<Guid>();
        var contentTypeListingIds = new List<Guid>();

        foreach (var text in texts)
        {
            LinkTargetDtoMapper.AppendReferences(text.Link, contentItemIds, contentTypeListingIds);
        }

        return BlockReferenceSet.Empty with
        {
            VideoIds = [settings.VideoId],
            ContentItemIds = contentItemIds,
            ContentTypeListingIds = contentTypeListingIds,
        };
    }
}
