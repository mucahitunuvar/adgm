using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §4.2 (Faz 1a Görev 3): a cover/detail/OG image reference must point at a MediaAsset that
// actually exists and is Kind == Image - checked here once, called by every command (ContentItem's
// cover/detail/OG image, and since Faz 1b Görev 2, Video's cover image) that accepts one of these
// fields, instead of a near-identical inline check per handler. entityName prefixes the resulting
// error code (e.g. "ContentItem.CoverImageNotFound", "Video.CoverImageNotFound") so it still reads
// correctly no matter which aggregate's command triggered it.
public static class MediaImageReferenceGuard
{
    public static async Task<Result> CheckAsync(
        Guid? mediaAssetId, string entityName, string fieldName, IMediaAssetRepository mediaAssetRepository, CancellationToken cancellationToken)
    {
        if (mediaAssetId is null)
        {
            return Result.Success();
        }

        var mediaAsset = await mediaAssetRepository.GetByIdAsync(mediaAssetId.Value, cancellationToken);
        if (mediaAsset is null)
        {
            return Result.Failure(Error.NotFound(
                $"{entityName}.{fieldName}NotFound", $"Media asset '{mediaAssetId}' could not be found."));
        }

        if (mediaAsset.Kind != MediaAssetKind.Image)
        {
            return Result.Failure(Error.Validation(
                $"{entityName}.{fieldName}NotImage", $"Media asset '{mediaAssetId}' is not an image."));
        }

        return Result.Success();
    }
}
