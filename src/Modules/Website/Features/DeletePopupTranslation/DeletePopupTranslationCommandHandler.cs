using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeletePopupTranslation;

public sealed class DeletePopupTranslationCommandHandler(
    IPopupRepository popupRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePopupTranslationCommand, Result>
{
    public async Task<Result> Handle(DeletePopupTranslationCommand request, CancellationToken cancellationToken)
    {
        var popup = await popupRepository.GetByIdAsync(request.Id, cancellationToken);
        if (popup is null)
        {
            return Result.Failure(Error.NotFound("Popup.NotFound", $"Popup '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(popup.RowVersion))
        {
            return Result.Failure(Error.Conflict("Popup.ConcurrencyConflict", "The popup was changed by someone else. Reload and try again."));
        }

        var languageCodeResult = LanguageCode.Create(request.LanguageCode);
        if (languageCodeResult.IsFailure)
        {
            return languageCodeResult;
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure(Error.Failure("Popup.NoDefaultLanguage", "No default site language is configured."));
        }

        var removeResult = popup.RemoveTranslation(
            languageCodeResult.Value, defaultLanguage.Code, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (removeResult.IsFailure)
        {
            return removeResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
