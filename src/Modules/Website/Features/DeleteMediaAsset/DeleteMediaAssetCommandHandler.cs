using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenclikMerkezi.Modules.Website.Features.DeleteMediaAsset;

public sealed class DeleteMediaAssetCommandHandler(
    IMediaAssetRepository mediaAssetRepository,
    IMediaUsageChecker mediaUsageChecker,
    IFileStorageService fileStorageService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork,
    ILogger<DeleteMediaAssetCommandHandler> logger)
    : IRequestHandler<DeleteMediaAssetCommand, Result>
{
    public async Task<Result> Handle(DeleteMediaAssetCommand request, CancellationToken cancellationToken)
    {
        var mediaAsset = await mediaAssetRepository.GetByIdAsync(request.Id, cancellationToken);
        if (mediaAsset is null)
        {
            return Result.Failure(Error.NotFound("MediaAsset.NotFound", "The specified media asset could not be found."));
        }

        var usages = await mediaUsageChecker.GetUsagesAsync(mediaAsset.Id, cancellationToken);
        if (usages.Count > 0)
        {
            var usageDescriptions = string.Join(", ", usages.Select(u => u.Description));
            return Result.Failure(Error.Conflict(
                "MediaAsset.InUse", $"This media asset is in use and cannot be deleted: {usageDescriptions}."));
        }

        // ADR-024 §6: DB row is removed in this Unit of Work; the physical files are deleted only
        // after that commit succeeds, and a file-deletion failure is logged, not surfaced as a
        // command failure (the DB is already the source of truth once committed).
        mediaAssetRepository.Remove(mediaAsset);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var fileKeys = new List<string> { mediaAsset.Original.FileKey };
        fileKeys.AddRange(mediaAsset.Variants.Select(v => v.File.FileKey));

        foreach (var fileKey in fileKeys)
        {
            try
            {
                await fileStorageService.DeleteAsync(fileKey, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception, "Failed to delete file {FileKey} for deleted MediaAsset {MediaAssetId}.", fileKey, mediaAsset.Id);
            }
        }

        return Result.Success();
    }
}
