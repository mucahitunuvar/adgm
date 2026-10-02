using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed class ImageTextBlockTypeDefinition(IHtmlContentSanitizer htmlContentSanitizer)
    : BlockTypeDefinition<ImageTextBlockSettings, ImageTextBlockTexts>(htmlContentSanitizer)
{
    private static readonly string[] AllowedPositions = ["left", "right"];

    public override string Key => "image-text";

    public override IReadOnlyList<PageLayoutTargetKind> AllowedTargets => [PageLayoutTargetKind.Home, PageLayoutTargetKind.Content];

    protected override Result ValidateSettings(ImageTextBlockSettings settings)
    {
        if (settings.ImageMediaId == Guid.Empty)
        {
            return Result.Failure(Error.Validation("image-text.ImageMediaIdRequired", "An image media id is required."));
        }

        if (!AllowedPositions.Contains(settings.ImagePosition, StringComparer.Ordinal))
        {
            return Result.Failure(Error.Validation("image-text.ImagePositionInvalid", "ImagePosition must be 'left' or 'right'."));
        }

        if (settings.Link is not null)
        {
            var linkResult = LinkTargetDtoMapper.ToLinkTarget(settings.Link);
            if (linkResult.IsFailure)
            {
                return Result.Failure(linkResult.Error);
            }
        }

        return Result.Success();
    }

    protected override Result ValidateTexts(ImageTextBlockSettings settings, ImageTextBlockTexts texts)
    {
        if (string.IsNullOrWhiteSpace(texts.Title))
        {
            return Result.Failure(Error.Validation("image-text.TitleRequired", "A title is required."));
        }

        if (string.IsNullOrWhiteSpace(texts.Body))
        {
            return Result.Failure(Error.Validation("image-text.BodyRequired", "A body is required."));
        }

        return Result.Success();
    }

    protected override ImageTextBlockTexts SanitizeTexts(ImageTextBlockTexts texts) =>
        texts with { Body = HtmlSanitizer!.Sanitize(texts.Body) };

    protected override BlockReferenceSet GetReferences(ImageTextBlockSettings settings, IReadOnlyList<ImageTextBlockTexts> texts)
    {
        var contentItemIds = new List<Guid>();
        var contentTypeListingIds = new List<Guid>();
        LinkTargetDtoMapper.AppendReferences(settings.Link, contentItemIds, contentTypeListingIds);

        return BlockReferenceSet.Empty with
        {
            ImageMediaIds = [settings.ImageMediaId],
            ContentItemIds = contentItemIds,
            ContentTypeListingIds = contentTypeListingIds,
        };
    }
}
