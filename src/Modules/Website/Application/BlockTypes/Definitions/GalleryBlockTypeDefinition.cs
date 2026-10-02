using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed class GalleryBlockTypeDefinition : BlockTypeDefinition<GalleryBlockSettings, GalleryBlockTexts>
{
    public override string Key => "gallery";

    public override IReadOnlyList<PageLayoutTargetKind> AllowedTargets => [PageLayoutTargetKind.Home, PageLayoutTargetKind.Content];

    protected override Result ValidateSettings(GalleryBlockSettings settings)
    {
        if (settings.MediaIds.Count is < 1 or > 30)
        {
            return Result.Failure(Error.Validation("gallery.MediaIdCountInvalid", "gallery must have between 1 and 30 images."));
        }

        if (settings.MediaIds.Distinct().Count() != settings.MediaIds.Count)
        {
            return Result.Failure(Error.Validation("gallery.DuplicateMediaId", "A media id appears more than once."));
        }

        return Result.Success();
    }

    protected override Result ValidateTexts(GalleryBlockSettings settings, GalleryBlockTexts texts) => Result.Success();

    protected override BlockReferenceSet GetReferences(GalleryBlockSettings settings, IReadOnlyList<GalleryBlockTexts> texts) =>
        BlockReferenceSet.Empty with { ImageMediaIds = settings.MediaIds };
}
