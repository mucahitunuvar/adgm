using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.CreateVideo;

public sealed class CreateVideoCommandHandler(
    IVideoRepository videoRepository,
    ISiteLanguageRepository siteLanguageRepository,
    IMediaAssetRepository mediaAssetRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateVideoCommand, Result<CreateVideoResponse>>
{
    public async Task<Result<CreateVideoResponse>> Handle(CreateVideoCommand request, CancellationToken cancellationToken)
    {
        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure<CreateVideoResponse>(
                Error.Failure("Video.NoDefaultLanguage", "No default site language is configured."));
        }

        var coverImageCheck = await MediaImageReferenceGuard.CheckAsync(
            request.CoverImageMediaId, "Video", "CoverImage", mediaAssetRepository, cancellationToken);
        if (coverImageCheck.IsFailure)
        {
            return Result.Failure<CreateVideoResponse>(coverImageCheck.Error);
        }

        var videoResult = Video.Create(
            request.YouTubeUrl, request.CoverImageMediaId, request.SortOrder, defaultLanguage.Code,
            request.DefaultLanguageTitle, request.DefaultLanguageDescription,
            currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (videoResult.IsFailure)
        {
            return Result.Failure<CreateVideoResponse>(videoResult.Error);
        }

        videoRepository.Add(videoResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateVideoResponse(
            videoResult.Value.Id, videoResult.Value.YouTubeVideoId.Value, defaultLanguage.Code.Value));
    }
}
