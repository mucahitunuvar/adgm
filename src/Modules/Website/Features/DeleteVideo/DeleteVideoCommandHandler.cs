using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeleteVideo;

public sealed class DeleteVideoCommandHandler(
    IVideoRepository videoRepository,
    IVideoUsageChecker videoUsageChecker,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteVideoCommand, Result>
{
    public async Task<Result> Handle(DeleteVideoCommand request, CancellationToken cancellationToken)
    {
        var video = await videoRepository.GetByIdAsync(request.Id, cancellationToken);
        if (video is null)
        {
            return Result.Failure(Error.NotFound("Video.NotFound", $"Video '{request.Id}' could not be found."));
        }

        var usages = await videoUsageChecker.GetUsagesAsync(video.Id, cancellationToken);
        if (usages.Count > 0)
        {
            var usageDescriptions = string.Join(", ", usages.Select(u => u.Description));
            return Result.Failure(Error.Conflict(
                "Video.InUse", $"This video is in use and cannot be deleted: {usageDescriptions}."));
        }

        videoRepository.Remove(video);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidatePublicContent(cacheService);

        return Result.Success();
    }
}
