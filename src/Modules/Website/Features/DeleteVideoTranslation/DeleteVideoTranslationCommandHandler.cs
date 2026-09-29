using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeleteVideoTranslation;

public sealed class DeleteVideoTranslationCommandHandler(
    IVideoRepository videoRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteVideoTranslationCommand, Result>
{
    public async Task<Result> Handle(DeleteVideoTranslationCommand request, CancellationToken cancellationToken)
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

        var languageCodeResult = LanguageCode.Create(request.LanguageCode);
        if (languageCodeResult.IsFailure)
        {
            return languageCodeResult;
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure(Error.Failure("Video.NoDefaultLanguage", "No default site language is configured."));
        }

        var removeResult = video.RemoveTranslation(
            languageCodeResult.Value, defaultLanguage.Code, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (removeResult.IsFailure)
        {
            return removeResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
