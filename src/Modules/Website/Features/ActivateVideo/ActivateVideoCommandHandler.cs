using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.ActivateVideo;

public sealed class ActivateVideoCommandHandler(
    IVideoRepository videoRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ActivateVideoCommand, Result>
{
    public async Task<Result> Handle(ActivateVideoCommand request, CancellationToken cancellationToken)
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

        var activateResult = video.Activate(currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (activateResult.IsFailure)
        {
            return activateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidatePublicContent(cacheService);

        return Result.Success();
    }
}
