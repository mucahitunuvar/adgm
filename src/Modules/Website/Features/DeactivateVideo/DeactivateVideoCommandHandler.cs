using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateVideo;

public sealed class DeactivateVideoCommandHandler(
    IVideoRepository videoRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivateVideoCommand, Result>
{
    public async Task<Result> Handle(DeactivateVideoCommand request, CancellationToken cancellationToken)
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

        var deactivateResult = video.Deactivate(currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (deactivateResult.IsFailure)
        {
            return deactivateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
