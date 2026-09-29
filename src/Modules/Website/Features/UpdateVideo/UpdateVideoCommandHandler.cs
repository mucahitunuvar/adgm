using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateVideo;

public sealed class UpdateVideoCommandHandler(
    IVideoRepository videoRepository,
    IMediaAssetRepository mediaAssetRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateVideoCommand, Result>
{
    public async Task<Result> Handle(UpdateVideoCommand request, CancellationToken cancellationToken)
    {
        var video = await videoRepository.GetByIdAsync(request.Id, cancellationToken);
        if (video is null)
        {
            return Result.Failure(Error.NotFound("Video.NotFound", $"Video '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(video.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "Video.ConcurrencyConflict", "The video was changed by someone else. Reload and try again."));
        }

        var coverImageCheck = await MediaImageReferenceGuard.CheckAsync(
            request.CoverImageMediaId, "Video", "CoverImage", mediaAssetRepository, cancellationToken);
        if (coverImageCheck.IsFailure)
        {
            return coverImageCheck;
        }

        var updateResult = video.Update(
            request.YouTubeUrl, request.CoverImageMediaId, request.SortOrder,
            currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
