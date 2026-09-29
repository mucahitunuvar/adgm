using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §4.1 (Faz 1b Görev 4): a content item attachment's MediaAssetId must point at a MediaAsset
// that actually exists and is Kind == Document - the same one-place check MediaImageReferenceGuard
// already is for Kind == Image references, kept as a separate guard rather than a shared parameterized
// one so each entityName/fieldName pairing keeps reading as a direct sentence ("ContentItem.GalleryItemNotImage"
// vs. "ContentItem.AttachmentNotDocument").
public static class MediaDocumentReferenceGuard
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

        if (mediaAsset.Kind != MediaAssetKind.Document)
        {
            return Result.Failure(Error.Validation(
                $"{entityName}.{fieldName}NotDocument", $"Media asset '{mediaAssetId}' is not a document."));
        }

        return Result.Success();
    }
}
