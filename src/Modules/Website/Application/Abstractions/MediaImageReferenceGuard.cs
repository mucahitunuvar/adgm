using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §4.2 (Faz 1a Görev 3): a cover/detail/OG image reference must point at a MediaAsset that
// actually exists and is Kind == Image - checked here once, called by every ContentItem command that
// accepts one of these three fields, instead of three near-identical inline checks per handler.
public static class MediaImageReferenceGuard
{
    public static async Task<Result> CheckAsync(
        Guid? mediaAssetId, string fieldName, IMediaAssetRepository mediaAssetRepository, CancellationToken cancellationToken)
    {
        if (mediaAssetId is null)
        {
            return Result.Success();
        }

        var mediaAsset = await mediaAssetRepository.GetByIdAsync(mediaAssetId.Value, cancellationToken);
        if (mediaAsset is null)
        {
            return Result.Failure(Error.NotFound(
                $"ContentItem.{fieldName}NotFound", $"Media asset '{mediaAssetId}' could not be found."));
        }

        if (mediaAsset.Kind != MediaAssetKind.Image)
        {
            return Result.Failure(Error.Validation(
                $"ContentItem.{fieldName}NotImage", $"Media asset '{mediaAssetId}' is not an image."));
        }

        return Result.Success();
    }
}
